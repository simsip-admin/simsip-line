using Cocos2D;
using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Simsip.LineRunner.Effects.Deferred;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Simsip.LineRunner.ContentPipeline;
using BEPUphysics.CollisionRuleManagement;
using Engine.Input;
using Simsip.LineRunner.GameObjects.Characters;


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    /// <summary>
    /// The function signature to use when you are subscribing to be notified of obstacle hits.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ObstacleHitEventHandler(object sender, ObstacleHitEventArgs e);

    /// <summary>
    /// The value class that will be passed to subscribers of obstacle hits.
    /// </summary>
    public class ObstacleHitEventArgs : EventArgs
    {
        private ObstacleModel _obstacleModel;

        public ObstacleHitEventArgs(ObstacleModel obstacleModel)
        {
            this._obstacleModel = obstacleModel;
        }

        /// <summary>
        /// The obstacle model that was hit.
        /// </summary>
        public ObstacleModel TheObstacleModel
        {
            get { return this._obstacleModel; }
        }
    }

    public class ObstacleCache : DrawableGameComponent, IObstacleCache
    {
        // Required services
        private IInputManager _inputManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private IPhysicsManager _physicsManager;
        private IPageObstaclesRepository _pageObstaclesRepository;
        private IObstacleRepository _obstacleRepository;

        // State we maintain
        private int _currentPageNumber;
        private int _currentLineNumber;
        private GameState _currentGameState;
        private IList<ObstacleModel> _obstacleHitList;

        // Determines what gets asked to be drawn
        private OcTreeNode _ocTreeRoot;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public ObstacleCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IObstacleCache), this);
        }

        #region Properties

        public IList<ObstacleModel> ObstacleModels { get; private set; }

        #endregion

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Iniitialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;
            this.ObstacleModels = new List<ObstacleModel>();
            this._obstacleHitList = new List<ObstacleModel>();

            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));
            this._lineCache = (ILineCache)this.Game.Services.GetService(typeof(ILineCache));
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
            this._pageObstaclesRepository = new PageObstaclesRepository();
            this._obstacleRepository = new ObstacleRepository();

            // Initialize our drawing filter
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Currently no-op as content is dynamicaly loaded in Process()
        }

        public override void Update(GameTime gameTime)
        {
            // Loop over obstacle hit list
            foreach (var obstacleModel in this._obstacleHitList.ToList())
            {
                // Emit event based on each obstacle
                var args = new ObstacleHitEventArgs(obstacleModel);
                if (ObstacleHit != null)
                {
                    ObstacleHit(this, args);
                }

                // Clear obstacle from world if ObstacleModel says so
                if (obstacleModel.RemoveAfterUpdate)
                {
                    if (obstacleModel.PhysicsEntity != null &&
                        obstacleModel.PhysicsEntity.Space != null)
                    {
                        this._physicsManager.TheSpace.Remove(obstacleModel.PhysicsEntity);
                    }

                    // TODO: Clear visual representation as well?
                }
            }

            // Clear obstacle hit list after finishing loop
            this._obstacleHitList.Clear();
        }


        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }

        #endregion

        #region IObstacleCache Implementation

        public void Draw(Effect effect = null, EffectType type = EffectType.None)
        {
            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            var frustrum = new BoundingFrustum(view * projection);
            _ocTreeRoot.Draw(view, projection, frustrum, effect, type);
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

                        // Get obstacles constructed for first page
                        this.ProcessNextPage();

                        // 1. Animate first line's obstacles into position
                        // 2. Disable previous line physics (n/a)
                        // 3. Enable current line physics
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

                        // 1. Animate next line's obstacles into position
                        // 2. Disable previous line physics
                        // 3. Enable current line physics
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Get obstacles constructed for next page
                        this.ProcessNextPage();

                        // 1. Animate first line's obstacles into position
                        // 2. TODO: Disable previous line physics
                        // 3. Enable current line physics
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get obstacles constructed for first page
                        this.ProcessNextPage();

                        // 1. Animate first line's obstacles into position
                        // 2. TODO: Disable previous line physics
                        // 3. Enable current line physics
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get obstacles constructed for first page
                        this.ProcessNextPage();

                        // 1. Animate first line's obstacles into position
                        // 2. TODO: Disable previous line physics
                        // 3. Enable current line physics
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

        public event ObstacleHitEventHandler ObstacleHit;

        public void AddObstacleHit(ObstacleModel obstacleModel)
        {
            if (!_obstacleHitList.Contains(obstacleModel))
            {
                _obstacleHitList.Add(obstacleModel);
            }
        }

        #endregion

        #region Helper Methods

        private void ProcessNextPage()
        {
            // Remove all previous obstacle models from our drawing filter
            foreach (var obstacleModel in this.ObstacleModels.ToList())
            {
                this._ocTreeRoot.RemoveModel(obstacleModel.ModelID);
            }

            // Remove all previous obstacle physics
            foreach(var obstacleModel in this.ObstacleModels.ToList())
            {
                if (obstacleModel.PhysicsEntity != null &&
                    obstacleModel.PhysicsEntity.Space != null)
                {
                    this._physicsManager.TheSpace.Remove(obstacleModel.PhysicsEntity);
                }
            }

            // Now clear out our previous obstacle models collection
            this.ObstacleModels.Clear();

            // Ok, let's grab the collection of obstacles for our current page
            var pageObstaclesEntities = _pageObstaclesRepository.GetObstacles(this._currentPageNumber);
            foreach (var pageObstaclesEntity in pageObstaclesEntities)
            {
                // We'll need the obstacle type to determine below how to do various transformations
                var obstacleType = (ObstacleType)Enum.Parse(typeof(ObstacleType), pageObstaclesEntity.ObstacleType);

                // Get an initial model constructed
                var obstacleEntity = this._obstacleRepository.GetObstacle(pageObstaclesEntity.ModelName);
                var obstacleModel = new ObstacleModel(obstacleEntity, pageObstaclesEntity);

                // If we are a "TOP" type obstacle, flip the model
                // IMPORTANT: Need to do this here before other transformations as currently trying
                // to apply with other transformations has the result of us losing our angle for top obstacles
                if (obstacleType == ObstacleType.SimpleTop)
                {
                    var flipMatrix = Matrix.CreateScale(
                        1f,
                       -1f,       // Flip
                        1f);
                    obstacleModel.WorldMatrix *= flipMatrix;
                }

                // Construct our scale matrix based on how we scaled page
                var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                var scaleMatrix = Matrix.CreateScale(scale);

                // Now get our new world dimensions
                obstacleModel.ModelToWorldRatio = scale;
                obstacleModel.WorldWidth = obstacleModel.TheModelEntity.ModelWidth * scale;
                obstacleModel.WorldHeight = obstacleModel.TheModelEntity.ModelHeight * scale;
                obstacleModel.WorldDepth = obstacleModel.TheModelEntity.ModelDepth * scale;

                // We'll need the corresponding line model for placement
                var lineModel = this._lineCache.GetLineModel(pageObstaclesEntity.LineNumber);

                // Now construct our placement values
                var xScaledToLineWidth = pageObstaclesEntity.LogicalXScaledTo100 *              // 1. Start with x in the [0,100] range
                                                (lineModel.WorldWidth / 100);                   // 2. Scale it by the world width of the line
                var heightScaledToLineSpacing = pageObstaclesEntity.LogicalHeightScaledTo100 *  // 1. Start with the height in the [0,100] range
                                (this._pageCache.CurrentPageModel.WorldLineSpacing / 100);      // 2. Scale it by the world line spacing

                if (obstacleModel.TheObstacleEntity.LogicalHeightScaledOverride != 0)
                {
                    heightScaledToLineSpacing *= obstacleModel.TheObstacleEntity.LogicalHeightScaledOverride / 100;
                }
                var translatedY = (lineModel.WorldOrigin.Y + lineModel.WorldHeight) -
                                  (obstacleModel.WorldHeight - heightScaledToLineSpacing);
                if (obstacleType == ObstacleType.SimpleTop)
                {
                    translatedY = (lineModel.WorldOrigin.Y + lineModel.WorldHeight + this._pageCache.CurrentPageModel.WorldLineSpacing)
                                  - heightScaledToLineSpacing; 
                }

                // Add in x adjustment for rotation if nesessary (angle will be specified as 0 degrees if not rotated)
                var angleInRadians = MathHelper.ToRadians(pageObstaclesEntity.LogicalAngle);
                if (angleInRadians != 0)
                {
                    // Since we rotate at the origin point, we need to adjust our world x position so that the obstacle
                    // pierces the line at the LogicalX value. Constructing a right triangle and knowing the height below the line (adjacent side),
                    // we can determine the x adjustment as the opposite side.
                    // Reference:
                    // http://www.mathsisfun.com/sine-cosine-tangent.html
                    var oppositeSide = Math.Tan(angleInRadians) * (obstacleModel.WorldHeight - obstacleModel.WorldHeightTruncated);
                    xScaledToLineWidth += (float)oppositeSide;
                }

                // Create a depth that will place obstacle out of sight, will be animiated forward in ProcessNextLine
                var translatedZ = -this._pageCache.PageDepthFromCameraStart - (0.5f * this._pageCache.CurrentPageModel.WorldDepth);

                // Ok, we can now create our translation matrix
                var translateMatrix = Matrix.CreateTranslation(new Vector3(
                    xScaledToLineWidth,                                     // 1. World X position with all scaling applied
                    translatedY,                                            // 2. World Y for line model plus World height for line model
                    translatedZ));                                          // 3. Tuck it back out of sight, will be animated foward in ProcessNextLine()

                // Construct rotation matrix (angle will be specified as 0 degrees if not rotated)
                obstacleModel.RotationMatrix = Matrix.CreateRotationZ(angleInRadians);

                // Store this away to help in ObstacleModel.Draw() call for creating scissor rectangle
                obstacleModel.WorldHeightTruncated = heightScaledToLineSpacing;

                // Translate and scale our model
                obstacleModel.WorldMatrix = scaleMatrix * obstacleModel.RotationMatrix * translateMatrix;

                // Uniquely identify character for octree
                obstacleModel.ModelID = pageObstaclesEntity.ObstacleNumber;

                // Add to octree for filtered drawing
                this._ocTreeRoot.AddModel(obstacleModel);

                // Record to our collection
                this.ObstacleModels.Add(obstacleModel);

                // IMPORTANT: Only add/remove physics in ProcessNextLine()
            }
        }

        // 1. Animate next line's obstacles into position
        // 2. Disable previous line physics
        // 3. Enable current line physics
        private void ProcessNextLine()
        {
            // Remove previous line physics
            var previousLineObstacles = this.ObstacleModels
                .Where(x => x.ThePageObstaclesEntity.LineNumber == this._currentLineNumber - 1);
            foreach (var obstacleModel in previousLineObstacles)
            {
                if (obstacleModel.PhysicsEntity != null &&
                    obstacleModel.PhysicsEntity.Space != null)
                {
                    this._physicsManager.TheSpace.Remove(obstacleModel.PhysicsEntity);
                }
            }

            // Grab current line obstacles
            var currentLineObstacles = this.ObstacleModels
                .Where(x => x.ThePageObstaclesEntity.LineNumber == this._currentLineNumber);

            // Animate next line's obstacles into position
            foreach (var obstacleModel in currentLineObstacles)
            {
                // Get a depth that will position obstacle exactly
                // straddling the halfway depth of the line
                var lineModel = this._lineCache.GetLineModel(_currentLineNumber);
                var halfwayDepth = -this._pageCache.PageDepthFromCameraStart +      // Start back at page depth
                                   (0.5f*lineModel.WorldDepth) +                    // Move forward to line depth halfway mark
                                   (0.5f*obstacleModel.WorldDepth);                 // Add in 1/2 of obstacle depth so obstacle
                                                                                    // will be positioned exactly straddling line deph halfway

                // Construct a position to move to that is the same X, Y as the original origin
                // but we move forward to our defined obstacle depth from the camera
                var obstacleMoveToPosition = new Vector3(
                    obstacleModel.WorldOrigin.X,                                                
                    obstacleModel.WorldOrigin.Y,
                    halfwayDepth);

                // Now animate for the defined move to next line duration (will match move to next line flyby, etc.)
                var obstacleMoveTo = new MoveTo(GameConstants.DURATION_MOVE_TO_NEXT_LINE, obstacleMoveToPosition);
                var obstacleProcessPhysics = new CallFunc(() => ProcesObstaclePhysics(obstacleModel));
                var obstacleAction = new Sequence(new FiniteTimeAction[] { obstacleMoveTo, obstacleProcessPhysics });
                obstacleModel.ModelRunAction(obstacleAction);
            }
        }

        private void ProcesObstaclePhysics(ObstacleModel obstacleModel)
        {
            var physicsTransform = new BEPUutilities.AffineTransform()
            {
                Matrix = ConversionHelper.MathConverter.Convert(obstacleModel.WorldMatrix)
            };
            var physicsVertices = obstacleModel.XnaModel.Tag as PhysicsModelVertices;
            var physicsMesh = new MobileMesh(physicsVertices.PhysicsVertices, physicsVertices.Indices, physicsTransform, MobileMeshSolidity.Counterclockwise);
            obstacleModel.PhysicsEntity = physicsMesh;
            physicsMesh.Tag = obstacleModel;
            var physicsLocalTransform = physicsMesh.Position -
                ConversionHelper.MathConverter.Convert(obstacleModel.WorldMatrix.Translation);
            obstacleModel.PhysicsLocalTransform = BEPUutilities.Matrix.CreateTranslation(-physicsLocalTransform);
            obstacleModel.PhysicsEntity.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
            this._physicsManager.TheSpace.Add(physicsMesh);
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
            var obstacleModel = sender.Entity.Tag as ObstacleModel;
            if (obstacleModel != null &&
                !_obstacleHitList.Contains(obstacleModel))
            {
                // Get our contact
                obstacleModel.TheContact = pair.Contacts[0].Contact;
                _obstacleHitList.Add(obstacleModel);
            }
        }


        #endregion

    }
}