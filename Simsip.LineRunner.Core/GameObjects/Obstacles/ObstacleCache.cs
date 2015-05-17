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
using Simsip.LineRunner.GameObjects.Sensors;
using BEPUphysics;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#else
using System.Threading;
#endif
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
using Simsip.LineRunner.GameObjects.ParticleEffects;
#else
using System.Collections.Concurrent;
using Simsip.LineRunner.GameObjects.ParticleEffects;
#endif


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    public class ObstacleCache : GameComponent, IObstacleCache
    {
        // Required services
        private IInputManager _inputManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private ICharacterCache _characterCache;
        private ISensorCache _sensorCache;
        private IParticleEffectCache _particleEffectCache;
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

        // Support for staging the results of asynchronous loads and then signaling
        // we need the results processed on the next update cycle
        private class LoadContentThreadArgs
        {
            public LoadContentAsyncType TheLoadContentAsyncType;
            public GameState TheGameState;
            public int PageNumber;
            public int[] LineNumbers;
            public IList<ObstacleModel> ObstacleModelsAsync;
        }
        private ConcurrentQueue<LoadContentThreadArgs> _loadContentThreadResults;

        // Identifies if we are injecting a random set of obstacles
        private const string RandomPrefix = "Random";

        // Used to create our random selections
        private Random _randomNumberGenerator;

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

        #region GameComponent Overrides

        public override void Initialize()
        {
            // Iniitialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;
            this.ObstacleModels = new List<ObstacleModel>();
            this._loadContentThreadResults = new ConcurrentQueue<LoadContentThreadArgs>(); 
            this._obstacleHitList = new List<ObstacleModel>();
            this.InitializeRandomObstacles();

            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));
            this._lineCache = (ILineCache)this.Game.Services.GetService(typeof(ILineCache));
            this._characterCache = (ICharacterCache)this.Game.Services.GetService(typeof(ICharacterCache));
            this._sensorCache = (ISensorCache)this.Game.Services.GetService(typeof(ISensorCache));
            this._particleEffectCache = (IParticleEffectCache)this.Game.Services.GetService(typeof(IParticleEffectCache));
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
            this._pageObstaclesRepository = new PageObstaclesRepository();
            this._obstacleRepository = new ObstacleRepository();

            // Initialize our random number generator
            this._randomNumberGenerator = new Random();

            // Initialize our drawing filter
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            base.Initialize();
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

            // Did we signal we need an async content load processed?
            if (this._loadContentThreadResults.Count > 0)
            {
                LoadContentThreadArgs loadContentThreadArgs = null;
                if (this._loadContentThreadResults.TryDequeue(out loadContentThreadArgs))
                {
                    // Load in new content from staged collection in args
                    ProcessLoadContentAsync(loadContentThreadArgs);

                    // Did anyone need to know we finished?
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

        #region IObstacleCache Implementation

        public event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        public void Draw(StockBasicEffect effect, EffectType type)
        {
            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            var frustrum = new BoundingFrustum(view * projection);
            this._ocTreeRoot.Draw(view, projection, frustrum, effect, type);
        }

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

                        // Get initial obstacles constructed for first page in background.
                        // this.ProcessNextLine() will be called in event handler when
                        // finished to:
                        // 1. Animate first line's obstacles into position
                        // 2. Disable previous line physics (n/a)
                        // 3. Enable current line physics
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Initialize, state);

                        break;
                    }
                case GameState.Moving:
                    {
                        // Propogate state change
                        this._characterCache.SwitchState(state);
                        this._sensorCache.SwitchState(state);
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

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Next, state);

                        // Propogate state change
                        this._sensorCache.SwitchState(state);
                        this._characterCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // 1. Animate first line's obstacles into position
                        // 2. Disable previous line physics
                        // 3. Enable current line physics
                        this.ProcessNextLine();

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Next, state);

                        // Propogate state change
                        this._sensorCache.SwitchState(state);
                        this._characterCache.SwitchState(state);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // In background, get initial obstacles constructed for first page.
                        // this.ProcessNextLine() will be called in event handler when 
                        // finished to:
                        // 1. Animate first line's obstacles into position
                        // 2. Disable previous line physics
                        // 3. Enable current line physics
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Initialize, state);

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // In background get initial obstacles constructed for first page.
                        // this.ProcessNextLine() will be called in event handler when
                        // finished to:
                        // 1. Animate first line's obstacles into position
                        // 2. Disable previous line physics
                        // 3. Enable current line physics
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
                        this._characterCache.SwitchState(state);
                        this._sensorCache.SwitchState(state);

                        break;
                    }
            }
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

        #region Event Handlers

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            this.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;


            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Initialize ||
                args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                //
                // IMPORTANT: This needs to be on Update() thread as we will be
                // removing/adding physics objects in ProcessNextLine/ProcessObstaclePhysics
                //
                this.ProcessNextLine();

                // Propogate state change
                this._sensorCache.SwitchState(args.TheGameState);
                this._characterCache.SwitchState(args.TheGameState);
            }
        }

        #endregion

        #region Helper Methods

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
                        pageNumber = 1;
                        lineNumbers = new int[] { 1, 2 };
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
                ObstacleModelsAsync = new List<ObstacleModel>(),
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
            var lineNumbers = loadContentThreadArgs.LineNumbers;
            var obstacleModels = loadContentThreadArgs.ObstacleModelsAsync;

            // Ok, let's grab the collection of obstacles for our current page using our helper
            // function that knows how to inject entries coded for random sets
            var pageObstaclesEntities = this.HydrateLineObstacles(pageNumber, lineNumbers);
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
                // if (obstacleModel.TheObstacleType == ObstacleType.SimpleBottom)
                // {
                    // Clip everything below top of bottom line
                    var bottomDistance = lineModel.WorldOrigin.Y + lineModel.WorldHeight;
                    obstacleModel.BottomClippingPlane = new Vector4(Vector3.Up, -bottomDistance);
                // }
                // else if (obstacleModel.TheObstacleType == ObstacleType.SimpleTop)
                // {
                    // Clip everything above bottom of top line
                    var topDistance = lineModel.WorldOrigin.Y + this._pageCache.CurrentPageModel.WorldLineSpacing;
                    obstacleModel.TopClippingPlane = new Vector4(Vector3.Down, topDistance);
                // }

                // Uniquely identify character for octree
                obstacleModel.ModelID = pageObstaclesEntity.ObstacleNumber;

                // Record to our staged collection
                obstacleModels.Add(obstacleModel);

                // IMPORTANT: Only add/remove physics in ProcessNextLine()/ProcessObstaclePhysics

            }

            this._loadContentThreadResults.Enqueue(loadContentThreadArgs);
        }

        // Migrate staged collection in args to public collection
        private void ProcessLoadContentAsync(LoadContentThreadArgs loadContentThreadArgs)
        {
            // If we are moving to first/next page, clear out previous page
            if (this._currentLineNumber == 1)
            {
                // Remove all previous obstacle models from our drawing filter
                // and remove all previous obstacle physics
                foreach (var obstacleModel in this.ObstacleModels.ToList())
                {
                    this._ocTreeRoot.RemoveModel(obstacleModel.ModelID);

                    if (obstacleModel.PhysicsEntity != null &&
                        obstacleModel.PhysicsEntity.Space != null)
                    {
                        this._physicsManager.TheSpace.Remove(obstacleModel.PhysicsEntity);
                    }

                }

                // And clear out the full set of previous models
                this.ObstacleModels.Clear();
            }

            // Populate our public model collections from our staged collections
            foreach (var obstacleModel in loadContentThreadArgs.ObstacleModelsAsync)
            {
                // Add to octree for filtered drawing
                this._ocTreeRoot.AddModel(obstacleModel);

                this.ObstacleModels.Add(obstacleModel);
            }

            //
            // IMPORTANT: Physics is handled in ProcessNextLine/ProcessObstaclePhysics
            //
        }

        private List<PageObstaclesEntity> HydrateLineObstacles(int pageNumber, int[] lineNumbers)
        {
            var returnEntities = new List<PageObstaclesEntity>();

            // Get the set of page obstacle entries for the page/line we are processing
            var pageObstaclesEntities = _pageObstaclesRepository.GetObstacles(pageNumber, lineNumbers);

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
                        switch (count)
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
                        version = this._randomNumberGenerator.Next(1, setCount + 1);
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

                    // Can we add in an additional random height adjustment?
                    // IMPORTANT: We can only modify by height adjustment using one height
                    // value across all obstacles in this set. This is because we want the
                    // Bottom/Top to vary so that the gap between them remains the same.
                    // To do this we add in a single height adjustment value to the bottom and subtract
                    // a single height adjustment value from the top.
                    // We take the first entry in the random obstacle set to define our
                    // height adjustment we will use for the entire set.
                    float heightAdjustment = 0f;
                    var heightRangeEntry = set[0];
                    if (heightRangeEntry.HeightRange != 0)
                    {
                        // Grab a an amount to adjust the height within the range [-HeightRange, HeightRange]
                        // Example:
                        // LogicalHeightScaledTo100 = 30, HeightRange = 4
                        // LogicalHeightScaledTo100 will be assigned a value in the range 26 to 34
                        heightAdjustment = this._randomNumberGenerator.Next(
                            -heightRangeEntry.HeightRange,
                            heightRangeEntry.HeightRange + 1);
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
                            DisplayParticle = randomObstacle.DisplayParticle,
                            IsGoal = randomObstacle.IsGoal
                        };

                        // Now add in our height adjustement - see comment write-up above heightAdjustment for more details
                        var theObstacleType = (ObstacleType)Enum.Parse(typeof(ObstacleType), randomPageObstaclesEntity.ObstacleType);
                        if (theObstacleType == ObstacleType.SimpleBottom)
                        {
                            randomPageObstaclesEntity.LogicalHeightScaledTo100 += heightAdjustment;
                        }
                        else if (theObstacleType == ObstacleType.SimpleTop)
                        {
                            randomPageObstaclesEntity.LogicalHeightScaledTo100 -= heightAdjustment;
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

        // 1. Disable previous line physics
        // 2. Animate next line's obstacles into position
        // 3. Enable current line physics
        // IMPORTANT: This needs to be on Update() thread as we will be
        // removing/adding physics objects
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
                .Where(x => x.ThePageObstaclesEntity.LineNumber == this._currentLineNumber)
                .ToList();

            // Animate next line's obstacles into position
            int obstacleCount = currentLineObstacles.Count();
            for(int i = 0; i < obstacleCount; i++)
            {
                var obstacleModel = currentLineObstacles[i];

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

                // Do we get an additional animation?
                var obstacleAnimation = DetermineAnimation(i, currentLineObstacles);

                // Stitch together full set of animations to bring obstacle to life and assign to obstacle to run
                Actions.Action obstacleAction = null;
                if (obstacleAnimation != null)
                {
                    obstacleAction = new Sequence(new FiniteTimeAction[] { obstacleMoveTo, /*obstacleProcessPhysics,*/ obstacleAnimation });
                }
                else
                {
                    obstacleAction = new Sequence(new FiniteTimeAction[] { obstacleMoveTo /*, obstacleProcessPhysics*/ });
                }
                obstacleModel.ModelRunAction(obstacleAction);

                // 1. Grab our definition for the point cloud for this obstacle
                // 2. Loop over the point cloud and multiply via our world matrix for this obstacle
                // IMPORTANT: Note the 2 ToList calls so that we don't modify original point cloud and
                //            we can modify our copy of the point cloud.
                var convexHull = ObstacleConvexHulls.ConvexHullTable[obstacleModel.TheModelEntity.ModelName].ToList();
                var pointCounter = 0;
                var worldMatrix = ConversionHelper.MathConverter.Convert(obstacleModel.WorldMatrix);
                foreach (var point in convexHull.ToList())
                {
                    var origPoint = convexHull[pointCounter];
                    var newPoint = BEPUutilities.Vector3.Zero;
                    BEPUutilities.Matrix.Transform(ref origPoint, ref worldMatrix, out newPoint);
                    convexHull[pointCounter] = newPoint;
                    pointCounter++;
                }

                // Now construct our convexHull with the appropriately transformed point cloud
                var physicsMesh = new ConvexHull(convexHull);

                // IMPORTANT: Immediately determine our offset to the physics center so we can use this if we 
                //            need to modify position of physics entity (e.g., when animating)
                var physicsLocalTransform = physicsMesh.Position - ConversionHelper.MathConverter.Convert(obstacleModel.WorldOrigin);
                obstacleModel.PhysicsLocalTransform = BEPUutilities.Matrix.CreateTranslation(physicsLocalTransform);

                // Assign references between 3d model and physics entity
                obstacleModel.PhysicsEntity = physicsMesh;
                physicsMesh.Tag = obstacleModel;

                // Hook up collision handling
                obstacleModel.PhysicsEntity.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;

                // And finally add to the physics world
                this._physicsManager.TheSpace.Add(physicsMesh);
            }
        }

        private FiniteTimeAction DetermineAnimation(int index, List<ObstacleModel> currentLineObstacles)
        {
            // TESTING: Uncomment for testing
            if (index != 0)
            {
                return null;
            }

            FiniteTimeAction returnAction = null;

            var currentObstacle = currentLineObstacles[index];

            // Do our predecessor check first
            if (index != 0 &&
                (currentObstacle.TheObstacleAnimationType == ObstacleAnimationType.SimpleTranslateX ||
                 currentObstacle.TheObstacleAnimationType == ObstacleAnimationType.SimpleTranslateY) )
            {
                // Do we have a predecessor at the same x location we should copy the animation from
                var predecessor = currentLineObstacles[index - 1];
                if (predecessor.ThePageObstaclesEntity.LogicalXScaledTo100 == currentObstacle.ThePageObstaclesEntity.LogicalXScaledTo100)
                {
                    // If we tagged this predecessor for copying, short-circuit by returning copy
                    var predecessorAction = predecessor.ModelActionManager.GetActionByTag(predecessor.ModelID, predecessor);
                    if (predecessorAction != null)
                    {
                        return predecessorAction.Copy() as FiniteTimeAction;
                    }
                }
            }

            // Ok, normal assignment logic, based on page number we will assign an increasingly higher frequency of animations
            // up to 6th page where we top out at 50% frequency of assigning an animation to an obstacle
            // (Tip: Map out several current page numbers to see how this works
            // TESTING: Comment out below for testing
            /*
            var exclusiveUpperBound  = 3;
            var divisor = 2;
            if (this._currentPageNumber < 7)
            {
                exclusiveUpperBound =  9 - this._currentPageNumber;
                divisor = 8 - this._currentPageNumber;
            }
            var qualifiesForAnimation = this._randomNumberGenerator.Next(1,exclusiveUpperBound) % divisor;

            // If we didn't get a hit based on page frequency, return null to indicate this
            if (qualifiesForAnimation != 0)
            {
                return null;
            }
            */

            // Do we want some type of particle effect with this obstacle?
            CallFunc particleAction = null;
            if (!string.IsNullOrEmpty(currentObstacle.ThePageObstaclesEntity.DisplayParticle))
            {
                particleAction = new CallFunc( () =>
                      this._particleEffectCache.AddDisplayParticleEffect(currentObstacle)
                    );
            }

            // Ok we qualify for an animation, let's randomly get one from our enum
            // TESTING: Comment out below for testing
            /*
            var enumCount = Enum.GetNames(typeof(ObstacleAnimationType)).Length;
            var randomEnum = this._randomNumberGenerator.Next(1,enumCount+1);       // Excludes ObstacleAnimationType.None
            currentObstacle.TheObstacleAnimationType = (ObstacleAnimationType)randomEnum;
            */
            // TESTING: Uncomment for testing
            currentObstacle.TheObstacleAnimationType = ObstacleAnimationType.SimpleRotateYaw;

            switch (currentObstacle.TheObstacleAnimationType)
            {
                case ObstacleAnimationType.SimpleRotatePitch:
                    {
                        var interval = GameConstants.DURATION_ANIMATE_OBSTACLE_SLOW / 4f;

                        var rotateByForward = new RotateBy(
                            interval,
                            0f,
                            45f,
                            0f);
                        var rotateByBack = new RotateBy(
                            2f * interval,
                            0f,
                            -90f,
                            0f);
                        var rotateBySequence = new Sequence(new FiniteTimeAction[] 
                            { 
                                rotateByForward, 
                                rotateByBack,
                                rotateByForward
                            });
                        returnAction = new RepeatForever(rotateBySequence);

                        break;
                    }
                case ObstacleAnimationType.SimpleRotateRoll:
                    {
                        var interval = GameConstants.DURATION_ANIMATE_OBSTACLE_SLOW / 4f;

                        var rotateByCW = new RotateBy(          // Clock wise
                            interval,
                            0f,
                            0f,
                            45f);
                        var rotateByCCW = new RotateBy(         // Counter clock wise
                            2f * interval,
                            0f,
                            0f,
                            -90f);
                        var rotateBySequence = new Sequence(new FiniteTimeAction[] 
                            { 
                                rotateByCW, 
                                rotateByCCW,
                                rotateByCW
                            });
                        returnAction = new RepeatForever(rotateBySequence);

                        break;
                    }
                case ObstacleAnimationType.SimpleRotateYaw:
                    {
                        var interval = GameConstants.DURATION_ANIMATE_OBSTACLE_SLOW / 4f;

                        var rotateByRight = new RotateBy(
                            interval,
                            30f,
                            0f,
                            0f);
                        var rotateByLeft = new RotateBy(
                            2f * interval,
                            -60f,
                            0f,
                            0f);
                        Sequence rotateBySequence = null;
                        if (particleAction != null)
                        {
                            rotateBySequence = new Sequence(new FiniteTimeAction[] 
                            { 
                                rotateByRight, 
                                particleAction,
                                rotateByLeft,
                                particleAction,
                                rotateByRight
                            });
                        }
                        else
                        {
                            rotateBySequence = new Sequence(new FiniteTimeAction[] 
                            { 
                                rotateByRight, 
                                rotateByLeft,
                                rotateByRight
                            });
                        }
                        returnAction = new RepeatForever(rotateBySequence);

                        break;
                    }
                case ObstacleAnimationType.SimpleScale:
                    {
                        // Not sure about this one yet as can we scale our physics representation?
                        break;
                    }
                case ObstacleAnimationType.SimpleTranslateX:
                    {
                        // Make delta for animation based on line length
                        // IMPORTANT: We are using page width here instead of line length as it is easily accessible - may have to revisit
                        // Use random delta of animation based on page which increases (1.0% for page 1, 2.0% for page 2, level off at 4.0%)
                        var level = Math.Min(this._currentPageNumber*10 + 1, 41);
                        var randomLevel = this._randomNumberGenerator.Next(10,level);
                        float delta = this._pageCache.CurrentPageModel.WorldWidth * (level/1000f);

                        var interval = GameConstants.DURATION_ANIMATE_OBSTACLE_SLOW / 4f;
                        var moveByRight = new MoveBy(interval, new Vector3(delta, 0, 0));
                        var moveByLeft = new MoveBy(2f * interval, new Vector3(-2f * delta, 0, 0));
                        var moveBySequence = new Sequence(new FiniteTimeAction[] { moveByRight, moveByLeft, moveByRight });
                        returnAction = new RepeatForever(moveBySequence);

                        // Tag in case we we are setting up a paired set of animations
                        returnAction.Tag = currentLineObstacles[index].ModelID;

                        break;
                    }
                case ObstacleAnimationType.SimpleTranslateY:
                    {
                        // Make delta for animation based on linespacing
                        // Use random delta of animation based on page which increases 
                        // (10% for page 1, 10-20% for page 2, level off at 10-60%)
                        var level = Math.Min(this._currentPageNumber * 10 + 1, 61);
                        var randomLevel = this._randomNumberGenerator.Next(10, level);
                        float delta = this._pageCache.CurrentPageModel.WorldLineSpacing * (randomLevel / 100f);

                        var interval = GameConstants.DURATION_ANIMATE_OBSTACLE_SLOW / 4f;
                        var moveByUp = new MoveBy(interval, new Vector3(0, delta, 0));
                        var moveByDown = new MoveBy(2f * interval, new Vector3(0, -2f * delta, 0));
                        var moveBySequence = new Sequence(new FiniteTimeAction[] { moveByUp, moveByDown, moveByUp });
                        returnAction = new RepeatForever(moveBySequence);

                        // Tag in case we we are setting up a paired set of animations
                        returnAction.Tag = currentLineObstacles[index].ModelID;

                        break;
                    }
            }

            return returnAction;
        }

        // TODO: Leaving in as example of this content pipeline component
        /*
        private void ProcessObstaclePhysics(ObstacleModel obstacleModel)
        {
            // IMPORTANT: We have already accounted for any scaling when we first loaded the vertices
            // and that is recorded into our WorldMatrix. Hence, even though we pull original unscaled 
            // vertices from model, the
            var physicsTransform = new BEPUutilities.AffineTransform()
            {
                Matrix = ConversionHelper.MathConverter.Convert(obstacleModel.WorldMatrix)
            };
            var physicsVertices = obstacleModel.XnaModel.Tag as PhysicsModelVertices;

            // TODO: Sometimes we come in with a nondivisible by 3 indices count. This will
            // throw an invalid index exception when attemptint to create a MobileMesh as it
            // walks the indices in sets of 3. This work-around for now justs truncates
            // the indices to be divisible by 3.
            // Example: Gramophone01
            var indicesModulus = physicsVertices.Indices.Count() % 3;
            if (indicesModulus != 0)
            {
                var sliceCount = physicsVertices.Indices.Count() - indicesModulus;
                var tempIndices = new int[sliceCount];
                Array.Copy(physicsVertices.Indices, tempIndices, sliceCount);
                physicsVertices.Indices = new int[sliceCount];
                Array.Copy(tempIndices, physicsVertices.Indices, sliceCount);
            }

            // var physicsMesh = new MobileMesh(physicsVertices.PhysicsVertices, physicsVertices.Indices, physicsTransform, MobileMeshSolidity.Counterclockwise);
        }
        */

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