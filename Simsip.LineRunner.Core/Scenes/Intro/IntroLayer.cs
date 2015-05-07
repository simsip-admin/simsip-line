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
using System.Diagnostics;
using System.Threading;
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
            _logo.Position = new CCPoint(this._screenSize.Width / 2, this._screenSize.Height / 2);
            this.AddChild(_logo);

            // Below the logo,
            // display a darkened progress bar background that will be overlayed by the progress bar highlight
            // IMPORTANT: Width:Height of progress png is 20:1 so we want to keep that ratio when scaling
            var progressBackground = new CCSprite("Images/Misc/ProgressBackground.png");
            progressBackground.Position = new CCPoint(this._screenSize.Width / 2, this._screenSize.Height / 3);
            this.AddChild(progressBackground);

            // Setup our progress bar highlight
            // IMPORTANT: Width:Height of progress png is 20:1 so we want to keep that ratio when scaling
            this._progressHightlightSprite = new CCSprite("Images/Misc/ProgressHighlight.png");
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
#else

                ThreadPool.QueueUserWorkItem(InitializeServicesDoWork);
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
            var screenSize = CCDirector.SharedDirector.WinSize;
            var paneLogicalOrigin = new CCPoint(
                0.02f * screenSize.Width,
                0.20f * screenSize.Height);
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = paneLogicalOrigin,
                LogicalWidth = 0.96f * screenSize.Width,
                LogicalHeight = 0.5f * screenSize.Height,
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
        private void InitializeServicesDoWork(object state)
#endif
        {
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("InitializeServicesDoWork: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
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