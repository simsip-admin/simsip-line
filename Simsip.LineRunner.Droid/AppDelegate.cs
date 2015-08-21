using Cocos2D;
using CocosDenshion;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Scenes;
using Simsip.LineRunner.Services.Inapp;
using System.Diagnostics;


namespace Simsip.LineRunner
{
    public class AppDelegate : CCApplication
    {
        private const int DESIGN_WIDTH = 640;
        private const int DESIGN_HEIGHT = 960;

        public AppDelegate(Game game, GraphicsDeviceManager graphics)
            : base(game, graphics)
        {
            s_pSharedApplication = this;

            CCDrawManager.InitializeDisplay(game,
                                            graphics,
                                            DisplayOrientation.Portrait);

            graphics.PreferMultiSampling = false;
        }

        /// <summary>
        /// Implement for initialize OpenGL instance, set source path, etc...
        /// </summary>
        public override bool InitInstance()
        {
            return base.InitInstance();
        }

        /// <summary>
        ///  Implement CCDirector and CCScene init code here.
        /// </summary>
        /// <returns>
        ///  true  Initialize success, app continue.
        ///  false Initialize failed, app terminate.
        /// </returns>
        public override bool ApplicationDidFinishLaunching()
        {
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ApplicationDidFinishLaunching: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

            // Initialize director
            CCDirector pDirector = CCDirector.SharedDirector;
            pDirector.SetOpenGlView();

            // TODO: How does taking this out affect us?
            // 2D projection
            // pDirector.ProjectionMatrix = CCDirectorProjection.Projection2D;

            // Turn on display FPS
            // pDirector.DisplayStats = true;

            // Set FPS. the default value is 1.0/60 if you don't call this
            pDirector.AnimationInterval = 1.0 / 60;

            // TODO: Which fonts do we want to pre-register?
            CCSpriteFontCache.FontRoot = "Fonts";
            CCSpriteFontCache.RegisterFont("arial-core", 18, 24, 32, 50);

            CCDrawManager.SetDesignResolutionSize(DESIGN_WIDTH, 
                                                  DESIGN_HEIGHT, 
                                                  CCResolutionPolicy.NoBorder);

            // Load inapp service on main thread
            // Note: Created in Program.OnCreate
            TheGame.SharedGame.Services.AddService(typeof(IInappService), Program.SharedProgram.TheInAppService);

            // Get initial loading screen displayed
            var scene = new CoreScene();
            pDirector.RunWithScene(scene);

#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ApplicationDidFinishLaunching.End: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif

            return true;
        }

        /// <summary>
        /// This function will be called when the application enters the background.
        /// </summary>
        public override void ApplicationDidEnterBackground()
        {
            // Stop all of the animation actions that are running.
            CCDirector.SharedDirector.Pause();

            // If you use SimpleAudioEngine, your music must be paused
            CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic();

            // IMPORTANT: Previously we had to kill the app when entering background - see commented out code below.
            //            Current code-set appears to be stable though.
            // On Android, entering the background and returning leads to 
            // all sorts of issues, hence we force stop for this platform.
            // Reference:
            // http://www.pencelgames.com/blog/porting-android-monogame-textures-and-spritesheets
            // Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        /// <summary>
        /// This function will be called when the application enters the foreground.
        /// </summary>
        public override void ApplicationWillEnterForeground()
        {
            CCDirector.SharedDirector.Resume();

            // If you use SimpleAudioEngine, your background music track must resume here. 
            CCSimpleAudioEngine.SharedEngine.ResumeBackgroundMusic();
        }
    }
}