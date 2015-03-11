using Cocos2D;
using CocosDenshion;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Scenes;


namespace Simsip.LineRunner
{
    public class ContentDirectory
    {
        public CCSize MinFrameSize;
        public string Directory;
    }

    public class AppDelegate : CCApplication
    {

        /* We are not using these yet, just keeping in here when we are ready to expand
        // Note: Default content directory is for resources targeting 480x320 devices
        ContentDirectory _0960x0640ResourceDirectory;   // e.g., iphone-hd
        ContentDirectory _1024x0768ResourceDirectory;   // e.g., iPad
        ContentDirectory _2048x1536ResourceDirectory;   // e.g., ipad-hd
        */

        private int _preferredWidth = 320;
        private int _preferredHeight = 480;

        public AppDelegate(Game game, GraphicsDeviceManager graphics)
            : base(game, graphics)
        {
            s_pSharedApplication = this;

            CCDrawManager.InitializeDisplay(game,
                                            graphics,
                                            DisplayOrientation.Portrait);


#if WINDOWS_PHONE8
            HandleMediaStateAutomatically = false; // Bug in MonoGame - https://github.com/Cocos2DXNA/cocos2d-xna/issues/325
#endif
            game.Window.AllowUserResizing = true;
            graphics.PreferMultiSampling = false;

            graphics.PreferredBackBufferWidth = this._preferredWidth;
            graphics.PreferredBackBufferHeight = this._preferredHeight;
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
            CCSpriteFontCache.FontScale = 0.6f;
            CCSpriteFontCache.FontRoot = "Fonts";
            CCSpriteFontCache.RegisterFont("arial", 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 38, 50, 64);
            CCSpriteFontCache.RegisterFont("Schwarzwald Regular", 26);

            var designSize = new CCSize(
                this._preferredWidth, 
                this._preferredHeight);

            /* TODO: Is this needed?
            float testHeight = CCDrawManager.Fr
            var adHeight = Program.SharedProgram.Ad.Height;
            var newFrameSize = CCDrawManager.FrameSize;
            newFrameSize.Height -= adHeight;
            CCDrawManager.FrameSize = newFrameSize;
            */

            // TODO: Should we be more exact in the content scale factor here?
            if (CCDrawManager.FrameSize.Height > 480)
            {
                CCSize resourceSize = new CCSize(640, 960);
                CCContentManager.SharedContentManager.SearchPaths.Add("Hd");
                pDirector.ContentScaleFactor = resourceSize.Height / designSize.Height;
            }

            CCDrawManager.SetDesignResolutionSize(designSize.Width, 
                                                  designSize.Height, 
                                                  CCResolutionPolicy.NoBorder /*CCResolutionPolicy.ShowAll*/);

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