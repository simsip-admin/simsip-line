using Cocos2D;
using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;
using System;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using ConversionHelper;
using BEPUphysics.Entities.Prefabs;
using Engine.Input;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Effects.Stock;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#else
using System.Threading;
#endif
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
using Simsip.LineRunner.GameObjects.Lines;
using System.Collections.Generic;
#else
using System.Collections.Concurrent;
using Simsip.LineRunner.GameObjects.Lines;
#endif


namespace Simsip.LineRunner.GameObjects.Pages
{
    public class PageCache : GameComponent, IPageCache
    {
        // Magic numbers
        private const float DEFAULT_PAGE_DEPTH_FROM_CAMERA = 4f;
        private const float DEFAULT_CHARACTER_DEPTH_FROM_PAGE = 0.4f;
        private const float DEFAULT_PANE_DEPTH_FROM_PAGE = 2f;

        // State we maintain
        private int _currentPageNumber;
        private int _currentLineNumber;
        private GameState _currentGameState;

        // Required services
        private IInputManager _inputManager;
        private IPhysicsManager _physicsManager;
        private ILineCache _lineCache;
        private IPadRepository _padRepository;
        
        // Determines what gets asked to be drawn
        private OcTreeNode _ocTreeRoot;

        // Custom content manager so we can reload a page model with different textures
        // private CustomContentManager _customContentManager;

        // Support for staging the results of asynchronous loads and then signaling
        // we need the results processed on the next update cycle
        private class LoadContentThreadArgs
        {
            public LoadContentAsyncType TheLoadContentAsyncType;
            public int PageNumber;
            public PageModel PageModelAsync;
        }
        private ConcurrentQueue<LoadContentThreadArgs> _loadContentThreadResults;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public PageCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IPageCache), this); 
        }

        #region GameComponent Overrides

        public override void Initialize()
        {
            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;
            this._loadContentThreadResults = new ConcurrentQueue<LoadContentThreadArgs>(); 
            
            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._physicsManager = (IPhysicsManager)this.Game.Services.GetService(typeof(IPhysicsManager));
            this._lineCache = (ILineCache)this.Game.Services.GetService(typeof(ILineCache));
            this._padRepository = new PadRepository();

            // Initialize our octree which will handle determination if we should draw or not
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            // Load definition for CurrentPageModel and add to octree
            // IMPORTANT: Note that this is different than other GameObjects as we want on
            // startup to get our positioning coordinates completed via CalculateWorldCoordinates()
            // 
            this.InitCurrentPageModel();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Did we signal we need an async content load processed?
            if (this._loadContentThreadResults.Count > 0)
            {
                LoadContentThreadArgs loadContentThreadArgs = null;
                if (this._loadContentThreadResults.TryDequeue(out loadContentThreadArgs))
                {
                    // Load in new content from a staged collection in args
                    ProcessLoadContentAsync(loadContentThreadArgs);

                    // Does anyone need to know we finished an async load?
                    if (LoadContentAsyncFinished != null)
                    {
                        var eventArgs = new LoadContentAsyncFinishedEventArgs(loadContentThreadArgs.TheLoadContentAsyncType);
                        LoadContentAsyncFinished(this, eventArgs);
                    }
                }
            }
        }

        #endregion

        #region IPageCache Implementation

        public event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        public void Draw(StockBasicEffect effect, EffectType type)
        {
            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            var frustrum = new BoundingFrustum(view * projection);
            _ocTreeRoot.Draw(view, projection, frustrum, effect, type);
        }

        public PageModel CurrentPageModel { get; private set; }

        public float PageDepthFromCameraStart { get { return DEFAULT_PAGE_DEPTH_FROM_CAMERA; } }

        public float CharacterDepthFromCameraStart { get { return PageDepthFromCameraStart - DEFAULT_CHARACTER_DEPTH_FROM_PAGE; } }

        public float PaneDepthFromCameraStart { get { return PageDepthFromCameraStart - DEFAULT_PANE_DEPTH_FROM_PAGE; } }

        public Vector3 StationaryCameraOriginalWorldPosition { get; private set; }

        public Vector3 StationaryCameraOriginalWorldTarget { get; private set; }

        public void CalculateWorldCoordinates()
        {
            // Grab these right away so as we progress through game we have a well defined 
            // camera position/target to do all our game component positioning from
            this.StationaryCameraOriginalWorldPosition = this._inputManager.TheStationaryCamera.Position;
            this.StationaryCameraOriginalWorldTarget = this._inputManager.TheStationaryCamera.Target;

            // We can get our translation defined right away as well
            var pagePosition = this._inputManager.TheStationaryCamera.Position +
                               new Vector3(0, 0, -this.PageDepthFromCameraStart);
            var translateMatrix = Matrix.CreateTranslation(pagePosition);

            // Our logical positioning coordinates
            var cameraOriginScreenPoint = new CCPoint(                      // The camera in the center of the screen
                0.5f * CCDrawManager.DesignResolutionSize.Width, 
                0.5f * CCDrawManager.DesignResolutionSize.Height);                    
            this.CurrentPageModel.LogicalStartOrigin = new CCPoint(         // We start our hero in the center of the screen
                0.5f * CCDrawManager.DesignResolutionSize.Width, 
                0.5f * CCDrawManager.DesignResolutionSize.Height);
            this.CurrentPageModel.LogicalLineSpacingTop = new CCPoint(      // We want the line margin to fill most of the screen with a little border on top/bottom
                0.5f  * CCDrawManager.DesignResolutionSize.Width,
                0.75f * CCDrawManager.DesignResolutionSize.Height);    
            this.CurrentPageModel.LogicalLineSpacingBottom = new CCPoint(
                0.5f  * CCDrawManager.DesignResolutionSize.Width,
                0.25f * CCDrawManager.DesignResolutionSize.Height);    

            // First, determine the unscaled line margin (distance between each line)
            var modelPageBody = this.CurrentPageModel.TheModelEntity.ModelHeight -
                                (this.CurrentPageModel.ThePadEntity.ModelHeaderMargin + this.CurrentPageModel.ThePadEntity.ModelFooterMargin); 
            var modelLineMargin = modelPageBody / (this.CurrentPageModel.ThePadEntity.LineCount);

            // Now, determine a scale matrix to use for our page model so it ends up a desired logical size on the screen
            // We are using the line margin here so that we will see both the top and bottom line margins on our display
            var scaleMatrix = XNAUtils.ScaleModel(this.CurrentPageModel.LogicalLineSpacingBottom,
                                                  this.CurrentPageModel.LogicalLineSpacingTop,
                                                  this.PageDepthFromCameraStart,
                                                  modelLineMargin,
                                                  XNAUtils.ScaleModelBy.Height,
                                                  XNAUtils.CameraType.Stationary);

            // Now get our new world dimensions
            Vector3 scale = new Vector3();
            Quaternion rot = new Quaternion();
            Vector3 trans = new Vector3();
            scaleMatrix.Decompose(out scale, out rot, out trans);
            this.CurrentPageModel.ModelToWorldRatio = scale.X;
            this.CurrentPageModel.WorldWidth = this.CurrentPageModel.TheModelEntity.ModelWidth * scale.X;
            this.CurrentPageModel.WorldHeight = this.CurrentPageModel.TheModelEntity.ModelHeight * scale.Y;
            this.CurrentPageModel.WorldDepth = this.CurrentPageModel.TheModelEntity.ModelDepth * scale.Z;
            this.CurrentPageModel.WorldHeaderMargin = this.CurrentPageModel.ThePadEntity.ModelHeaderMargin * scale.Y;
            this.CurrentPageModel.WorldFooterMargin = this.CurrentPageModel.ThePadEntity.ModelFooterMargin * scale.Y;
            this.CurrentPageModel.WorldLineSpacing = modelLineMargin * scale.Y;

            // Now calculate our starting positions
            var trackingCameraPosition = new Vector3(
                    this._inputManager.LineRunnerCamera.Position.X + this.CurrentPageModel.ThePadEntity.ModelStartX * scale.X,
                    this._inputManager.LineRunnerCamera.Position.Y + this.CurrentPageModel.ThePadEntity.ModelStartY * scale.Y,
                    this._inputManager.LineRunnerCamera.Position.Z);
            var trackingCameraTarget = trackingCameraPosition + new Vector3(0, 0, -this.PageDepthFromCameraStart);
            this._inputManager.LineRunnerCamera.Position = trackingCameraPosition;
            this._inputManager.LineRunnerCamera.Target = trackingCameraTarget;

            this.CurrentPageModel.PageStartOrigin = trackingCameraPosition + new Vector3(0, 0, -this.PageDepthFromCameraStart); ;
            this.CurrentPageModel.HeroStartOrigin = trackingCameraPosition + new Vector3(0, 0, -this.CharacterDepthFromCameraStart);
            
            // Adjust world matrix references accordingly with our newly constructed scale and translate matrixes
            // (Remember, order of multiplication is important here)
            this.CurrentPageModel.WorldMatrix = scaleMatrix * translateMatrix;

            // Update our drawing filter with our new position
            this._ocTreeRoot.UpdateModelWorldMatrix(this.CurrentPageModel.ModelID,
                                                    this.CurrentPageModel.WorldMatrix);

            // Create pad as a physics box body
            // IMPORTANT: Adjusting for physics representation with origin in middle
            var physicsLocalTransform = new BEPUutilities.Vector3(
                (0.5f * this.CurrentPageModel.WorldWidth),
                (0.5f * this.CurrentPageModel.WorldHeight),
                -(0.5f * this.CurrentPageModel.WorldDepth));
            this.CurrentPageModel.PhysicsLocalTransform = BEPUutilities.Matrix.CreateTranslation(physicsLocalTransform);

            // Create physics box to represent pad and add to physics space
            var padPhysicsOrigin = ConversionHelper.MathConverter.Convert(this.CurrentPageModel.WorldOrigin) + physicsLocalTransform;
            var padPhysicsBox = new Box(
                padPhysicsOrigin, 
                this.CurrentPageModel.WorldWidth, 
                this.CurrentPageModel.WorldHeight, 
                this.CurrentPageModel.WorldDepth);
            this.CurrentPageModel.PhysicsEntity = padPhysicsBox;
            padPhysicsBox.Tag = this.CurrentPageModel;
            this._physicsManager.TheSpace.Add(padPhysicsBox);
        }

        /// <summary>
        /// IMPORTANT: Do not call on background thread. Only call during normal Update/Draw cycle. 
        /// </summary>
        /// <param name="state"></param>
        public void SwitchState(GameState state)
        {
            // Update our overall game state
            this._currentGameState = state;

            switch (this._currentGameState)
            {
                case GameState.Intro:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // IMPORTANT: No background loading but we do propogate the state to the next
                        // dependent game object
                        this._lineCache.SwitchState(this._currentGameState);

                        break;
                    }
                case GameState.Moving:
                    {
                        // Propogate state change
                        this._lineCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        // Set state
                        this._currentLineNumber++;

                        // Propogate state change
                        this._lineCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Propogate state change
                        this._lineCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Propogate state change
                        this._lineCache.SwitchState(state);

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Remaining refresh logic centralized in helper
                        // this.Refresh();
                        // In background get initial lines constructed for first page.
                        // this.ProcessNextLine() will be called in event handler when
                        // finished to animate header and first line for first page into position
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Refresh);

                        break;
                    }
                case GameState.Start:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Propogate state change
                        this._lineCache.SwitchState(state);

                        break;
                    }
            }
        }

        #endregion

        #region Event Handlers

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            this.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

            this._lineCache.SwitchState(this._currentGameState);
        }

        #endregion

        #region Helper methods

         // IMPORTANT: Do not call on background thread. Only call during normal Update/Draw cycle.
        // (e.g., via SwitchState())
        private void LoadContentAsync(LoadContentAsyncType loadContentAsyncType)
        {
            // Build our state object for this background content load request
            var loadContentThreadArgs = new LoadContentThreadArgs
            {
                TheLoadContentAsyncType = loadContentAsyncType,
                PageNumber = 1
            };

#if NETFX_CORE

            IAsyncAction asyncAction = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        LoadContentAsyncThread(loadContentThreadArgs);
                    });
#else
            ThreadPool.QueueUserWorkItem(LoadContentAsyncThread, loadContentThreadArgs);
#endif

        }

        private void LoadContentAsyncThread(object args)
        {
            var loadContentThreadArgs = args as LoadContentThreadArgs;
            var loadContentAsyncType = loadContentThreadArgs.TheLoadContentAsyncType;
            var pageNumber = loadContentThreadArgs.PageNumber;
            var pageModel = loadContentThreadArgs.PageModelAsync;

            // Get our current page definition ready, will be needed for positioning camera
            var currentPad = UserDefaults.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_PAD,
                GameConstants.USER_DEFAULT_INITIAL_CURRENT_PAD);
            var padEntity = this._padRepository.GetPad(currentPad);

            // Load a fresh or cached version of our page model
            var customContentManager = new CustomContentManager(
               TheGame.SharedGame.Services,
               TheGame.SharedGame.Content.RootDirectory);
            pageModel = new PageModel(
                padEntity: padEntity,
                customContentManager: customContentManager,
                allowCached: true);

            // We only have 1 pad at a time to worry about
            pageModel.ModelID = 1;
            
            this._loadContentThreadResults.Enqueue(loadContentThreadArgs);
        }

        // Migrate staged collection in args to public collection
        private void ProcessLoadContentAsync(LoadContentThreadArgs loadContentThreadArgs)
        {
            this._ocTreeRoot.RemoveModel(this.CurrentPageModel.ModelID);

            if (this.CurrentPageModel.PhysicsEntity != null &&
                this.CurrentPageModel.PhysicsEntity.Space != null)
            {
                this._physicsManager.TheSpace.Remove(this.CurrentPageModel.PhysicsEntity);
            }

            // Clear out any previous line model textures
            GameModel.OriginalEffectsDictionary.Remove(this.CurrentPageModel.ThePadEntity.ModelName);

            // Now we are safe to call call Unload on our custom ContentManager.
            this.CurrentPageModel.TheCustomContentManager.Unload();

            this.CurrentPageModel = loadContentThreadArgs.PageModelAsync;

            this._ocTreeRoot.AddModel(this.CurrentPageModel);

            if (this.CurrentPageModel.PhysicsEntity != null)
            {
                this._physicsManager.TheSpace.Add(this.CurrentPageModel.PhysicsEntity);
            }

            // Refresh pad into world, 
            // will cause CalculateWorldCoordinates to be called again
            this._inputManager.ThePlayerControllerInput.RefreshPad();
        }

        private void InitCurrentPageModel(bool unloadPreviousPageModel=false)
        {
            // Get our current page definition ready, will be needed for positioning camera
            var currentPad = UserDefaults.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_PAD,
                GameConstants.USER_DEFAULT_INITIAL_CURRENT_PAD);
            var padEntity = this._padRepository.GetPad(currentPad);

            // Load a fresh or cached version of our page model
            // We use our own CustomContentManager scoped to this cache so that
            // we can reload a page model with different textures
             var customContentManager = new CustomContentManager(
                TheGame.SharedGame.Services,
                TheGame.SharedGame.Content.RootDirectory);
            this.CurrentPageModel = new PageModel(
                padEntity: padEntity,
                customContentManager: customContentManager,
                allowCached: true);

            // We only have 1 pad at a time to worry about
            this.CurrentPageModel.ModelID = 1;  
            this._ocTreeRoot.AddModel(CurrentPageModel);
        }

        #endregion

        #region Legacy

        private void Refresh()
        {
            // Remove physics model from physics space
            if (this.CurrentPageModel.PhysicsEntity != null &&
                this.CurrentPageModel.PhysicsEntity.Space != null)
            {
                this._physicsManager.TheSpace.Remove(this.CurrentPageModel.PhysicsEntity);
            }

            // Clean out previous pad model
            this._ocTreeRoot.RemoveModel(this.CurrentPageModel.ModelID);

            // Load definition for CurrentPageModel and add to octree
            // IMPORTANT: Note how we override the default paramter of unloadPreviousPageModel to
            //            true. This will allow us to get a fresh copy of the page model.
            this.InitCurrentPageModel(unloadPreviousPageModel: true);

            // Refresh pad into world, 
            // will cause CalculateWorldCoordinates to be called again
            this._inputManager.ThePlayerControllerInput.RefreshPad();
        }

        #endregion

    }
}