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


namespace Simsip.LineRunner.GameObjects.Pages
{
    public class PageCache : DrawableGameComponent, IPageCache
    {
        // State we maintain
        private int _currentPageNumber;
        private int _currentLineNumber;
        private GameState _currentGameState;

        // Required services
        private IInputManager _inputManager;
        private IPhysicsManager _physicsManager;
        private IPadRepository _padRepository;
        
        // Determines what gets asked to be drawn
        private OcTreeNode _ocTreeRoot;

        // Custom content manager so we can reload a page model with different textures
        private CustomContentManager _customContentManager;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public PageCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IPageCache), this); 
        }

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;
            
            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._physicsManager = (IPhysicsManager)this.Game.Services.GetService(typeof(IPhysicsManager));
            this._padRepository = new PadRepository();

            // Our depth value can be set right away
            // All other depths (hero, obstacles, etc), will be based off of this
            this.PageDepthFromCameraStart = 4f;

            // Initialize our octree which will handle determination if we should draw or not
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            // We use our own CustomContentManager scoped to this cache so that
            // we can reload a page model with different textures
            this._customContentManager = new CustomContentManager(
                TheGame.SharedGame.Services,
                TheGame.SharedGame.Content.RootDirectory);

            // Load definition for CurrentPageModel and add to octree
            this.InitCurrentPageModel();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Currently a no-op as content is dynamically loaded in Process()
        }

        public override void Update(GameTime gameTime)
        {
            // Currently a no-op
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }

        #endregion

        #region IPageCache Implementation

        public void Draw(Effect effect = null, EffectType type = EffectType.None)
        {
            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            var frustrum = new BoundingFrustum(view * projection);
            _ocTreeRoot.Draw(view, projection, frustrum, effect, type);
        }

        public PageModel CurrentPageModel { get; private set; }

        public float PageDepthFromCameraStart { get; private set; }

        public float PageDepthFromCharacter { get { return PageDepthFromCameraStart - CharacterDepthFromCameraStart; } }

        public float PageDepthFromObstacle { get { return PageDepthFromCameraStart - ObstacleDepthFromCameraStart; } }

        public float PageDepthFromPane { get { return PageDepthFromCameraStart - PaneDepthFromCameraStart; } }

        public float CharacterDepthFromCameraStart { get { return PageDepthFromCameraStart - 0.4f; } }

        public float ObstacleDepthFromCameraStart { get { return PageDepthFromCameraStart - 1; } }

        public float PaneDepthFromCameraStart { get { return PageDepthFromCameraStart - 2; } }

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
            var cameraOriginScreenPoint = new CCPoint(160, 240);            // The camera in the center of the screen
            this.CurrentPageModel.LogicalStartOrigin = new CCPoint(160, 240);  // We start our hero 1/4 in from left in middle of screen
            this.CurrentPageModel.LogicalLineSpacingTop = new CCPoint(160, 340);  // We want the line margin to fill most of the screen with a little border on top/bottom
            this.CurrentPageModel.LogicalLineSpacingBottom = new CCPoint(160,  140);

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
            var padPhysicsOrigin = new Vector3(
                this.CurrentPageModel.WorldOrigin.X + (0.5f * this.CurrentPageModel.WorldWidth),
                this.CurrentPageModel.WorldOrigin.Y + (0.5f * this.CurrentPageModel.WorldHeight),
                this.CurrentPageModel.WorldOrigin.Z - (0.5f * this.CurrentPageModel.WorldDepth)
                );

            // Create physics box to represent pad and add to physics space
            var padPhysicsBox = new Box(
                MathConverter.Convert(padPhysicsOrigin), 
                this.CurrentPageModel.WorldWidth, 
                this.CurrentPageModel.WorldHeight, 
                this.CurrentPageModel.WorldDepth);
            this.CurrentPageModel.PhysicsEntity = padPhysicsBox;
            padPhysicsBox.Tag = this.CurrentPageModel;
            this._physicsManager.TheSpace.Add(padPhysicsBox);
        }

        public void SwitchState(GameState state)
        {
            switch (state)
            {
                case GameState.Intro:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        break;
                    }
                case GameState.Moving:
                    {
                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        // Set state
                        this._currentLineNumber++;

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Remaining refresh logic centralized in helper
                        this.Refresh();

                        // Migrate to the start game state
                        state = GameState.Start;

                        break;
                    }
                case GameState.Start:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        break;
                    }
            }

            // Update our state
            this._currentGameState = state;
        }

        #endregion

        #region Helper methods

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

        private void InitCurrentPageModel(bool unloadPreviousPageModel=false)
        {
            // Get our current page definition ready, will be needed for positioning camera
            var currentPad = UserDefaults.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_PAD,
                GameConstants.USER_DEFAULT_INITIAL_CURRENT_PAD);
            var padEntity = this._padRepository.GetPad(currentPad);

            // If requested, clear out any previous page models and their effects
            // (e.g., coming from options page and selected a new texture for page model)
            if (unloadPreviousPageModel)
            {
                this._customContentManager.Unload();
                GameModel.OriginalEffectsDictionary.Remove(padEntity.ModelName);
            }

            // Load a fresh or cached version of our page model
            this.CurrentPageModel = new PageModel(
                padEntity: padEntity,
                customContentManager: this._customContentManager,
                allowCached: true);

            // We only have 1 pad at a time to worry about
            this.CurrentPageModel.ModelID = 1;  
            this._ocTreeRoot.AddModel(CurrentPageModel);
        }

        #endregion

    }
}