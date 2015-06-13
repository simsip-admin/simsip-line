using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using BEPUphysics.Character;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionShapes;
using ConversionHelper;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Engine.Input;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.Effects.Stock;
using Simsip.LineRunner.GameObjects.Obstacles;
using System.Diagnostics;
using BEPUphysics;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#else
using System.Threading;
#endif
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
#else
using System.Collections.Concurrent;
using Simsip.LineRunner.Entities.LineRunner;
#endif


namespace Simsip.LineRunner.GameObjects.Lines
{
    public class LineCache : GameComponent, ILineCache
    {
        // Required services
        private IInputManager _inputManager;
        private IPageCache _pageCache;
        private IObstacleCache _obstacleCache;
        private IPhysicsManager _physicsManager;
        private IPageLinesRepository _pageLinesRepository;
        private ILineRepository _lineRepository;

        // State we maintain
        private int _currentPageNumber;
        private int _currentLineNumber;
        private GameState _currentGameState;
        private IList<LineModel> _lineHitList;

        // Determines what gets asked to be drawn
        private OcTreeNode _ocTreeRoot;

        // Support for staging the results of asynchronous loads and then signaling
        // we need the results processed on the next update cycle
        private class LoadContentThreadArgs
        {
            public LoadContentAsyncType TheLoadContentAsyncType;
            public GameState TheGameState;
            public int PageNumber;
            public int[] LineNumbers;
            public IList<LineModel> LineModelsAsync;
        }
        private ConcurrentQueue<LoadContentThreadArgs> _loadContentThreadResults;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public LineCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(ILineCache), this); 
        }

        #region Properties

        public IList<LineModel> LineModels { get; private set; }

        #endregion

        #region GameComponent Overrides

        public override void Initialize()
        {
            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;
            this.LineModels = new List<LineModel>();
            this._loadContentThreadResults = new ConcurrentQueue<LoadContentThreadArgs>(); 
            this._lineHitList = new List<LineModel>();

            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));
            this._obstacleCache = (IObstacleCache)this.Game.Services.GetService(typeof(IObstacleCache));
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
            this._pageLinesRepository = new PageLinesRepository();
            this._lineRepository = new LineRepository();

            // Initialize our drawing filter
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Loop over line hit list
            foreach (var lineModel in this._lineHitList.ToList())
            {
                // Emit event based on each line
                var args = new LineHitEventArgs(lineModel);
                if (LineHit != null)
                {
                    LineHit(this, args);
                }
            }

            // Clear line hit list after finishing loop
            this._lineHitList.Clear();

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
                        var eventArgs = new LoadContentAsyncFinishedEventArgs(
                            loadContentThreadArgs.TheLoadContentAsyncType,
                            loadContentThreadArgs.TheGameState);
                        LoadContentAsyncFinished(this, eventArgs);
                    }
                }
            }
        }

        #endregion

        #region ILineCache Implementation

        public event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        public void Draw(StockBasicEffect effect, EffectType type)
        {
            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ViewMatrix;

            /* TODO: We sometimes lose our line display, think it is due to buggy boundingfrustrum
            BoundingFrustum frustrum = new BoundingFrustum(view * projection);
            _ocTreeRoot.Draw(view, projection, frustrum);
            */
            foreach (var lineModel in this.LineModels.ToList())
            {
                // Limit lines drawn up to current line number, others have not been animated
                // into place
                if (this._currentLineNumber >= lineModel.ThePageLinesEntity.LineNumber)
                {
                    lineModel.Draw(view, projection, effect, type);
                }
            }
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

                        // In background get initial lines constructed for first page.
                        // this.ProcessNextLine() will be called in event handler when 
                        // finished to animate header and first line for first page into position
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Initialize, state);

                        break;
                    }
                case GameState.Moving:
                    {
                        // Propogate state change
                        this._obstacleCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        // Set state
                        this._currentLineNumber++;

                        // Animate next line into position
                        this.ProcessNextLine();

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Next, state);

                        // Propogate state change
                        // this._obstacleCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Animate first line and header for next page into position
                        this.ProcessNextLine();

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Next, state);

                        // Propogate state change
                        // this._obstacleCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // In background get initial lines constructed for first page.
                        // this.ProcessNextLine() will be called in event handler when
                        // finished to animate header and first line for first page into position
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Initialize, state);

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // In background get initial lines constructed for first page.
                        // this.ProcessNextLine() will be called in event handler when
                        // finished to animate header and first line for first page into position
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Refresh, state);

                        break;
                    }
                case GameState.Start:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Propogate state change
                        this._obstacleCache.SwitchState(state);

                        break;
                    }
            }
        }

        public event LineHitEventHandler LineHit;

        public void AddLineHit(LineModel lineModel)
        {
            if (!_lineHitList.Contains(lineModel))
            {
                _lineHitList.Add(lineModel);
            }
        }

        public LineModel GetLineModel(int lineNumber)
        {
            var lineModel = this.LineModels
                            .Where(x => x.ThePageLinesEntity.LineNumber == lineNumber)
                            .FirstOrDefault();
            return lineModel;
        }

        #endregion

        #region Event Handlers

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            this.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

            /*
            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Initialize ||
                args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
            */
                this.ProcessNextLine();

                // Propogate state change
                this._obstacleCache.SwitchState(args.TheGameState);
            // }
        }

        #endregion

        #region Helper methods

        // IMPORTANT: Do not call on background thread. Only call during normal Update/Draw cycle.
        // (e.g., via SwitchState())
        private void LoadContentAsync(LoadContentAsyncType loadContentAsyncType, GameState gameState)
        {
            // Determine which page/line number we are loading
            var pageNumber = -1;
            int[] lineNumbers = null;
            switch (loadContentAsyncType)
            {
                case LoadContentAsyncType.Initialize:
                case LoadContentAsyncType.Refresh:
                    {
                        pageNumber = this._currentPageNumber;
                        lineNumbers = new int[] { 
                            this._currentLineNumber - 1, 
                            this._currentLineNumber, 
                            this._currentLineNumber + 1 };
                        break;
                    }
                case LoadContentAsyncType.Next:
                    {
                        // Are we on the last line of the page
                        var lineCount = this._pageCache.CurrentPageModel.ThePadEntity.LineCount;
                        if (this._currentLineNumber == lineCount)
                        {
                            // Ok, on last line, move to first line of next page
                            pageNumber = this._currentPageNumber + 1;
                            lineNumbers = new int[] { 1 };
                        }
                        else
                        {
                            // Not on last line, just go for next line
                            pageNumber = this._currentPageNumber;
                            lineNumbers = new int[] { this._currentLineNumber + 1 };
                        }
                        break;
                    }
            }

            // Build our state object for this background content load request
            var loadContentThreadArgs = new LoadContentThreadArgs
            {
                TheLoadContentAsyncType = loadContentAsyncType,
                TheGameState = gameState,
                PageNumber = pageNumber,
                LineNumbers = lineNumbers,
                LineModelsAsync = new List<LineModel>()
            };

            // If we are doing a refresh we need to flush our custom content manaager so we don't load
            // a cached version of the model - critical when changing textures for model.
            // (e.g., coming from options page and selected a new texture for line models)
            //
            // Since this will cause Dispose to be called on our XNA models, we need to carefully
            // clear out our entire set of models, physics and textures for this scenario.
            if (loadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                // Remove all previous models from our drawing filter
                // and physics from our physics simulation
                foreach (var lineModel in this.LineModels.ToList())
                {
                    this._ocTreeRoot.RemoveModel(lineModel.ModelID);

                    if (lineModel.PhysicsEntity != null &&
                        lineModel.PhysicsEntity.Space != null)
                    {
                        this._physicsManager.TheSpace.Remove(lineModel.PhysicsEntity);
                    }

                    lineModel.TheCustomContentManager.Unload();
                }

                // And finally clear out the full set of previous models
                this.LineModels.Clear();
            }

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
            var lineNumbers = loadContentThreadArgs.LineNumbers;
            var lineModels = loadContentThreadArgs.LineModelsAsync;

            // Line construction logic will vary based on if we have
            // 1 line definition or more than 1 line definition for the current page
            var pageLinesEntities = this._pageLinesRepository.GetLines(this._currentPageNumber); 
            if (pageLinesEntities.Count == 1)
            {
                // Ok, we have one line definition that is the same for all
                // lines on this page
                var padEntity = this._pageCache.CurrentPageModel.ThePadEntity;
                for (int i = 0; i < lineNumbers.Count(); i++)
                {
                    // Get an initial model constructed
                    var currentLine = UserDefaults.SharedUserDefault.GetStringForKey(
                        GameConstants.USER_DEFAULT_KEY_CURRENT_LINE,
                        GameConstants.USER_DEFAULT_INITIAL_CURRENT_LINE);
                    var lineEntity = this._lineRepository.GetLine(currentLine);

                    // Create a PageLinesEntity to represent this model
                    var pageLinesEntity = new PageLinesEntity
                        {
                            PageNumber = pageLinesEntities[0].PageNumber,
                            LineNumber = lineNumbers[i],
                            ModelName = pageLinesEntities[0].ModelName,
                            LineType = pageLinesEntities[0].LineType
                        };

                    // Load a fresh or cached version of our line model
                    var customContentManager = new CustomContentManager(
                       TheGame.SharedGame.Services,
                       TheGame.SharedGame.Content.RootDirectory);
                    var lineModel = new LineModel(
                        lineEntity,
                        pageLinesEntity,
                        customContentManager);

                    // Scale the line model to be the width of the page
                    var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                    var scaleMatrix = Matrix.CreateScale(scale);

                    // Now get our new world dimensions
                    lineModel.ModelToWorldRatio = scale;
                    lineModel.WorldWidth = lineModel.TheModelEntity.ModelWidth * scale;
                    lineModel.WorldHeight = lineModel.TheModelEntity.ModelHeight * scale;
                    lineModel.WorldDepth = lineModel.TheModelEntity.ModelDepth * scale;

                    // To determine origin of line model:
                    var lineSpacingMultiplier = padEntity.LineCount - lineNumbers[i];
                    var translatePosition = this._pageCache.CurrentPageModel.WorldOrigin +                                          // Start at world orgin for pad
                                            new Vector3(0, 0, -0.5f * this._pageCache.CurrentPageModel.WorldDepth) +                // Tuck it out of sight
                                            new Vector3(0, this._pageCache.CurrentPageModel.WorldFooterMargin, 0) +                 // Add in footer margin
                                            new Vector3(0, lineSpacingMultiplier * this._pageCache.CurrentPageModel.WorldLineSpacing, 0);  // Then add in current line count * line margin
                    var translateMatrix = Matrix.CreateTranslation(translatePosition);

                    // Finally, create our flatten matrix to make lines flat until
                    // we navigate to line and expand it
                    var flattenMatrix = Matrix.CreateScale(1f, 1f, 0.1f);

                    // Translate and scale our model
                    lineModel.WorldMatrix = flattenMatrix * scaleMatrix * translateMatrix;

                    // Uniquely identify line for octree
                    lineModel.ModelID = lineNumbers[i];

                    // Record to our staging collection
                    lineModels.Add(lineModel);

                    // CreateLineHitParticles line as a physics box body
                    // IMPORTANT: Adjusting for physics representation with origin in middle
                    var physicsLocalTransfrom = new BEPUutilities.Vector3(
                        (0.5f * lineModel.WorldWidth),
                        (0.5f * lineModel.WorldHeight),
                        -(0.5f * lineModel.WorldDepth));
                    lineModel.PhysicsLocalTransform = BEPUutilities.Matrix.CreateTranslation(physicsLocalTransfrom);

                    // IMPORTANT, line will move out in ProcessLine(), hence the adjustment here to our z value
                    var linePhysicsOrigin = 
                        ConversionHelper.MathConverter.Convert(lineModel.WorldOrigin) +
                        physicsLocalTransfrom;
                    linePhysicsOrigin.Z = 
                        linePhysicsOrigin.Z -
                        (0.5f * this._pageCache.CurrentPageModel.WorldDepth);
                        
                    // CreateLineHitParticles physics box to represent line and add to physics space
                    // IMPORTANT: Don't add until we are on the thread executing our Update() logic
                    var linePhysicsBox = new Box(
                        linePhysicsOrigin,
                        lineModel.WorldWidth,
                        lineModel.WorldHeight,
                        lineModel.WorldDepth);
                    lineModel.PhysicsEntity = linePhysicsBox;
                    linePhysicsBox.Tag = lineModel;
                    linePhysicsBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
                }
            }
            else
            {
                // TODO: Ok, we have each line for this page individually defined
            }

            this._loadContentThreadResults.Enqueue(loadContentThreadArgs);
        }

        // Migrate staged collection in args to public collection
        private void ProcessLoadContentAsync(LoadContentThreadArgs loadContentThreadArgs)
        {
            switch (loadContentThreadArgs.TheLoadContentAsyncType)
            {
                case LoadContentAsyncType.Initialize:
                case LoadContentAsyncType.Refresh:
                    {

                        // Remove all previous models from our drawing filter
                        // and physics from our physics simulation
                        foreach (var lineModel in this.LineModels.ToList())
                        {
                            this._ocTreeRoot.RemoveModel(lineModel.ModelID);

                            if (lineModel.PhysicsEntity != null &&
                                lineModel.PhysicsEntity.Space != null)
                            {
                                this._physicsManager.TheSpace.Remove(lineModel.PhysicsEntity);
                            }

                            // Remove all previous animations for this model
                            lineModel.ModelActionManager.RemoveAllActionsFromTarget(lineModel);

                            // Dispose of all XNA resources
                            lineModel.TheCustomContentManager.Unload();
                            lineModel.TheCustomContentManager.Dispose();
                        }

                        // And clear out the full set of previous models
                        this.LineModels.Clear();

                        break;
                    }
                case LoadContentAsyncType.Next:
                    {
                        // Remove all previous obstacle models from our drawing filter
                        // and remove all previous obstacle physics
                        var lineToRemove = this._currentLineNumber - 2;
                        if (lineToRemove > 0)
                        {
                            var lineModel = this.LineModels[0];
                            this._ocTreeRoot.RemoveModel(lineModel.ModelID);

                            if (lineModel.PhysicsEntity != null &&
                                lineModel.PhysicsEntity.Space != null)
                            {
                                this._physicsManager.TheSpace.Remove(lineModel.PhysicsEntity);
                            }

                            // Remove all previous animations for this model
                            lineModel.ModelActionManager.RemoveAllActionsFromTarget(lineModel);

                            this.LineModels.Remove(lineModel);

                            // Dispose of all XNA resources
                            lineModel.TheCustomContentManager.Unload();
                            lineModel.TheCustomContentManager.Dispose();
                        }

                        break;
                    }
            }

            // Populate our public model/physics collections from our staged collections
            foreach (var lineModel in loadContentThreadArgs.LineModelsAsync)
            {
                // Add to octree for filtered drawing
                this._ocTreeRoot.AddModel(lineModel);

                this.LineModels.Add(lineModel);

                if (lineModel.PhysicsEntity != null)
                {
                    this._physicsManager.TheSpace.Add(lineModel.PhysicsEntity);
                }
            }
        }

        private void ProcessNextLine()
        {
            // First line also animates in the header line
            if (this._currentLineNumber == GameManager.SharedGameManager.AdminStartLineNumber)
            {
                var headerLineNumber = GameManager.SharedGameManager.AdminStartLineNumber - 1;
                var headerLine = this.GetLineModel(headerLineNumber);
                 
                // IMPORTANT: Note the adjustment in depth as we have staged this line tucked behind page.
                //            See ProcessNextPage() for staging
                var headerMoveToPosition = headerLine.WorldOrigin +
                        new Vector3(0, 0, headerLine.WorldDepth + (0.5f * this._pageCache.CurrentPageModel.WorldDepth));
                var headerScaleAction = new ScaleTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, headerLine.ModelToWorldRatio);
                var headerMoveAction = new MoveTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, headerMoveToPosition);
                var headerAction = new Simsip.LineRunner.Actions.Parallel(new FiniteTimeAction[] { headerScaleAction, headerMoveAction });

                // Now only animate header if we are doing the intro, otherwise, just get header in place
                // quickly after kill
                if (this._currentGameState == GameState.Intro)
                {
                    headerLine.ModelRunAction(headerAction);
                }
                else
                {
                    var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                    var scaleMatrix = Matrix.CreateScale(scale);
                    var translateMatrix = Matrix.CreateTranslation(headerMoveToPosition);
                    headerLine.WorldMatrix = scaleMatrix * translateMatrix;
                    headerLine.PhysicsEntity.Position =
                        ConversionHelper.MathConverter.Convert(headerMoveToPosition) +
                        headerLine.PhysicsLocalTransform.Translation;
                }
            }                    

            // Normal line animation into place        
            var lineModel = this.GetLineModel(this._currentLineNumber);

            // IMPORTANT: Note the adjustment in depth as we have staged this line tucked behind page.
            //            See ProcessNextPage() for staging
            var lineMoveToPosition = lineModel.WorldOrigin +
                                     new Vector3(0, 0, lineModel.WorldDepth + (0.5f * this._pageCache.CurrentPageModel.WorldDepth));

            var lineScaleAction = new ScaleTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, lineModel.ModelToWorldRatio);
            var lineMoveAction = new MoveTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, lineMoveToPosition);
            var lineAction = new Simsip.LineRunner.Actions.Parallel(new FiniteTimeAction[] { lineScaleAction, lineMoveAction });

            // Now if we are not in the intro and this is the first line display
            // right away as we are coming back from a kill
            if (this._currentGameState != GameState.Intro &&
                this._currentLineNumber == 1)
            {
                var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                var scaleMatrix = Matrix.CreateScale(scale);
                var translateMatrix = Matrix.CreateTranslation(lineMoveToPosition);
                lineModel.WorldMatrix = scaleMatrix * translateMatrix;
                lineModel.PhysicsEntity.Position =
                    ConversionHelper.MathConverter.Convert(lineMoveToPosition) +
                    lineModel.PhysicsLocalTransform.Translation;
            }
            else
            {
                lineModel.ModelRunAction(lineAction);
            }
        }

        /// <summary>
        /// Used to handle a collision event triggered by an entity specified above.
        /// </summary>
        /// <param name="sender">Entity that had an event hooked.</param>
        /// <param name="other">Entity causing the event to be triggered.</param>
        /// <param name="pair">Collision pair between the two objects in the event.</param>
        private void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            // This type of event can occur when an entity hits any other object which can be collided with.
            // They aren't always entities; for example, hitting a StaticMesh would trigger this.
            // Entities use EntityCollidables as collision proxies; see if the thing we hit is one.
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation == null)
            {
                return;
            }

            var otherModel = otherEntityInformation.Entity.Tag as HeroModel;
            if (otherModel == null)
            {
                return;
            }

            // TODO: Flag to action layer we need to move to reset game
            //       after update/draw cycle has completed
            //We hit an entity! remove it.
            var lineModel = sender.Entity.Tag as LineModel;
            if (lineModel != null &&
                !_lineHitList.Contains(lineModel))
            {
                lineModel.TheContact = pair.Contacts[0].Contact;
                _lineHitList.Add(lineModel);
            }
        }

        #endregion

    }
}