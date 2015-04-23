using Cocos2D;
using CocosDenshion;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Scenes;


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
            CCSpriteFontCache.RegisterFont("arial", 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 38, 50, 64);
            CCSpriteFontCache.RegisterFont("Schwarzwald Regular", 26);

            CCDrawManager.SetDesignResolutionSize(DESIGN_WIDTH, 
                                                  DESIGN_HEIGHT, 
                                                  CCResolutionPolicy.NoBorder);

            // Get initial loading screen displayed
            var scene = new CoreScene();
            pDirector.RunWithScene(scene);

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

            // On Android, entering the background and returning leads to 
            // all sorts of issues, hence we force stop for this platform.
            // Reference:
            // http://www.pencelgames.com/blog/porting-android-monogame-textures-and-spritesheets
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
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