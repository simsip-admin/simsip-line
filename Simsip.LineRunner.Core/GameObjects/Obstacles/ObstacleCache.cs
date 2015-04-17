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
using System.Diagnostics;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.Effects.Stock;


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
        private Dictionary<int, IList<RandomObstaclesEntity>> _randomObstacles01;
        private Dictionary<int, IList<RandomObstaclesEntity>> _randomObstacles02;
        private Dictionary<int, IList<RandomObstaclesEntity>> _randomObstacles04;
        private Dictionary<int, IList<RandomObstaclesEntity>> _randomObstacles08;

        private const string RandomPrefix = "Random";

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
            this.InitializeRandomObstacles();

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

        public void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None)
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

            // Ok, let's grab the collection of obstacles for our current page using our helper
            // function that knows how to inject entries coded for random sets
            var pageObstaclesEntities = this.HydratePageObstacles();
            foreach (var pageObstaclesEntity in pageObstaclesEntities)
            {
                // Get an initial model constructed
                var obstacleEntity = this._obstacleRepository.GetObstacle(pageObstaclesEntity.ModelName);
                var obstacleModel = new ObstacleModel(obstacleEntity, pageObstaclesEntity);

                // Construct our scale matrix based on how we scaled page.
                // IMPORTANT: Note how we take into account an additional optional scaling that
                // can be applied to the model to make it bigger or smaller than its default size.
                var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                if (pageObstaclesEntity.LogicalScaleScaledTo100 != 0)
                {
                    scale *= pageObstaclesEntity.LogicalScaleScaledTo100 / 100;
                }
                var scaleMatrix = Matrix.CreateScale(scale);

                // Now get our new world dimensions
                obstacleModel.ModelToWorldRatio = scale;
                obstacleModel.WorldWidth = obstacleModel.TheModelEntity.ModelWidth * scale;
                obstacleModel.WorldHeight = obstacleModel.TheModelEntity.ModelHeight * scale;
                obstacleModel.WorldDepth = obstacleModel.TheModelEntity.ModelDepth * scale;

                // We'll need the corresponding line model for placement
                var lineModel = this._lineCache.GetLineModel(pageObstaclesEntity.LineNumber);

                // Now construct our placement values
                var worldLogicalX = lineModel.WorldWidth *                                      // Start with the world width of the line,
                                    (pageObstaclesEntity.LogicalXScaledTo100 / 100);            // then scale it by our logical X in the range [0,100]
                var worldLogicalHeight = obstacleModel.WorldHeight *                            // Start with the world height of the obstacle,
                                         (pageObstaclesEntity.LogicalHeightScaledTo100 / 100);  // then scale it by logical height in the range [0,100] for truncated obstacles
                                                                                                // or greater than 100 to float in middle of line
                
                // Based on our world logical height, what should our Y position be to represent this obstacle?
                float worldLogicalY = 0;
                if (obstacleModel.TheObstacleType == ObstacleType.SimpleBottom)
                {
                    // Ok, we are jutting up from the bottom by the amount of worldLogicalHeight
                    worldLogicalY = (lineModel.WorldOrigin.Y + lineModel.WorldHeight) -           // (see diagram) Start at the top of the bottom line
                                    (obstacleModel.WorldHeight - worldLogicalHeight);             // then drop down by the remainder of the world height after removing the logical height
                }
                else if (obstacleModel.TheObstacleType == ObstacleType.SimpleTop)
                {
                    worldLogicalY = (lineModel.WorldOrigin.Y + this._pageCache.CurrentPageModel.WorldLineSpacing) +   // (see diagram) Start at the bottom of the top line
                                    (obstacleModel.WorldHeight - worldLogicalHeight);                                 // then move up by the remainder of the world height after removing logical height
                }

                // Add in x and y adjustments if rotating (angle will be specified as 0 degrees if not rotated)
                // Reference:
                // http://www.mathsisfun.com/sine-cosine-tangent.html
                // and see diagrams
                var angleInRadians = MathHelper.ToRadians(pageObstaclesEntity.LogicalAngle);
                if (angleInRadians != 0)
                {
                    if (obstacleModel.TheObstacleType == ObstacleType.SimpleBottom)
                    {
                        if (pageObstaclesEntity.LogicalAngle > 0)
                        {
                            var hypotenuse = obstacleModel.WorldHeight - worldLogicalHeight;
                            var adjacent = (float)Math.Cos(angleInRadians) * hypotenuse;
                            var opposite = (float)Math.Sin(angleInRadians) * hypotenuse;
                            worldLogicalX += opposite;
                            worldLogicalY = (lineModel.WorldOrigin.Y + lineModel.WorldHeight) -
                                             adjacent;
                        }
                        else
                        {
                            var angleInRadiansAbsolute = MathHelper.ToRadians(Math.Abs(pageObstaclesEntity.LogicalAngle));
                            var hypotenuse = obstacleModel.WorldHeight - worldLogicalHeight;
                            var adjacent = (float)Math.Cos(angleInRadiansAbsolute) * hypotenuse;
                            var opposite = (float)Math.Sin(angleInRadiansAbsolute) * hypotenuse;
                            worldLogicalX -= opposite;
                            worldLogicalY = (lineModel.WorldOrigin.Y + lineModel.WorldHeight) -
                                            adjacent;
                        }
                    }
                    else if (obstacleModel.TheObstacleType == ObstacleType.SimpleTop)
                    {
                        if (pageObstaclesEntity.LogicalAngle > 0)
                        {
                            var hypotenuse = obstacleModel.WorldHeight - worldLogicalHeight;
                            var adjacent = (float)Math.Cos(angleInRadians) * hypotenuse;
                            var opposite = (float)Math.Sin(angleInRadians) * hypotenuse;
                            worldLogicalX -= opposite;
                            worldLogicalY = (lineModel.WorldOrigin.Y + this._pageCache.CurrentPageModel.WorldLineSpacing) +
                                            adjacent;
                        }
                        else
                        {
                            var angleInRadiansAbsolute = MathHelper.ToRadians(Math.Abs(pageObstaclesEntity.LogicalAngle));
                            var hypotenuse = obstacleModel.WorldHeight - worldLogicalHeight;
                            var adjacent = (float)Math.Cos(angleInRadiansAbsolute) * hypotenuse;
                            var opposite = (float)Math.Sin(angleInRadiansAbsolute) * hypotenuse;
                            worldLogicalX += opposite;
                            worldLogicalY = (lineModel.WorldOrigin.Y + this._pageCache.CurrentPageModel.WorldLineSpacing) +
                                            adjacent;
                        }
                    }
                }

                // Adjust our depth such that the obstacle will be placed out of sight.
                // Obstacled will be animated forward in ProcessNextLine()
                var translatedZ = -this._pageCache.PageDepthFromCameraStart - (0.5f * this._pageCache.CurrentPageModel.WorldDepth);

                // Ok, we can now create our translation matrix
                var translateMatrix = Matrix.CreateTranslation(new Vector3(
                    worldLogicalX,                                      // 1. World X position with all adjustments as needed
                    worldLogicalY,                                        // 2. World Y position with all adjustments as needed
                    translatedZ));                                      // 3. World Z position tucked back out of sight, will be animated foward in ProcessNextLine()

                // Construct rotation matrix (angle will be specified as 0 degrees if not rotated)
                if (obstacleModel.TheObstacleType == ObstacleType.SimpleBottom)
                {
                    obstacleModel.RotationMatrix = Matrix.CreateRotationZ(angleInRadians);
                }
                else if (obstacleModel.TheObstacleType == ObstacleType.SimpleTop)
                {
                    // IMPORTANT: See diagrams to understand what is going on here
                    // IMPORTANT: This will change the origin and affect positioning. Orign depth will now be in back instead of in front.
                    obstacleModel.RotationMatrix = Matrix.CreateRotationZ(-angleInRadians) * Matrix.CreateRotationX(Microsoft.Xna.Framework.MathHelper.ToRadians(180));

                    /* Leaving this in here in case we need to return to first attempt's implementation where we moved to origin, then did flip, then translated back.:
                    // IMPORTANT: We need to flip our model. To do this we
                    //            1. Translate model to the origin
                    //            2. Scale as normal
                    //            3. Peform flip (TODO: Account for rotation)
                    //            4. Translate model back to position before translating to origin
                    //            5. Translate as normal
                    // IMPORTANT: This will change the origin and affect positioning. Orign depth will now be in back instead of in front.
                    //
                    Matrix.CreateTranslation(
                        -obstacleModel.TheModelEntity.ModelWidth / 2, 
                        -obstacleModel.TheModelEntity.ModelHeight/2, 
                        obstacleModel.TheModelEntity.ModelDepth/2) *
                        scaleMatrix * 
                    obstacleModel.RotationMatrix *
                    Matrix.CreateTranslation(
                        obstacleModel.WorldWidth / 2, 
                        obstacleModel.WorldHeight / 2, 
                       obstacleModel.WorldWidth / 2) *
                   translateMatrix;
                   */

                }

                // Scale, rotate and translate our model
                obstacleModel.WorldMatrix = 
                    scaleMatrix *                   // 1. Scale
                    obstacleModel.RotationMatrix *  // 2. Rotate
                    translateMatrix;                // 3. Translate

                // Now that we have the obstacle positioned correctly, construct an appropriate clipping plane to use
                if (obstacleModel.TheObstacleType == ObstacleType.SimpleBottom)
                {
                    // Clip everything below top of bottom line
                    var distance = lineModel.WorldOrigin.Y + lineModel.WorldHeight;
                    obstacleModel.ClippingPlane = new Vector4(Vector3.Up, -distance);
                }
                else if (obstacleModel.TheObstacleType == ObstacleType.SimpleTop)
                {
                    // Clip everything above bottom of top line
                    var distance = lineModel.WorldOrigin.Y + this._pageCache.CurrentPageModel.WorldLineSpacing;
                    obstacleModel.ClippingPlane = new Vector4(Vector3.Down, distance);
                }

                // Uniquely identify character for octree
                obstacleModel.ModelID = pageObstaclesEntity.ObstacleNumber;

                // Add to octree for filtered drawing
                this._ocTreeRoot.AddModel(obstacleModel);

                // Record to our collection
                this.ObstacleModels.Add(obstacleModel);

                // IMPORTANT: Only add/remove physics in ProcessNextLine()
            }
        }

        private List<PageObstaclesEntity> HydratePageObstacles()
        {
            var returnEntities = new List<PageObstaclesEntity>();
            var randomNumberGenerator = new Random();

            // Get the set of page obstacle entries for the page we are processing
            var pageObstaclesEntities = _pageObstaclesRepository.GetObstacles(this._currentPageNumber);

            // Now loop over all entries
            foreach (var pageObstaclesEntity in pageObstaclesEntities)
            {
                // Update these in case we are injecting random obstacles on this pass
                // which will need to know this state
                var currentObstacleNumber = pageObstaclesEntity.ObstacleNumber;
                var currentLogicalXScaledTo100 = pageObstaclesEntity.LogicalXScaledTo100;

                // Are we injecting random obstacles?
                if (pageObstaclesEntity.ModelName.StartsWith(RandomPrefix))
                {
                    // Grab the "count" this random obstacle entry
                    var count = int.Parse(pageObstaclesEntity.ModelName.Substring(6, 2));

                    // Careful with version, first see if they have specified "x" to indicate
                    // they want us to randomly choose a set from a "count" category.
                    // Example: Random01x
                    //          Pick a random set from the 1 count category
                    var version = 1;
                    if (pageObstaclesEntity.ModelName.Substring(8, 1) == "x")
                    {
                        var setCount = 0;
                        switch(count)
                        {
                            case 1:
                                {
                                    setCount = this._randomObstacles01.Count;
                                    break;
                                }
                            case 2:
                                {
                                    setCount = this._randomObstacles02.Count;
                                    break;
                                }
                            case 4:
                                {
                                    setCount = this._randomObstacles04.Count;
                                    break;
                                }
                            case 8:
                                {
                                    setCount = this._randomObstacles08.Count;
                                    break;
                                }
                        }
                        version = randomNumberGenerator.Next(1, setCount+1);
                    }
                    else
                    {
                        version = int.Parse(pageObstaclesEntity.ModelName.Substring(8, 2));
                    }

                    // Grab our set of random obstacles
                    IList<RandomObstaclesEntity> set = null;
                    switch (count)
                    {
                        case 1:
                            {
                                set = this._randomObstacles01[version];
                                break;
                            }
                        case 2:
                            {
                                set = this._randomObstacles02[version];
                                break;
                            }
                        case 4:
                            {
                                set = this._randomObstacles04[version];
                                break;
                            }
                        case 8:
                            {
                                set = this._randomObstacles08[version];
                                break;
                            }
                    }

                    // Inject our random obstacles with height variation if specified
                    foreach (var randomObstacle in set)
                    {
                        // Construct a PageObstaclesEntity from our RandomObstaclesEntity
                        // IMPORTANT: Note the details for obstacle number, proper X placement etc.
                        var randomPageObstaclesEntity = new PageObstaclesEntity
                            {
                                PageNumber = pageObstaclesEntity.PageNumber,
                                LineNumber = pageObstaclesEntity.LineNumber,
                                ObstacleNumber = currentObstacleNumber++,
                                ModelName = randomObstacle.ModelName,
                                ObstacleType = randomObstacle.ObstacleType,
                                LogicalXScaledTo100 = currentLogicalXScaledTo100 + randomObstacle.LogicalXScaledTo100,
                                LogicalHeightScaledTo100 = randomObstacle.LogicalHeightScaledTo100,
                                LogicalScaleScaledTo100 = randomObstacle.LogicalScaleScaledTo100,
                                LogicalAngle = randomObstacle.LogicalAngle,
                                IsGoal = randomObstacle.IsGoal
                            };


                        if (randomObstacle.HeightRange != 0)
                        {
                            var randomHeightAdjustment = randomNumberGenerator.Next(
                                -randomObstacle.HeightRange,
                                randomObstacle.HeightRange + 1);
                        }
                        returnEntities.Add(randomPageObstaclesEntity);
                    }
                }
                else
                {
                    returnEntities.Add(pageObstaclesEntity);
                }
            }

            return returnEntities;
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
                                   (0.5f * lineModel.WorldDepth);                    // Move forward to line depth halfway mark
                if (obstacleModel.TheObstacleType == ObstacleType.SimpleBottom)
                {
                    halfwayDepth += 0.5f*obstacleModel.WorldDepth;                 // Add in 1/2 of obstacle depth so obstacle
                                                                                   // will be positioned exactly straddling line deph halfway
                }
                else if (obstacleModel.TheObstacleType == ObstacleType.SimpleTop)
                {
                    halfwayDepth -= 0.5f * obstacleModel.WorldDepth;                // IMPORTANT: Remove 1/2 of obstacle depth so obstacle
                                                                                    // will be positioned exactly straddling line deph halfway
                                                                                    // because the model and hence its origin has been flipped
                }

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
            // IMPORTANT: We have already accounted for any scaling when we first loaded the vertices
            // and that is recorded into our WorldMatrix. Hence, even though we pull original unscaled 
            // vertices from model, the 
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

        // Create the a set of dictionaries to hold entries for our defined
        // random obstacle sets.
        private void InitializeRandomObstacles()
        {
            // Initialize our dictionaries to hold our random obstacle sets
            //                  Count             Version Set
            this._randomObstacles01 = new Dictionary<int, IList<RandomObstaclesEntity>>();
            this._randomObstacles02 = new Dictionary<int, IList<RandomObstaclesEntity>>();
            this._randomObstacles04 = new Dictionary<int, IList<RandomObstaclesEntity>>();
            this._randomObstacles08 = new Dictionary<int, IList<RandomObstaclesEntity>>();

            // Pull in our defined random obstacle sets
            var randomRepository = new RandomObstaclesRepository();
            var randomObstacles = randomRepository.GetAllRandomObstacles();

            // Loop over all defined random obstacles
            // IMPORTANT:
            // 1. We depend upon the result set ordered by the column RandomObstaclesSet
            // 2. We depend upon RandomObstaclesSet to be named as follows:
            //    "Random"<count of goals><version of this set of count of goals>
            // Examples: 
            // Random0101 - Specifies the set representing 1 goal, first version of this category
            // Random0403 - Specifies the set representing 4 goasls, third version of this category
            //
            // Also note that in PageObstaclesRepository, this may be specified as Random<count>x
            // (e.g., Random04x). In this case, we are specifing choose a random set from the count
            // 4 category.
            foreach(var randomObstacle in randomObstacles)
            {
                // Grab the "count" and "version" for this random obstacle entry
                var currentSet = randomObstacle.RandomObstaclesSet;
                var currentCount = int.Parse(currentSet.Substring(6, 2));
                var currentVersion = int.Parse(currentSet.Substring(8, 2));

                // Determine which "count" dictionary to update, 
                // creating the List to hold the "version" if needed
                switch (currentCount)
                {
                    // We have an entry for the a set with a "count" of 1
                    case 1:
                        {
                            // Create dictionary for "count" if needed
                            if (!this._randomObstacles01.ContainsKey(currentVersion))
                            {
                                this._randomObstacles01[currentVersion] = new List<RandomObstaclesEntity>();
                            }

                            // Update the List for this "version" of "count"
                            this._randomObstacles01[currentVersion].Add(randomObstacle);
                            break;
                        }
                    case 2:
                        {
                            if (!this._randomObstacles02.ContainsKey(currentVersion))
                            {
                                this._randomObstacles02[currentVersion] = new List<RandomObstaclesEntity>();
                            }
                            this._randomObstacles02[currentVersion].Add(randomObstacle);
                            break;
                        }
                    case 4:
                        {
                            if (!this._randomObstacles04.ContainsKey(currentVersion))
                            {
                                this._randomObstacles04[currentVersion] = new List<RandomObstaclesEntity>();
                            }
                            this._randomObstacles04[currentVersion].Add(randomObstacle);
                            break;
                        }
                    case 8:
                        {
                            if (!this._randomObstacles08.ContainsKey(currentVersion))
                            {
                                this._randomObstacles08[currentVersion] = new List<RandomObstaclesEntity>();
                            }
                            this._randomObstacles08[currentVersion].Add(randomObstacle);
                            break;
                        }
                }
            }
        }

        #endregion

    }
}