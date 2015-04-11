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


namespace Simsip.LineRunner.GameObjects.Lines
{
    /// <summary>
    /// The function signature to use when you are subscribing to be notified of line hits.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LineHitEventHandler(object sender, LineHitEventArgs e);

    /// <summary>
    /// The value class that will be passed to subscribers of line hits.
    /// </summary>
    public class LineHitEventArgs : EventArgs
    {
        private LineModel _lineModel;

        public LineHitEventArgs(LineModel lineModel)
        {
            this._lineModel = lineModel;
        }

        /// <summary>
        /// The line model that was hit.
        /// </summary>
        public LineModel TheLineModel
        {
            get { return this._lineModel; }
        }
    }

    public class LineCache : DrawableGameComponent, ILineCache
    {
        // Required services
        private IInputManager _inputManager;
        private IPageCache _pageCache;
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

        // Custom content manager so we can reload a page model with different textures
        private CustomContentManager _customContentManager;

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

        public bool Ready { get; private set; }

        #endregion

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;
            this.LineModels = new List<LineModel>();
            this._lineHitList = new List<LineModel>();

            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
            this._pageLinesRepository = new PageLinesRepository();
            this._lineRepository = new LineRepository();

            // Initialize our drawing filter
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            // We use our own CustomContentManager scoped to this cache so that
            // we can reload a page model with different textures
            this._customContentManager = new CustomContentManager(
                TheGame.SharedGame.Services,
                TheGame.SharedGame.Content.RootDirectory);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Currently no-op as content is dynamically loaded in Process()
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
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }

        #endregion

        #region ILineCache Implementation
                
        public void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None)
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

        public void SwitchState(GameState state)
        {
            switch (state)
            {
                case GameState.Intro:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get lines constructed for first page
                        this.ProcessNextPage();

                        // Animate header and first line for first page into position
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.Moving:
                    {
                        // Currently a no-op
                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        // Set state
                        this._currentLineNumber++;

                        // Animate next line into position
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Get lines constructed for next page
                        this.ProcessNextPage();

                        // Animate first line and header for next page into position
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get lines constructed for first page
                        this.ProcessNextPage();

                        // Animate header and first line for first page into position
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get lines constructed for first page
                        // IMPORTANT: Note how we override the default parameter of unloadPreviousLineModels
                        //            to true. This will force us to get a fresh copy of the line models.
                        this.ProcessNextPage(unloadPreviousLineModels: true);

                        // Animate header and first line for first page into position
                        this.ProcessNextLine();

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
            _currentGameState = state;
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
            return this.LineModels[this.LineModels.Count - (lineNumber+1)];
        }

        #endregion

        #region Helper methods

        private void ProcessNextPage(bool unloadPreviousLineModels=false)
        {
            // Signal to anyone who needs to know we are rebuilding
            this.Ready = false;

            // Remove all previous models from our drawing filter
            // and physics from our physics simulation
            foreach(var lineModel in this.LineModels.ToList())
            {
                this._ocTreeRoot.RemoveModel(lineModel.ModelID);

                if (lineModel.PhysicsEntity != null &&
                    lineModel.PhysicsEntity.Space != null)
                {
                    this._physicsManager.TheSpace.Remove(lineModel.PhysicsEntity);
                }
            }

            // Ok, now clear out our previous collection
            this.LineModels.Clear();

            // If requested, clear out any previous line models
            // (e.g., coming from options page and selected a new texture for line models)
            if (unloadPreviousLineModels)
            {
                this._customContentManager.Unload();
            }

            // Line construction logic will vary based on if we have
            // 1 line definition or more than 1 line definition for the current page
            var pageLinesEntities = this._pageLinesRepository.GetLines(_currentPageNumber);
            if (pageLinesEntities.Count == 1)
            {
                // Ok, we have one line definition that is the same for all
                // lines on this page
                var padEntity = this._pageCache.CurrentPageModel.ThePadEntity;
                for(int i = 0; i < padEntity.LineCount + 1; i++)
                {
                    // Get an initial model constructed
                    var currentLine = UserDefaults.SharedUserDefault.GetStringForKey(
                        GameConstants.USER_DEFAULT_KEY_CURRENT_LINE,
                        GameConstants.USER_DEFAULT_INITIAL_CURRENT_LINE);
                    var lineEntity = this._lineRepository.GetLine(currentLine);

                    // If requested, clear out any previous line model effects
                    // (e.g., coming from options page and selected a new texture for line models)
                    if (unloadPreviousLineModels)
                    {
                        GameModel.OriginalEffectsDictionary.Remove(lineEntity.ModelName);
                    }

                    // Load a fresh or cached version of our page model
                    var lineModel = new LineModel(
                        lineEntity: lineEntity, 
                        pageLinesEntity: pageLinesEntities[0],
                        customContentManager: this._customContentManager,
                        allowCached: true);

                    // Scale the line model to be the width of the page
                    var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                    var scaleMatrix = Matrix.CreateScale(scale);
                    
                    // Now get our new world dimensions
                    lineModel.ModelToWorldRatio = scale;
                    lineModel.WorldWidth = lineModel.TheModelEntity.ModelWidth * scale;
                    lineModel.WorldHeight = lineModel.TheModelEntity.ModelHeight * scale;
                    lineModel.WorldDepth = lineModel.TheModelEntity.ModelDepth * scale;

                    // To determine origin of line model:
                    var translatePosition = this._pageCache.CurrentPageModel.WorldOrigin +                             // Start at world orgin for pad
                                            new Vector3(0, 0, -0.5f * this._pageCache.CurrentPageModel.WorldDepth) +   // Tuck it out of sight
                                            new Vector3(0, this._pageCache.CurrentPageModel.WorldFooterMargin, 0) +    // Add in footer margin
                                            new Vector3(0, i * this._pageCache.CurrentPageModel.WorldLineSpacing, 0);  // Then add in current line count * line margin
                    var translateMatrix = Matrix.CreateTranslation(translatePosition);

                    // Finally, create our flatten matrix to make lines flat until
                    // we navigate to line and expand it
                    var flattenMatrix = Matrix.CreateScale(1f, 1f, 0.1f);

                    // Translate and scale our model
                    lineModel.WorldMatrix = flattenMatrix * scaleMatrix * translateMatrix;

                    // Uniquely identify line for octree
                    lineModel.ModelID = i;

                    // And add to octree for filtered drawing
                    this._ocTreeRoot.AddModel(lineModel);

                    // Record to our collection
                    this.LineModels.Add(lineModel);

                    // Create line as a physics box body
                    // IMPORTANT: Adjusting for physics representation with origin in middle
                    var linePhysicsOrigin = new Vector3(
                        lineModel.WorldOrigin.X + (0.5f * lineModel.WorldWidth),
                        lineModel.WorldOrigin.Y + (0.5f * lineModel.WorldHeight),
                        // IMPORTANT, line will move out in ProcessLine(), hence the adjustment here to our z value
                        // (normally we would subtract but above in setting line origin we hav sunk it into pad)
                        lineModel.WorldOrigin.Z + (0.5f * this._pageCache.CurrentPageModel.WorldDepth) + 
                            (0.5f * lineModel.WorldDepth) 
                        );

                    // Create physics box to represent line and add to physics space
                    var linePhysicsBox = new Box(
                        MathConverter.Convert(linePhysicsOrigin), 
                        lineModel.WorldWidth, 
                        lineModel.WorldHeight, 
                        lineModel.WorldDepth);
                    lineModel.PhysicsEntity = linePhysicsBox;
                    linePhysicsBox.Tag = lineModel;
                    linePhysicsBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
                    this._physicsManager.TheSpace.Add(linePhysicsBox);
                }
            }
            else
            {
                // Ok, we have each line for this page individually defined
                foreach(var pageLinesEntity in pageLinesEntities)
                {
                    // TODO:
                    // Determine our scale for each line

                    var lineEntity = _lineRepository.GetLine(pageLinesEntity.ModelName);
                    var lineModel = new LineModel(lineEntity, pageLinesEntity);

                    // Uniquely identity line for octree
                    lineModel.ModelID = pageLinesEntity.LineNumber;

                    // Add to octree for filtered drawing
                    this._ocTreeRoot.AddModel(lineModel);

                    // Record to our collection
                    this.LineModels.Add(lineModel);
                }
            }

            // Ok, we are good to go, let anyone interested know
            this.Ready = true;
        }

        private void ProcessNextLine()
        {
            // First line also animates in the header line
            if (this._currentLineNumber == 1)
            {
                var headerLine = this.GetLineModel(0);
                 
                // IMPORTANT: Note the adjustment in depth as we have staged this line tucked behind page.
                //            See ProcessNextPage() for staging
                var headerMoveToPosition = headerLine.WorldOrigin +
                        new Vector3(0, 0, headerLine.WorldDepth + (0.5f * this._pageCache.CurrentPageModel.WorldDepth));
                var headerScaleAction = new ScaleTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, headerLine.ModelToWorldRatio);
                var headerMoveAction = new MoveTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, headerMoveToPosition);
                var headerAction = new Parallel(new FiniteTimeAction[] { headerScaleAction, headerMoveAction });
                headerLine.ModelRunAction(headerAction);
            }                    

            // Normal line animation into place        
            var lineModel = this.GetLineModel(this._currentLineNumber);


            // IMPORTANT: Note the adjustment in depth as we have staged this line tucked behind page.
            //            See ProcessNextPage() for staging
            var lineMoveToPosition = lineModel.WorldOrigin +
                                     new Vector3(0, 0, lineModel.WorldDepth + (0.5f * this._pageCache.CurrentPageModel.WorldDepth));

            var lineScaleAction = new ScaleTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, lineModel.ModelToWorldRatio);
            var lineMoveAction = new MoveTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, lineMoveToPosition);
            var lineAction = new Parallel(new FiniteTimeAction[] { lineScaleAction, lineMoveAction });
            lineModel.ModelRunAction(lineAction);
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