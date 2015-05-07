using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using Engine.Universe;
using System.Threading;
using Simsip.LineRunner.Settings.Readers;
using Engine.Core.Config;
using Engine.Graphics.Effects.PostProcessing.Bloom;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.GameObjects.Sensors;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.GameObjects;
#if IOS
using AVFoundation;
using Foundation;
#endif
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Start
{
    public class StartPage1Layer : GameLayer
    {
        private CoreScene _parent;

        // Pane
        private PaneModel _paneModel;
        private CCPoint _paneLogicalOrigin;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;
        
        // Layer actions
        private CCAction _layerActionIn;
        private CCFiniteTimeAction _layerActionOut;
        private CCPoint _layerStartPosition;
        private CCPoint _layerEndPosition;

        // Menu
        private CCMenuItemImage _startButton;
        private CCMenu _startMenu;

        // Flag so we only kick off services once
        private bool _servicesStarted;

        // Allows for controling holding off from starting game from finish screen
        // when navigating quicker than move to start flyby
        private bool _isStartEnabled;
        private bool _isLoadingShowing;

        public StartPage1Layer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.96f * screenSize.Width,
                0.4f * screenSize.Height);

            // Just determine pane possition, will be loaded via api LoadPane()
            this._paneLogicalOrigin = new CCPoint(
                0.02f * screenSize.Width,
                0.05f * screenSize.Height);

            // Pane transition in/out
            this._layerStartPosition = new CCPoint(
                 this._paneLogicalOrigin.X,
                -this.ContentSize.Height);
            this._layerEndPosition = this._paneLogicalOrigin;

            // Layer transition in/out
            var layerStartPlacementAction = new CCPlace(this._layerStartPosition);
            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, this._layerEndPosition);
            this._layerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction }));
            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, this._layerStartPosition);
            this._layerActionOut = layerMoveOutAction;

            // Logo
            var logo = new CCSprite("Images/Misc/Logo1.png");
            logo.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.7f * this.ContentSize.Height);
            this.AddChild(logo);

            // Start menu
            this._isStartEnabled = false;
            var startButtonNormal = new CCSprite("Images/Icons/StartButtonNormal.png");
            var startButtonSelected = new CCSprite("Images/Icons/StartButtonSelected.png");
            this._startButton = new CCMenuItemImage( (obj) => { this.NavigateStartPage2(); });
            this._startButton.NormalImage = startButtonNormal;
            this._startButton.SelectedImage = startButtonSelected;

            var ratingsButtonNormal = new CCSprite("Images/Icons/RatingsButtonNormal.png");
            var ratingsButtonSelected = new CCSprite("Images/Icons/RatingsButtonSelected.png");
            var ratingsButton = new CCMenuItemImage((obj) => { this.Rate(); });
            ratingsButton.NormalImage = ratingsButtonNormal;
            ratingsButton.SelectedImage = ratingsButtonSelected;

            var optionsButtonNormal = new CCSprite("Images/Icons/OptionsButtonNormal.png");
            var optionsButtonSelected = new CCSprite("Images/Icons/OptionsButtonSelected.png");
            var optionsButton = new CCMenuItemImage((obj) => { this.NavigateBase(LayerTags.OptionsMasterLayer); });
            optionsButton.NormalImage = optionsButtonNormal;
            optionsButton.SelectedImage = optionsButtonSelected;

            var helpButtonNormal = new CCSprite("Images/Icons/HelpButtonNormal.png");
            var helpButtonSelected = new CCSprite("Images/Icons/HelpButtonSelected.png");
            var helpButton = new CCMenuItemImage((obj) => { this.NavigateBase(LayerTags.HelpMasterLayer); });
            helpButton.NormalImage = helpButtonNormal;
            helpButton.SelectedImage = helpButtonSelected;
            
            this._startMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._startButton,
                        ratingsButton,
                        optionsButton,
                        helpButton
                    });
            this._startMenu.AlignItemsHorizontallyWithPadding(
                0.1f * this.ContentSize.Width);
            this._startMenu.AnchorPoint = CCPoint.AnchorMiddle;
            this._startMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(this._startMenu);

#if DEBUG
            var adminText = string.Empty;
#if ANDROID
            adminText = Program.SharedProgram.Resources.GetString(Resource.String.StartAdmin);
#elif IOS
            adminText = NSBundle.MainBundle.LocalizedString(Strings.StartAdmin, Strings.StartAdmin);
#else
            adminText = AppResources.StartAdmin;
#endif
            var adminLabel = new CCLabelTTF(adminText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var adminItem = new CCMenuItemLabel(adminLabel,
                (obj) => { this._parent.Navigate(LayerTags.AdminLayer); });
            var adminLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        adminItem
                    });
            adminLabelMenu.Position = new CCPoint(
                 0.5f * this.ContentSize.Width, 
                -0.1f * this.ContentSize.Height );
            this.AddChild(adminLabelMenu);
#endif

            // We'll use our override of Update to intelligently kick-off
            // initialization
            this.ScheduleUpdate();
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate pane/layer
            if (this._paneModel != null)
            {
                this._paneModel.ModelRunAction(this._paneActionIn);
            }
            this.RunAction(this._layerActionIn);

            // If we meet the ratings prompt criteria, display ratings prompt
            CheckForRatingsPrompt();
        }

        public override void Update(float dt)
        {
            // Don't kick off our second stage of initialization until
            // core app initialization is complete (e.g., database copied into place,
            // user settings file in place, etc.)
            if (!TheGame.SharedGame.Ready)
            {
                return;
            }

            // Below is our ONE-TIME services start code
            if (this._servicesStarted)
            {
                return;
            }

            // Get all services created/initialized in background
#if NETFX_CORE
            IAsyncAction asyncAction = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        InitializeServicesDoWork();
                    });
#else
            ThreadPool.QueueUserWorkItem(InitializeServicesDoWork);
#endif
            this._servicesStarted = true;
        }

        public override void Draw()
        {
            // Draw pane with Cocos2D view, projection and game state
            if (this._paneModel != null)
            {
                this._paneModel.DrawViaStationaryCamera();
            }

            base.Draw();
        }

        #endregion

        #region Api

        /// <summary>
        /// Allows for outside control of enabling start menu.
        /// 
        /// A "loading" message box will display if user taps start menu and
        /// we are not ready to start.
        /// Examples:
        /// Initial start of game
        /// Hero was killed and is moving to start
        /// </summary>
        /// <param name="enableStart">True if tapping on start menu should start game immediately. False
        /// if tapping on start menu should show the loading indicator.</param>
        public void EnableStart(bool enableStart)
        {
            this._isStartEnabled = enableStart;
        }

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Initialize ||
                args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                this._isStartEnabled = true;

                // Remove any previous loading message box and proceed to
                // StartPage2 (tap to start)
                if (this._isLoadingShowing)
                {
                    this._isLoadingShowing = false;
                    this._parent.TheMessageBoxLayer.Hide();
                    this.NavigateStartPage2();
                }
            }
        }


        // Needed as services to load model will not be available
        // when constructor called
        public void LoadPane()
        {
            var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            var paneStartPosition = XNAUtils.LogicalToWorld(
                this._layerStartPosition,
                pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var paneEndPosition = XNAUtils.LogicalToWorld(
                this._layerEndPosition,
                pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var paneStartPlacementAction = new Place(paneStartPosition);
            var paneMoveInAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneEndPosition);
            this._paneActionIn = new EaseBackOut(
                new Sequence(new FiniteTimeAction[] { paneStartPlacementAction, paneMoveInAction }));
            var paneMoveOutAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneStartPosition);
            this._paneActionOut = new EaseBackIn(paneMoveOutAction);

            // Pane model
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = this._paneLogicalOrigin,
                LogicalWidth = this.ContentSize.Width,
                LogicalHeight = this.ContentSize.Height,
            };
            this._paneModel = new PaneModel(paneModelArgs);
        }

        #endregion

        #region Helper methods

        private void Rate()
        {
#if NETFX_CORE
            App.RateApp();
#elif DESKTOP
            Program.RateApp();
#else
           Program.SharedProgram.RateApp();
#endif
        }

        private void NavigateStartPage2()
        {
            /* Staging for getting first set of binaries for starter worlds
            var world = (IWorld)TheGame.SharedGame.Services.GetService(typeof(IWorld));
            world.Save();
            return;
            */

            if (this._isStartEnabled)
            {
                this.NavigateBase(LayerTags.StartPage2Layer);
            }
            else
            {
                this._isLoadingShowing = true;
                this._parent.TheMessageBoxLayer.Show(
                    "loading",
                    string.Empty,
                    MessageBoxType.MB_PROGRESS);
            }
        }

        private void NavigateBase(LayerTags layer)
        {
            // Setup navigation to occur after we animate layer out
            var navigateAction = new CCCallFunc(() => { this._parent.Navigate(layer); });
            var layerMoveOutAction = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { this._layerActionOut, navigateAction } )
                );

            // Ok, now run pane/layer actions out
            this._paneModel.ModelRunAction(this._paneActionOut);
            this.RunAction(layerMoveOutAction);
        }

        private void CheckForRatingsPrompt()
        {
            var installDate = UserDefaults.SharedUserDefault.GetDateForKey(GameConstants.USER_DEFAULT_KEY_INSTALL_DATE, DateTime.Now);
            var gamesPlayedCount = UserDefaults.SharedUserDefault.GetIntegerForKey(GameConstants.USER_DEFAULT_KEY_GAMES_PLAYED_COUNT, 1);
            var askForRating = UserDefaults.SharedUserDefault.GetBoolForKey(GameConstants.USER_DEFAULT_KEY_ASK_FOR_RATING, true);

            var installedDuration = DateTime.UtcNow - installDate;
            if (installedDuration.TotalDays > GameConstants.RATING_WINDOW_IN_DAYS &&
                gamesPlayedCount > 9 &&
                askForRating)
            {
                this._parent.Navigate(LayerTags.RatingsLayer);
            }

        }

#if NETFX_CORE
        private async void InitializeServicesDoWork()
#else
        private void InitializeServicesDoWork(object state)
#endif
        {
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("InitializeServicesDoWork: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            // Read settings.
            var audioSettings = new Simsip.LineRunner.Settings.Readers.AudioSettings();
            var graphicsSettings = new GraphicsSettings();

            // Create a new engine configuration.
            var config = new EngineConfig
            {
                Chunk =
                {
                    WidthInBlocks = 16,
                    HeightInBlocks = 128,
                    LengthInBlocks = 16,
                },
                Cache =
                {
                    CacheExtraChunks = true,
                    ViewRange = 4,
                    CacheRange = 6,
                },
                Graphics =
                {
                    Width = TheGame.SharedGame.GraphicsDevice.PresentationParameters.BackBufferWidth, // graphicsSettings.Width,
                    Height = TheGame.SharedGame.GraphicsDevice.PresentationParameters.BackBufferHeight, // graphicsSettings.Height,
                    FullScreenEnabled = true, // graphicsSettings.FullScreenEnabled,
                    VerticalSyncEnabled = graphicsSettings.VerticalSyncEnabled,
                    FixedTimeStepsEnabled = graphicsSettings.FixedTimeStepsEnabled,
                },
                World =
                {
                    IsInfinitive = true,
                },
                Debugging =
                {
                    GraphsEnabled = true,
                },
                Bloom =
                {
                    Enabled = false,
                    State = BloomState.Saturated,
                },
                Audio =
                {
                    Enabled = audioSettings.Enabled,
                }
            };

            // Construct all voxeliq services
            var engine = new Engine.Core.Engine(TheGame.SharedGame, config);
            engine.Run();

#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("Engine.Run: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

            // Construct all simsip services
            var physicsManager = new PhysicsManager(TheGame.SharedGame);
            physicsManager.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PhysicsManager: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var pageCache = new PageCache(TheGame.SharedGame);
            pageCache.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PageCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var lineCache = new LineCache(TheGame.SharedGame);
            lineCache.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("LineCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var obstacleCache = new ObstacleCache(TheGame.SharedGame);
            obstacleCache.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ObstacleCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var characterCache = new CharacterCache(TheGame.SharedGame);
            characterCache.Enabled = false;
            characterCache.LoadContentAsyncFinished += LoadContentAsyncFinishedHandler;

#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("CharacterCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var particleCache = new ParticleEffectCache(TheGame.SharedGame);
            particleCache.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ParticleCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var sensorCache = new SensorCache(TheGame.SharedGame);
            sensorCache.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("SensorCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var paneCache = new PaneCache(TheGame.SharedGame);
            paneCache.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PaneCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            var deferredShadowMapping = new DeferredShadowMapping(TheGame.SharedGame);
            deferredShadowMapping.Enabled = false;
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("DeferredShadowMapping: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

            // Initialize all voxeliq services, with updates to status messages as needed
            // IMPORTANT: InputManager Needs to come first as SpawnPlayer() flow depends on this being in place first.
            //            See World.Initialize() for start of SpawnPlayer() flow
            engine.InputManagerComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("InputManager: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

#if NETFX_CORE
            await engine.AssetManagerComponent.Initialize();
#else
            engine.AssetManagerComponent.Initialize();
#endif

#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("AssetManager: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.SkyDomeComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("SkyDome: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.NewSkyComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("NewSky: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.FoggerComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("Fogger: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.ChunkStorageComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ChunkStorage: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.VertexBuilderComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("VertexBuilder: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.ChunkCacheComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ChunkCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.BlockStorageComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("BlockStorage: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.WorldComponent.Initialize();         // Needs to come before player component?
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("World: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.WaterCacheComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("WaterCache: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.UserInterfaceComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("UserInterface: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.InGameDebuggerComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("InGameDebugger: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.DebugBarComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("DebugBar: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.GraphManagerComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("GraphManager: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            engine.AudioManagerComponent.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("AudioManager: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

            // Initialize all simsip services
            physicsManager.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PhysicsManager.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            pageCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PageCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            lineCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("LineCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            obstacleCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ObstacleCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            characterCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("CharacterCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            particleCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ParticleCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            sensorCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("SensorCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            paneCache.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PaneCache.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            deferredShadowMapping.Initialize();
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("DeferredShadowMapping.Initialize: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

            GameManager.SharedGameManager.ServicesReadyForUpdate = true;
        }

        #endregion
    }
}