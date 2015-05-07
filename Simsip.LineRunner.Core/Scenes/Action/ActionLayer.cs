using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysicsDrawer.Lines;
using Cocos2D;
using ConversionHelper;
using Engine.Assets;
using Engine.Chunks;
using Engine.Debugging;
using Engine.Debugging.Graphs;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Input;
using Engine.Interface;
using Engine.Sky;
using Engine.Universe;
using Engine.Water;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.GameObjects.Sensors;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Scenes.Hud;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using Engine.Blocks;
using Engine.Audio;
using System.Diagnostics;


namespace Simsip.LineRunner.Scenes.Action
{
    public class ActionLayer : GameLayer
    {
        // The parent scene to all the layers
        private CoreScene _parent;

        // Our current action state
        private GameState _currentGameState;

        // Our current line and page number we are on
        private int _currentLineNumber;
        private int _currentPageNumber;

        // Control our first time fly by upon startup
        private bool _firstTimeSetupPage;
        private FlyBy _currentFlyBy;

        // Allows us to create an appropriate time input to XNA game components
        // from our Cocos2d framework
        private float _totalGameTime;
        private GameTime _gameTime;

        // Required Voxeliq services
        private IAssetManager _assetManager;
        private ISkyDome _skyDome;
        private INewSky _newSky;
        private IChunkCache _chunkCache;
        private IInputManager _inputManager;
        private IWorld _world;
        private IWaterCache _waterCache;
        private IUserInterface _userInterface;
        private IInGameDebuggerService _inGameDebugger;
        private IStatistics _debugBar;
        private IGraphManager _graphManager;
        private IAudioManager _audioManager;
        
        // Required game services
        private IPhysicsManager _physicsManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private IObstacleCache _obstacleCache;
        private ICharacterCache _characterCache;
        private IParticleEffectCache _particleCache;
        private ISensorCache _sensorCache;
        private IPaneCache _paneCache;
        private IDeferredShadowMapping _deferredShadowMapping;

        // Support for communicating score to ui layer
        private HudLayer _hudLayer;

        // Manages periodic adjustments in update cycle
        private double _timeAccumulator = 0;

        public ActionLayer(CoreScene parent)
        {
            this._parent = parent;

            // Hook-up initial event handlers we'll need
            _parent.SwitchedUI += OnSwitchingUI;
        }

        #region Properties

        /// <summary>
        /// We use this one-time flag to signal that the page layer has reached a ready state
        /// (e.g., for update/draw calls for instance)
        /// </summary>
        public bool Ready { get; private set; }

        #endregion

        #region Api

        public void Refresh()
        {
            this.SwitchState(GameState.Refresh);
        }

        public void StartWorld()
        {
            this.SwitchState(GameState.World);
        }

        public void ResumeFromWorld()
        {
            this.SwitchState(GameState.Moving);
        }

        #endregion

        #region Event handlers

        private void OnSwitchingUI(object sender, SwitchUIEventArgs e)
        {
            switch (e.NewLayer)
            {
                case LayerTags.HudLayer:
                    {
                        SwitchState(GameState.Moving);
                        break;
                    }
                case LayerTags.AchievementsLayer:
                case LayerTags.CreditsMasterLayer:
                case LayerTags.OptionsMasterLayer:
                case LayerTags.StartPage1Layer:
                    {
                        SwitchState(GameState.Start);
                        break;
                    }
                case LayerTags.StartPage2Layer:
                    {
                        // Immediately unhook any fly-bys in progress
                        this._currentFlyBy.FlyByFinished -= OnCurrentFlyByFinished;
                        this._currentFlyBy = null;

                        // If so, see HudLayer.Draw for how to keep ui positioned correctly
                        // SwitchState(GameState.MovingToStart);

                        SwitchState(GameState.Start);
                        break;
                    }
                case LayerTags.FinishLayer:
                    {
                        // TODO: Keeping it simple now, will consider animation later
                        // If so, see HudLayer.Draw for how to keep ui positioned correctly
                        // SwitchState(GameState.MovingToStart);
                        SwitchState(GameState.Start);

                        break;
                    }
            }

        }

        private void OnCurrentFlyByFinished(object sender, FlyByFinishedEventArgs e)
        {
            // Immediately unhook to make this a one-shot event
            this._currentFlyBy.FlyByFinished -= OnCurrentFlyByFinished;
            this._currentFlyBy = null;

            if (e.NextFlyBy != null)
            {
                this._currentFlyBy = e.NextFlyBy;
                return;
            }

            switch(this._currentGameState)
            {
                case GameState.Intro:
                    {
                        // Intro is finished, display start screen
                        // _parent.Navigate(LayerTags.StartPage1Layer);
                        break;
                    }
                case GameState.MovingToNextLine:
                case GameState.MovingToNextPage:
                    {
                        // Moving to next line/page is finished while in-game, get
                        // hero moving again
                        this.SwitchState(GameState.Moving);
                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Moving to start has finished (e.g., from finish layer)

                        // IMPORTANT: Turn on ability to start game now that flyby created
                        // is finished. This precaution is in place in case user quickly
                        // exits from finish screen while flyby is in process.
                        //
                        // Currently, a complete flyby is important as it will position hero
                        // correctly for a clean start
                        this._parent.TheStartPage1Layer.EnableStart(true);

                        break;
                    }
            }
        }

        private void OnSensorHit(object sender, SensorHitEventArgs e)
        {
            var sensorModel = e.TheSensorModel;

            if (e.TheSensorModel is GoalSensorModel)
            {
                // TODO: Cache string for faster reference
                SoundUtils.PlaySoundEffect(_assetManager.GetSound(Asset.SoundHeroPoint));

                GameManager.SharedGameManager.CurrentScore++;
                _hudLayer.DisplayScore(GameManager.SharedGameManager.CurrentScore);
            }
            else if (e.TheSensorModel is MarginSensorModel)
            {
                if (this._currentLineNumber != this._pageCache.CurrentPageModel.ThePadEntity.LineCount)
                {
                    this.SwitchState(GameState.MovingToNextLine);
                }
                else
                {
                    this.SwitchState(GameState.MovingToNextPage);
                }
            }

        }

        private void OnObstacleHit(object sender, ObstacleHitEventArgs e)
        {
            // Don't let hits outside of game play sneak in
            if (this._currentGameState != GameState.Moving)
            {
                return;
            }

            // Play particle effect animation
            this._particleCache.AddParticleEffect(e.TheObstacleModel, e.TheObstacleModel.TheContact);

            // Play glow animation
            var glowUpAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Color.White, 0.1f);
            var glowDownAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Color.White, 0.0f);
            var turnOffTint = new CallFunc(() => { e.TheObstacleModel.IsTinted = false; });
            var glowAction = new Sequence(new FiniteTimeAction[] 
                { 
                    glowUpAction, 
                    glowDownAction,
                    turnOffTint
                });
            e.TheObstacleModel.ModelRunAction(glowAction);

            // Are we allowing kills?
            if (GameManager.SharedGameManager.AdminIsKillAllowed)
            {
                // Don't let touches sneak in
                this.TouchEnabled = false;

                // Move us out of gameplay
                this.SwitchState(GameState.MovingToStart);

                // Animate hero being killed then determine where to navigate to in callback
                this._characterCache.HandleKill(
                    e.TheObstacleModel.TheContact,
                    () => HandleMoveToFinish());
            }
        }

        private void OnLineHit(object sender, LineHitEventArgs e)
        {
            // Don't let hits outside of game play sneak in
            if (this._currentGameState != GameState.Moving)
            {
                return;
            }

            // Play particle effect animation
            this._particleCache.AddParticleEffect(e.TheLineModel, e.TheLineModel.TheContact);

            // Play glow animation
            var glowUpAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Color.White, 0.1f);
            var glowDownAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Color.White, 0.0f);
            var turnOffTint = new CallFunc(() => { e.TheLineModel.IsTinted = false; });
            var glowAction = new Sequence(new FiniteTimeAction[] 
                { 
                    glowUpAction, 
                    glowDownAction,
                    turnOffTint
                });
            e.TheLineModel.ModelRunAction(glowAction);

            // Are we allowing kills?
            if (GameManager.SharedGameManager.AdminIsKillAllowed)
            {
                // Don't let touches sneak in
                this.TouchEnabled = false;

                // Move us out of gameplay
                this.SwitchState(GameState.MovingToStart);

                // Animate hero being killed then determine where to navigate to in callback
                this._characterCache.HandleKill(
                    e.TheLineModel.TheContact,
                    () => HandleMoveToFinish());
            }
        }

        #endregion

        #region CCLayer Framework Overrides

        public override bool Init()
        {
            if (!base.Init())
            {
                return false;
            }

            // Set state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;

            // Dummy up an XNA gametime for us convert Cocos2d times into
            _gameTime = new GameTime();

            // Schedule Update/Draw to be called
            this.ScheduleUpdate();

            return true;
        }

        public override void Update(float dt)
        {
            if (!this.Ready &&                               // If we have not flipped our one-time flag that says we are ready
                GameManager.SharedGameManager.ServicesReadyForUpdate)   // AND the game services are ready to be referenced
            {
                // Get references to services we need
                this._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));
                this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
                this._skyDome = (ISkyDome)TheGame.SharedGame.Services.GetService(typeof(ISkyDome));
                this._newSky = (INewSky)TheGame.SharedGame.Services.GetService(typeof(INewSky));
                this._chunkCache = (IChunkCache)TheGame.SharedGame.Services.GetService(typeof(IChunkCache));
                this._world = (IWorld)TheGame.SharedGame.Services.GetService(typeof(IWorld));
                this._waterCache = (IWaterCache)TheGame.SharedGame.Services.GetService(typeof(IWaterCache));
                this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
                this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
                this._lineCache = (ILineCache)TheGame.SharedGame.Services.GetService(typeof(ILineCache));
                this._obstacleCache = (IObstacleCache)TheGame.SharedGame.Services.GetService(typeof(IObstacleCache));
                this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));
                this._particleCache = (IParticleEffectCache)TheGame.SharedGame.Services.GetService(typeof(IParticleEffectCache));
                this._sensorCache = (ISensorCache)TheGame.SharedGame.Services.GetService(typeof(ISensorCache));
                this._paneCache = (IPaneCache)TheGame.SharedGame.Services.GetService(typeof(IPaneCache));
                this._deferredShadowMapping = (IDeferredShadowMapping)TheGame.SharedGame.Services.GetService(typeof(IDeferredShadowMapping));
                this._userInterface = (IUserInterface)TheGame.SharedGame.Services.GetService(typeof(IUserInterface));
                this._inGameDebugger = (IInGameDebuggerService)TheGame.SharedGame.Services.GetService(typeof(IInGameDebuggerService));
                this._debugBar = (IStatistics)TheGame.SharedGame.Services.GetService(typeof(IStatistics));
                this._graphManager = (IGraphManager)TheGame.SharedGame.Services.GetService(typeof(IGraphManager));
                this._audioManager = (IAudioManager)TheGame.SharedGame.Services.GetService(typeof(IAudioManager));

                // Register event handlers
                this._lineCache.LineHit += OnLineHit;
                this._obstacleCache.ObstacleHit += OnObstacleHit;
                this._sensorCache.SensorHit += OnSensorHit;

                // Flip our one-time flag that says we are ready (e.g., for Update/Draw)
                this.Ready = true;

#if STOPWATCH
                Program.TheStopwatch.Stop();
                Debug.WriteLine("ActionLayer.Ready: " + Program.TheStopwatch.ElapsedMilliseconds);
                Program.TheStopwatch.Restart();
#endif

            }

            // We are not ready to update/draw so just return until we are
            if (!this.Ready)
            {
                return;
            }

            // Take care of any animations in progress
            GameManager.SharedGameManager.TheActionManager.Update(dt);

            // Construct an XNA time from a Cocos2d time
            this._totalGameTime += dt;
            this._gameTime.ElapsedGameTime = TimeSpan.FromSeconds(dt);
            this._gameTime.TotalGameTime = TimeSpan.FromSeconds(_totalGameTime);

            // Do a physics simulation step
            // IMPORTANT: Needs to come before InputManager.Update() as InputManager.Update's
            //            XxxControllers will depend upon a physics's space update happening
            //            before their XxxController.Update()'s are called.
            this._physicsManager.TheSpace.Update();
/*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PhysicsManager.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
*/

            // Let voxeliq services update
            this._inputManager.Update(this._gameTime);
/*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("InputManager.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
*/
            // TODO: Add back in when ready to tackle skydome again
            // this._skyDome.Update(this._gameTime);
            this._newSky.Update(this._gameTime);
            /*
#if STOPWATCH

            Program.TheStopwatch.Stop();
            Debug.WriteLine("NewSky.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
             */
            this._chunkCache.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ChunkCache.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */
            this._world.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("World.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */

            // TODO: Add back in when ready to tackle water again
            // this._waterCache.Update(this._gameTime);
            this._deferredShadowMapping.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("DeferredShadowMapping.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */
            this._userInterface.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("UserInterface.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */

            /* TODO: Add back in when ready to investigate these
            this._inGameDebugger.Update(this._gameTime);
            this._debugBar.Update(this._gameTime);
            this._graphManager.Update(this._gameTime);
            */

            // We depend on camera (positioning) and chunk cache (background)
            // to be ready before we start updating; otherwise we short-circuit
            if (!this._inputManager.TheLineRunnerControllerInput.Ready) /* ||
                !this._chunkCache.Ready)*/
            {
                return;
            }

            // Do a one-time prep of our first page
            if (!_firstTimeSetupPage)
            {
                // Let our intro layer display a pane now 
                //(3d model depends on services AND stationary camera being in place for display of panes)
                // this._parent.TheIntroLayer.LoadPane();
                this._parent.TheStartPage1Layer.LoadPane();

                // TODO: This starts too early, taking out until we can investigate further
                // And fade in the game scene
                // this._parent.TheIntroLayer.FadeIn();

                // It is now safe to lazy load our hud layer 
                // (depends upon stationary camera being in place for display of panes)
                this._hudLayer = _parent.TheHudLayer;

                // Now that our page components are ready, get our intro mechanics going (e.g., flyby)
                this.SwitchState(GameState.Intro);

                this._firstTimeSetupPage = true;

#if STOPWATCH
                Program.TheStopwatch.Stop();
                Debug.WriteLine("ActionLayer.FirstTimeSetupPage: " + Program.TheStopwatch.ElapsedMilliseconds);
                Program.TheStopwatch.Restart();
#endif
            }


            // Let game services update
            this._pageCache.Update(this._gameTime);
            this._lineCache.Update(this._gameTime);
            this._obstacleCache.Update(this._gameTime);
            this._characterCache.Update(this._gameTime);
            this._particleCache.Update(this._gameTime);
            this._sensorCache.Update(this._gameTime);
            this._paneCache.Update(this._gameTime);

            // Adjust hero physics so it stays in 2D plane
            if (this._characterCache.TheHeroModel != null)
            {
                this._characterCache.TheHeroModel.AdjustHeroPhysics();
            }

            // Position camera based on whether we are in a fly by or simply
            // tracking the hero position
            if (this._currentFlyBy != null)
            {
                this._currentFlyBy.UpdateBezier(dt);
            }
            else if (this._characterCache.TheHeroModel != null)
            {
                this.UpdateTrackingCamera();
            }
        }

        public override void Draw()
        {
            // We are dependent on this layer being ready (see Update())
            // AND the camera being ready before we start drawing
            if (!this.Ready ||                    
                !this._inputManager.TheLineRunnerControllerInput.Ready) /* ||
                !this._chunkCache.Ready)*/
            {
                return;
            }

            //
            // Note, unlike our short-cicuit for Update above when in refresh state,
            // we do those checks for drawing at the individual GameObject level
            // (e.g., see LineCache.Draw)
            //

            this._deferredShadowMapping.Draw(this._gameTime);

            this._particleCache.Draw(this._gameTime);

            /* TODO: Why is this not working here?
             * We currently have this in the DeferredShadowMapping drawing pipeline.
            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Matrix.Identity;
            LineDrawer.ViewDirection = this._trackingCamera.ViewDirection;
            LineDrawer.ProjectionMatrix = this._trackingCamera.ProjectionMatrix;
            ContactDrawer.Draw(LineDrawer, this._physicsManager.TheSpace);
            BoundingBoxDrawer.Draw(LineDrawer, this._physicsManager.TheSpace);
            */
            
            
            base.Draw();
        }

        #endregion
        
        #region Touch Implementation

        /*
        http://www.cocos2d-iphone.org/forums/topic/tutorials-dont-mention-cctouchdispatcherremovedelegate/
         
        Setting self.isTouchEnabled to YES in a CCLayer causes RegisterWithTouchDispatcher 
        to be called in onEnter, and CCDirector.SharedDirector.TouchDispatcher.RemoveDelegate(this)
        to be called in onExit.

        RegisterWithTouchDispatcher in CCLayer registers as a Standard touch delegate. 
        So you only need to override it if you want the Targeted touch messages.
            
        Note if you don't set CCTouchMode it will default to CCTouchMode.AllAtOnce, which means
        override TouchesBegan. Otherwise set CCTouchMode to CCTouchMode.OneByOne and override
        TouchBegan.
         
        In TouchBegan, If you return true then ccTouchEnded will called. 
        If you return false then ccTouchEnded will not be called, and the event 
        will go the parent layer
        */

        /// <summary>
        /// If you return true then ccTouchEnded will called. 
        /// If you return false then ccTouchEnded will not be called, and the event will go the parent layer
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        public override bool TouchBegan(CCTouch touch)
        {
            // Flow touch down to hero if we are in game play
            if (this._currentGameState == GameState.Moving)
            {
                _characterCache.TouchBegan(touch);
            }

            return true;
        }

        #endregion

        #region Helper Methods

        private void SwitchState(GameState gameState)
        {
            // Ok, now set our current state
            this._currentGameState = gameState;

            switch (this._currentGameState)
            {
                case GameState.World:
                        {
                             var blockStorage = (IBlockStorage)TheGame.SharedGame.Services.GetService(typeof(IBlockStorage));

                            var footPosition = this._inputManager.LineRunnerCamera.Position + 
                                new Vector3(20f, 0f, 0f);
                            var standingBlock = blockStorage.BlockAt(footPosition);
                            while (!standingBlock.Exists)
                            {
                                footPosition += new Vector3(0f, -1f, 0f);
                                standingBlock = blockStorage.BlockAt(footPosition);
                            }

                            this._inputManager.PlayerCamera.Position = footPosition;
                            this._inputManager.PlayerCamera.Target = Vector3.Forward;

                            break;
                        }
                case GameState.Intro:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;
                        GameManager.SharedGameManager.CurrentScore = GameManager.SharedGameManager.AdminStartScore;

                        // Update page/line number in hud
                        this._hudLayer.DisplayPageNumber(this._currentPageNumber);
                        this._hudLayer.DisplayLineNumber(this._currentLineNumber);

                        var pageWidth = this._pageCache.CurrentPageModel.WorldWidth;
                        var pageHeight = this._pageCache.CurrentPageModel.WorldHeight;
                        var pageDepthFromCamera = this._pageCache.PageDepthFromCameraStart;
                        var cameraStartingPoint = this._inputManager.LineRunnerCamera.Position;
                        var controlPoints = new List<Vector3>()
                        {
                            cameraStartingPoint, // Start to right and behing pad
                            cameraStartingPoint + new Vector3(0.25f * pageWidth, 0, 4.0f * pageDepthFromCamera), // By first control point in middle of pad in front
                            cameraStartingPoint + new Vector3(0.75f * pageWidth, 0, 4.0f * pageDepthFromCamera), // By second control point we are way out to top left of pad
                            cameraStartingPoint + new Vector3(1.1f * pageWidth, 0, 0)                                                           // Finish at camera position
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            this._inputManager.LineRunnerCamera.Target,          // Start looking back at terrain
                            this._inputManager.LineRunnerCamera.Target,          // By first control point we still looking straight back
                            this._inputManager.LineRunnerCamera.Target,          // Be second control point we are looking down at pad from upper left
                            this._inputManager.LineRunnerCamera.Target          // Finish by mimicing original camera target
                        };
                        var controlPointsReversed = new List<Vector3>()
                        {
                            cameraStartingPoint + new Vector3(1.1f * pageWidth, 0, 0),                                                           // Finish at camera position
                            cameraStartingPoint + new Vector3(0.75f * pageWidth, 0, 4.0f * pageDepthFromCamera), // By second control point we are way out to top left of pad
                            cameraStartingPoint + new Vector3(0.25f * pageWidth, 0, 4.0f * pageDepthFromCamera), // By first control point in middle of pad in front
                            cameraStartingPoint, // Start to right and behing pad
                        };
                        var targetPointsReversed = new List<Vector3>()
                        {
                            this._inputManager.LineRunnerCamera.Target,          // Start looking back at terrain
                            this._inputManager.LineRunnerCamera.Target,          // By first control point we still looking straight back
                            this._inputManager.LineRunnerCamera.Target,          // Be second control point we are looking down at pad from upper left
                            this._inputManager.LineRunnerCamera.Target          // Finish by mimicing original camera target
                        };

                        this._currentFlyBy = new FlyBy();
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;
                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_INTRO, controlPoints, targetPoints);

                        var nextFlyBy = new FlyBy();
                        nextFlyBy.FlyByFinished += OnCurrentFlyByFinished;
                        nextFlyBy.InitBezierControlPoints(GameConstants.DURATION_INTRO, controlPointsReversed, targetPointsReversed);
                        
                        this._currentFlyBy.NextFlyBy = nextFlyBy;
                        
                        /* Legacy: remove when comfortable
                        var controlPoints = new List<Vector3>()
                        {
                            cameraStartingPoint + new Vector3( 1.2f * pageWidth, -0.8f * pageHeight, -40), // Start to right and behing pad
                            cameraStartingPoint + new Vector3( 0.5f * pageWidth, -0.5f * pageHeight,  120), // By first control point in middle of pad in front
                            cameraStartingPoint + new Vector3(-0.2f * pageWidth,  0.2f * pageHeight,  120), // By second control point we are way out to top left of pad
                            cameraStartingPoint                                                            // Finish at camera position
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            controlPoints[0] + new Vector3(0,  0, -1),          // Start looking back at terrain
                            controlPoints[1] + new Vector3(0,  0, -1),          // By first control point we still looking straight back
                            controlPoints[2] + new Vector3(1, -1, -1),          // Be second control point we are looking down at pad from upper left
                            this._inputManager.LineRunnerCamera.Target          // Finish by mimicing original camera target
                        };
                        */

                        break;
                    }
                case GameState.Moving:
                    {
                        // Enable touches for hero
                        this.TouchMode = CCTouchMode.OneByOne;
                        this.TouchEnabled = true;

                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentLineNumber++;

                        // Update line number in hud
                        this._hudLayer.DisplayLineNumber(this._currentLineNumber);

                        // Create bezier based on current and next lines from _lineCache
                        this._currentFlyBy = new FlyBy(
                            cameraUpdate: FlyByCameraUpdate.TrackingAndStationaryHeightOnly,    // We will update our tracking camera and only the height of the stationary camera
                            targetAttachment: this._characterCache.TheHeroModel,                // We will have the hero model follow our target bezier
                            targetAttachmentUse: FlyByTargetAttachment.UsePhysicsBody);         // We will use the physics model for the hero when following the target bezier
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;             // Will switch state here in this event handler when finished
                        var heroPosition = this._characterCache.TheHeroModel.WorldOrigin;
                        var previousLineModel = this._lineCache.GetLineModel(this._currentLineNumber - 1);
                        var lineModel = this._lineCache.GetLineModel(this._currentLineNumber);
                        var lineSpacing = this._pageCache.CurrentPageModel.WorldLineSpacing;
                        var pageWidth = this._pageCache.CurrentPageModel.WorldWidth;
                        var pageDepth = this._pageCache.CurrentPageModel.WorldDepth;
                        var previousCenterLineWorldHeight = previousLineModel.WorldOrigin.Y +
                                                    (0.5f * lineSpacing);
                         var cameraStartPosition = new Vector3(
                                 heroPosition.X,
                                 previousCenterLineWorldHeight,
                                 heroPosition.Z + this._pageCache.CharacterDepthFromCameraStart);
                         var cameraStartTarget = heroPosition;

                        // We use this direction modifier to control our curve
                        // line number even => construct curve on right of pad
                        // line number odd => construct curve on left of pad
                        float direction = 1f;
                        if (this._currentLineNumber % 2 != 0)
                        {
                            direction = -direction;
                        }
                        
                        var controlPoints = new List<Vector3>()
                        {
                            cameraStartPosition,                                              // Start at our current camera position
                            cameraStartPosition + new Vector3(0.75f*pageWidth * direction, -0.75f * lineSpacing, -1.2f* this._pageCache.CharacterDepthFromCameraStart),  // By first control point move to right and down 1/4 page margin
                            cameraStartPosition + new Vector3(1.5f*pageWidth * direction, -1.0f * lineSpacing,  -1.2f*this._pageCache.CharacterDepthFromCameraStart),  // By second control point we still to right and down 3/4 page margin
                            new Vector3(                                                        // Finish at previous camera position x,z and set our height to be middle of next line
                                cameraStartPosition.X,                                                            
                                lineModel.WorldOrigin.Y + (0.5f * lineSpacing),
                                cameraStartPosition.Z)
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            cameraStartTarget,                                                // Start with current camera target
                            cameraStartTarget + new Vector3(0.5f * pageWidth * direction, -0.75f * lineSpacing, 0f),    // By first control point move to right and down 1/4 line spacing
                            cameraStartTarget + new Vector3(0.5f * pageWidth * direction, -1.0f * lineSpacing, 0f),    // By second control point we still to right and down 3/4 line spacing
                            new Vector3(                                                        // Finish at previous camera target x,z and set our height to be middle of next line
                                cameraStartTarget.X,                                                            
                                lineModel.WorldOrigin.Y + (0.5f * lineSpacing),
                                cameraStartTarget.Z)                        };

                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_MOVE_TO_NEXT_LINE, controlPoints, targetPoints);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Update page/line number in hud
                        this._hudLayer.DisplayPageNumber(this._currentPageNumber);
                        this._hudLayer.DisplayLineNumber(this._currentLineNumber);

                        // Create bezier based on current page and start position
                        this._currentFlyBy = new FlyBy(
                            cameraUpdate: FlyByCameraUpdate.TrackingAndStationaryHeightOnly,
                            targetAttachment: this._characterCache.TheHeroModel,
                            targetAttachmentUse: FlyByTargetAttachment.UsePhysicsBody);
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;     // Will switch state here in this event handler when finished
                        var pageModel = this._pageCache.CurrentPageModel;
                        var controlPoints = new List<Vector3>()
                        {
                            this._inputManager.LineRunnerCamera.Position,                                                                                // Start at our current camera position
                            this._inputManager.LineRunnerCamera.Position + new Vector3(-1.5f * pageModel.WorldWidth, 0.25f * pageModel.WorldHeight, 20f),  // By first control point move to left and up 1/4 page height
                            this._inputManager.LineRunnerCamera.Position + new Vector3(-1.5f * pageModel.WorldWidth, 0.75f * pageModel.WorldHeight, 20f),  // By second control point we still to left and up 3/4 page height
                            pageModel.PageStartOrigin + new Vector3(                                                           // Finish at world starting point adjusted back appropriately for camera
                                0,                                                            
                                0,
                                this._pageCache.PageDepthFromCameraStart)
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            this._inputManager.LineRunnerCamera.Target,                                                                                  // Start with current camera target
                            this._inputManager.LineRunnerCamera.Target + new Vector3(-1.5f * pageModel.WorldWidth, 0.25f * pageModel.WorldHeight, 20f),    // By first control point move to left and up 1/4 page height
                            this._inputManager.LineRunnerCamera.Target + new Vector3(-1.5f * pageModel.WorldWidth, 0.75f * pageModel.WorldHeight, 20f),    // By second control point we still to left and up 3/4 page height
                            pageModel.PageStartOrigin
                        };                                                                        // Finish with target looking at world starting point

                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_MOVE_TO_NEXT_PAGE, controlPoints, targetPoints);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // TODO: Not beting useed - keeping it simple now, will consider animation later.
                        // If so, see HudLayer.Draw for how to keep ui positioned correctly

                        /*
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;
                        GameManager.SharedGameManager.CurrentScore = GameManager.SharedGameManager.AdminStartScore;

                        // Update page/line number in hud
                        this._hudLayer.DisplayPageNumber(this._currentPageNumber);
                        this._hudLayer.DisplayLineNumber(this._currentLineNumber);

                        // IMPORTANT: Turn off ability to start game until the flyby created
                        // here is finished. In the flyby callback we will enable starting game
                        // again.
                        this._parent.TheStartPage1Layer.EnableStart(false);

                        // Create bezier based on current page and start position
                        this._currentFlyBy = new FlyBy(
                            cameraUpdate: FlyByCameraUpdate.TrackingAndStationaryHeightOnly,
                            targetAttachment: this._characterCache.TheHeroModel,
                            targetAttachmentUse: FlyByTargetAttachment.UsePhysicsBody);
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;     // Will switch state here in this event handler when finished
                        var pageModel = this._pageCache.CurrentPageModel;
                        var controlPoints = new List<Vector3>()
                        {
                            this._trackingCamera.Position,                                                                                // Start at our current camera position
                            this._trackingCamera.Position + new Vector3(-1.5f * pageModel.WorldWidth, 0.25f * pageModel.WorldHeight, 20f),  // By first control point move to left and up 1/4 page height
                            this._trackingCamera.Position + new Vector3(-1.5f * pageModel.WorldWidth, 0.75f * pageModel.WorldHeight, 20f),  // By second control point we still to left and up 3/4 page height
                            pageModel.PageStartOrigin + new Vector3(                                                           // Finish at world starting point adjusted back appropriately for camera
                                0,                                                            
                                0,
                                this._pageCache.PageDepthFromCameraStart)
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            this._trackingCamera.Target,                                                                                  // Start with current camera target
                            this._trackingCamera.Target + new Vector3(-1.5f * pageModel.WorldWidth, 0.25f * pageModel.WorldHeight, 20f),    // By first control point move to left and up 1/4 page height
                            this._trackingCamera.Target + new Vector3(-1.5f * pageModel.WorldWidth, 0.75f * pageModel.WorldHeight, 20f),    // By second control point we still to left and up 3/4 page height
                            pageModel.PageStartOrigin
                        };

                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_MOVE_TO_NEXT_PAGE, controlPoints, targetPoints);
                        */
                        break;
                    }
                case GameState.ResumingFromWorld:
                    {
                        // Currently a no-op
                        break;
                    }
                case GameState.Refresh:
                case GameState.Start:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;
                        GameManager.SharedGameManager.CurrentScore = GameManager.SharedGameManager.AdminStartScore;

                        // Update page/line number in hud
                        this._hudLayer.DisplayPageNumber(this._currentPageNumber);
                        this._hudLayer.DisplayLineNumber(this._currentLineNumber);

                        break;
                    }
            }

            this.SwitchServicesState(gameState);

            this._hudLayer.SwitchState(gameState);
        }

        // Helper function to centralize calling SwitchState on all appropriate game services
        private void SwitchServicesState(GameState gameState)
        {
            this._assetManager.SwitchState(gameState);
            this._world.SwitchState(gameState);
            this._chunkCache.SwitchState(gameState);
            this._inputManager.SwitchState(gameState);
            this._audioManager.SwitchState(gameState);
            this._pageCache.SwitchState(gameState);
            this._lineCache.SwitchState(gameState);
            // IMPORTANT: ObstacleCache depends upon LineCache hence SwitchState's are handled
            //            for ObstacleCache are handled in LineCache's SwitchState and possibly
            //            subsequent load functions
            // this._obstacleCache.SwitchState(gameState);
            // this._characterCache.SwitchState(gameState);
            // this._sensorCache.SwitchState(gameState);
            this._particleCache.SwitchState(gameState);
            this._deferredShadowMapping.SwitchState(gameState);
        }

        private void UpdateTrackingCamera()
        {
            switch (this._currentGameState)
            {
                case GameState.Moving:
                    {
                        // TODO:
                        // Slightly angle off of hero then come to opposite of angle as we reach end of line
                        var heroPosition = this._characterCache.TheHeroModel.WorldOrigin;
                        var lineModel = this._lineCache.GetLineModel(this._currentLineNumber);
                        var lineSpacing = this._pageCache.CurrentPageModel.WorldLineSpacing;
                        var centerLineWorldHeight = lineModel.WorldOrigin.Y +
                                                    (0.5f * lineSpacing);
                        var offset = this._characterCache.TheHeroModel.WorldWidth;
                        if (this._currentLineNumber % 2 == 0)
                        {
                            offset = -offset;
                        }
                        this._inputManager.LineRunnerCamera.Position =
                            new Vector3( // Position
                                heroPosition.X + offset,
                                centerLineWorldHeight,
                                heroPosition.Z + this._pageCache.CharacterDepthFromCameraStart);
                        this._inputManager.LineRunnerCamera.Target =
                            new Vector3(            // Target
                                heroPosition.X + (2f * offset),
                                centerLineWorldHeight,
                                heroPosition.Z
                                );

                        var cameraPositionLogical = XNAUtils.WorldToLogical(new Vector3(
                            heroPosition.X,
                            centerLineWorldHeight,
                            heroPosition.Z),
                            XNAUtils.CameraType.Stationary);

                        // Ok, now position the Cocos2D view so we follow the hero as well
                        CCDrawManager.ViewMatrix = Matrix.CreateLookAt(
                                new Vector3(cameraPositionLogical.X, cameraPositionLogical.Y, CCDirector.SharedDirector.ZEye),
                                new Vector3(cameraPositionLogical.X, cameraPositionLogical.Y, 0f),
                                Vector3.Up);

                        break;
                    }
                case GameState.Start:
                    {
                        // TODO:
                        // Position camera slightly off angle from hero
                        var heroPosition = this._characterCache.TheHeroModel.WorldOrigin;
                        // var heroPosition = this._pageCache.CurrentPageModel.HeroStartOrigin;
                        var lineModel = this._lineCache.GetLineModel(this._currentLineNumber);
                        var lineSpacing = this._pageCache.CurrentPageModel.WorldLineSpacing;
                        var centerLineWorldHeight = lineModel.WorldOrigin.Y +
                                                    (0.5f * lineSpacing);
                        var offset = this._characterCache.TheHeroModel.WorldWidth;
                        if (this._currentLineNumber%2 == 0)
                        {
                            offset = -offset;
                        }
                        this._inputManager.LineRunnerCamera.Position =
                            new Vector3( // Position
                                heroPosition.X + offset,
                                centerLineWorldHeight,
                                heroPosition.Z + this._pageCache.CharacterDepthFromCameraStart);
                        this._inputManager.LineRunnerCamera.Target =
                            new Vector3(            // Target
                                heroPosition.X + (2f * offset),
                                centerLineWorldHeight,
                                heroPosition.Z
                                );
                        break;
                    }
            }
        }

        // Callback used when line/obstacle hit to determine
        // where to navigate to next
        public void HandleMoveToFinish()
        {
            // Did we get a new high score?
            var navigateToLayer = LayerTags.StartPage1Layer;
            var scoreRepository = new FacebookScoreRepository();
            var previousTopScore = scoreRepository.GetTopScoresForPlayer(1);
            if (GameManager.SharedGameManager.CurrentScore > 0  // We have a score to check
                &&                                              // AND
                (previousTopScore.Count == 0 ||                 // This is our first top score or we beat our previous top score
                previousTopScore[0].Score < GameManager.SharedGameManager.CurrentScore))
            {
                // Record new high score and give them a chance to post
                this._parent.Navigate(LayerTags.FinishLayer);
            }
            else
            {
                // Just let them start again
                this._parent.AdjustNavigationStackToStart();
                this._parent.GoBack();
            }
        }


        #endregion
    }
}