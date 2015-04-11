using Cocos2D;
using Engine.Core.Config;
using Engine.Graphics.Effects.PostProcessing.Bloom;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.GameObjects.Sensors;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Settings.Readers;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Utils;
using System.ComponentModel;
#if IOS
using AVFoundation;
using Foundation;
#endif
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Intro
{
    public class IntroLayer : CCLayerColor, IIntroLayer
    {
        private CoreScene _parent;
        private CCSize _screenSize;
        private PaneModel _paneModel;
        private CCSprite _logo;
        private CCSprite _progressHightlightSprite;
        private CCLabelTTF _messageLabel;

        // Used to control the background initialization of services
        private int _updateCounter;

        private CCTintToWithAlpha _fadeInAction;

        public IntroLayer(CoreScene parent)
            : base(new CCColor3B(Microsoft.Xna.Framework.Color.Black))
        {
            this._parent = parent;

            this._screenSize = CCDirector.SharedDirector.WinSize;

            // Display logo in center
            // IMPORTANT: Width:Height of logo png is 3:1 so we want to keep that ratio when scaling
            _logo = new CCSprite("Images/Misc/Logo1.png");
            Cocos2DUtils.ResizeSprite(this._logo,
                0.9f * this._screenSize.Width,
                0.3f * this._screenSize.Width);
            _logo.Position = new CCPoint(this._screenSize.Width / 2, this._screenSize.Height / 2);
            this.AddChild(_logo);

            // Below the logo,
            // display a darkened progress bar background that will be overlayed by the progress bar highlight
            // IMPORTANT: Width:Height of progress png is 20:1 so we want to keep that ratio when scaling
            var progressBackground = new CCSprite("Images/Misc/ProgressBackground.png");
            Cocos2DUtils.ResizeSprite(progressBackground,
                0.8f  * this._screenSize.Width,
                0.04f * this._screenSize.Width);
            progressBackground.Position = new CCPoint(this._screenSize.Width / 2, this._screenSize.Height / 3);
            this.AddChild(progressBackground);

            // Setup our progress bar highlight
            // IMPORTANT: Width:Height of progress png is 20:1 so we want to keep that ratio when scaling
            this._progressHightlightSprite = new CCSprite("Images/Misc/ProgressHighlight.png");
            Cocos2DUtils.ResizeSprite(this._progressHightlightSprite,
                0.8f  * this._screenSize.Width,
                0.04f * this._screenSize.Width);
            var progressHighlight = new CCProgressTimer(_progressHightlightSprite);
            progressHighlight.Type = CCProgressTimerType.Bar;
            progressHighlight.Midpoint = new CCPoint(0.0f, 0.5f);  // Starts from left
            progressHighlight.BarChangeRate = new CCPoint(1.0f, 0.0f); // Grow only in the "x" horizontal direction
            progressHighlight.Position = new CCPoint(this._screenSize.Width / 2, this._screenSize.Height / 3); // Overlap the progress background added above
            AddChild(progressHighlight);

            // Add in placeholder to display various loading messages from other modules
            _messageLabel = new CCLabelTTF(Message, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            _messageLabel.Position = new CCPoint(this._screenSize.Width / 2, this._screenSize.Height / 4);
            _messageLabel.Color = new CCColor3B(255, 255, 255);
            this.AddChild(_messageLabel);        

            // Animate our progress bar highlight over the progress bar background
            var progressAnimation = new CCRepeatForever(new CCProgressFromTo(4, 0, 100));
            progressHighlight.RunAction(progressAnimation);

            // So we don't have an abrupt display from all black to game scene, we use this
            // animation exposed by an api below
            this._fadeInAction = new CCTintToWithAlpha(GameConstants.DURATION_INTRO_FADE_IN, 0, 0, 0, this);
            this._fadeInAction.Opacity = 0;

            // Export service
            TheGame.SharedGame.Services.AddService(typeof(IIntroLayer), this);

            // We'll need this to update our ui message on
            this.ScheduleUpdate();
        }

        #region Properties

        private string _message = string.Empty;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }

        #endregion

        #region Cocos2D overrides

        public override void Update(float dt)
        {
            base.Update(dt);

            // Don't kick off our second stage of initialization until
            // core app initialization is complete (e.g., database copied into place,
            // user settings file in place, etc.)
            if (!TheGame.SharedGame.Ready)
            {
                return;
            }

            _messageLabel.Text = _message;

            // We use this logic here so that the intro screen can be displayed
            // as quickly as possible, then start the additional loading needed
            _updateCounter++;
            if (_updateCounter == 10)
            {
                // Get all services created/initialized in background
                // IMPORTANT: Intro screen will be replaced with start screen once
                // we flag TheGame.SharedGame.ServicesReadyForUpdate
                #if NETFX_CORE
                // InitializeServicesDoWork();
            IAsyncAction asyncAction = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        InitializeServicesDoWork();
                    });
#elif DESKTOP
                InitializeServicesDoWork(null, null);
                GameManager.SharedGameManager.ServicesReadyForUpdate = true;
#else

                var initializeServicesWorker = new BackgroundWorker();
                initializeServicesWorker.DoWork += InitializeServicesDoWork;
                initializeServicesWorker.RunWorkerCompleted += InitializeServicesCompleted;
                initializeServicesWorker.RunWorkerAsync();
#endif
            }
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

        // Needed as services to load model will not be available
        // when constructor called
        public void LoadPane()
        {
            // Pane model
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = new CCPoint(10, 100),
                LogicalWidth = 300,
                LogicalHeight = 200,
            };
            this._paneModel = new PaneModel(paneModelArgs);
        }

        public void FadeIn()
        {
            this.RunAction(this._fadeInAction);
        }

        #endregion

        #region Helper methods

#if NETFX_CORE
        private async void InitializeServicesDoWork()
#else
        private void InitializeServicesDoWork(object sender, DoWorkEventArgs e)
#endif
        {
            // Update loading status message
            var introLayerService = (IIntroLayer)TheGame.SharedGame.Services.GetService(typeof(IIntroLayer));
            var loadingModelsText = string.Empty;
#if ANDROID
            loadingModelsText = Program.SharedProgram.Resources.GetString(Resource.String.IntroLoadingModels);
#elif IOS
            loadingModelsText = NSBundle.MainBundle.LocalizedString(Strings.IntroLoadingModels, Strings.IntroLoadingModels);
#else
            loadingModelsText = AppResources.IntroLoadingModels;
#endif
            introLayerService.Message = loadingModelsText;

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

            // Construct all simsip services
            var physicsManager = new PhysicsManager(TheGame.SharedGame);
            physicsManager.Enabled = false;
            var pageCache = new PageCache(TheGame.SharedGame);
            pageCache.Enabled = false;
            var lineCache = new LineCache(TheGame.SharedGame);
            lineCache.Enabled = false;
            var obstacleCache = new ObstacleCache(TheGame.SharedGame);
            obstacleCache.Enabled = false;
            var characterCache = new CharacterCache(TheGame.SharedGame);
            characterCache.Enabled = false;
            var particleCache = new ParticleEffectCache(TheGame.SharedGame);
            particleCache.Enabled = false;
            var sensorCache = new SensorCache(TheGame.SharedGame);
            sensorCache.Enabled = false;
            var paneCache = new PaneCache(TheGame.SharedGame);
            paneCache.Enabled = false;
            var deferredShadowMapping = new DeferredShadowMapping(TheGame.SharedGame);
            deferredShadowMapping.Enabled = false;

            // Initialize all voxeliq services, with updates to status messages as needed
            // IMPORTANT: InputManager Needs to come first as SpawnPlayer() flow depends on this being in place first.
            //            See World.Initialize() for start of SpawnPlayer() flow
            engine.InputManagerComponent.Initialize();  
#if NETFX_CORE
            await engine.AssetManagerComponent.Initialize();
#else
            engine.AssetManagerComponent.Initialize();
#endif
            var generatingTerrainText = string.Empty;
#if ANDROID
            generatingTerrainText = Program.SharedProgram.Resources.GetString(Resource.String.IntroGeneratingTerrain);
#elif IOS
            generatingTerrainText = NSBundle.MainBundle.LocalizedString(Strings.IntroGeneratingTerrain, Strings.IntroGeneratingTerrain);
#else
            generatingTerrainText = AppResources.IntroGeneratingTerrain;
#endif
            introLayerService.Message = generatingTerrainText;
            engine.SkyDomeComponent.Initialize();
            engine.NewSkyComponent.Initialize();
            engine.FoggerComponent.Initialize();
            engine.ChunkStorageComponent.Initialize();
            engine.VertexBuilderComponent.Initialize();
            engine.ChunkCacheComponent.Initialize();
            engine.BlockStorageComponent.Initialize();
            engine.WorldComponent.Initialize();         // Needs to come before player component?
            engine.WaterCacheComponent.Initialize();
            engine.UserInterfaceComponent.Initialize();
            engine.InGameDebuggerComponent.Initialize();
            engine.DebugBarComponent.Initialize();
            engine.GraphManagerComponent.Initialize();
            engine.AudioManagerComponent.Initialize();

            // Initialize all simsip services
            physicsManager.Initialize();
            pageCache.Initialize();
            lineCache.Initialize();
            obstacleCache.Initialize();
            characterCache.Initialize();
            particleCache.Initialize();
            sensorCache.Initialize();
            paneCache.Initialize();
            deferredShadowMapping.Initialize();

#if NETFX_CORE
            GameManager.SharedGameManager.ServicesReadyForUpdate = true;
#endif
        }

#if !NETFX_CORE
        private void InitializeServicesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Let all other game objects (e.g., layers) know that they can start to use services
            GameManager.SharedGameManager.ServicesReadyForUpdate = true;
        }
#endif

        #endregion
    }
}