using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
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
using Microsoft.Xna.Framework.Graphics;
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
    public class StartPage1Layer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCFiniteTimeAction _layerActionOut;

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

            // Layer transition in/out
             var layerEndPosition = new CCPoint(
                0.02f * screenSize.Width,
                0.05f * screenSize.Height);
            var  layerStartPosition = new CCPoint(
                 layerEndPosition.X,
                -this.ContentSize.Height);
            var layerStartPlacementAction = new CCPlace(layerStartPosition);
            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
            this._layerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction }));
            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
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
            var startButton = new CCMenuItemImage( (obj) => { this.NavigateStartPage2(); });
            startButton.NormalImage = startButtonNormal;
            startButton.SelectedImage = startButtonSelected;

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
            
            var mainImageMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        startButton,
                        ratingsButton,
                        optionsButton,
                        helpButton
                    });
            mainImageMenu.AlignItemsHorizontallyWithPadding(
                0.1f * this.ContentSize.Width);
            mainImageMenu.AnchorPoint = CCPoint.AnchorMiddle;
            mainImageMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(mainImageMenu);

            var playText = string.Empty;
#if ANDROID
            playText = Program.SharedProgram.Resources.GetString(Resource.String.MainPlay);
#elif IOS
            playText = NSBundle.MainBundle.LocalizedString(Strings.MainPlay, Strings.MainPlay);
#else
            playText = AppResources.MainPlay;
#endif
            var playLabel = new CCLabelTTF(playText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var playItem = new CCMenuItemLabel(playLabel,
                (obj) => { this.NavigateStartPage2(); });
            
            var rateText = string.Empty;
#if ANDROID
            rateText = Program.SharedProgram.Resources.GetString(Resource.String.MainRate);
#elif IOS
            rateText = NSBundle.MainBundle.LocalizedString(Strings.MainRate, Strings.MainRate);
#else
            rateText = AppResources.MainRate;
#endif
            var rateLabel = new CCLabelTTF(rateText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var rateItem = new CCMenuItemLabel(rateLabel,
                (obj) => { this.Rate(); });

            var optionsText = string.Empty;
#if ANDROID
            optionsText = Program.SharedProgram.Resources.GetString(Resource.String.MainOptions);
#elif IOS
            optionsText = NSBundle.MainBundle.LocalizedString(Strings.MainOptions, Strings.MainOptions);
#else
            optionsText = AppResources.MainOptions;
#endif
            var optionsLabel = new CCLabelTTF(optionsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var optionsItem = new CCMenuItemLabel(optionsLabel,
                (obj) => { this.NavigateBase(LayerTags.OptionsMasterLayer); });

            var helpText = string.Empty;
#if ANDROID
            helpText = Program.SharedProgram.Resources.GetString(Resource.String.MainHelp);
#elif IOS
            helpText = NSBundle.MainBundle.LocalizedString(Strings.MainHelp, Strings.MainHelp);
#else
            helpText = AppResources.MainHelp;
#endif
            var helpLabel = new CCLabelTTF(helpText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var helpItem = new CCMenuItemLabel(helpLabel,
                (obj) => { this.NavigateBase(LayerTags.HelpMasterLayer); });
            
            var mainLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        playItem,
                        rateItem,
                        optionsItem,
                        helpItem
                    });
            mainLabelMenu.Position = new CCPoint(
                 0.5f * this.ContentSize.Width,
                 0.1f * this.ContentSize.Height);
            mainLabelMenu.AlignItemsHorizontallyWithPadding(
                0.1f * this.ContentSize.Width);
            this.AddChild(mainLabelMenu);

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

            // Animate layer
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

        #endregion

        #region Api

        /// <summary>
        /// Allows for outside control of enabling start menu.
        /// 
        /// A "loading" message box will display if user taps start menu and
        /// we are not ready to start.
        /// Examples:
        /// - Initial start of game
        /// - Hero was killed and is moving to start
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