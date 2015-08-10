using System.Diagnostics;
using Cocos2D;
using CocosDenshion;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Scenes;
using UIKit;
using GoogleAdMobAds;
using CoreGraphics;
using Foundation;
using Simsip.LineRunner.Services.Inapp;


namespace Simsip.LineRunner
{
    public class AppDelegate : CCApplication
    {
        // IOS Admob support
        const string AdmobID = "ca-app-pub-1449829469918284/9470451252";
        GADBannerView _adView;

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
            // Construct our ad banner
            var mainWindow = (UIWindow)TheGame.SharedGame.Services.GetService(typeof(UIWindow));
            var iosGameViewController = (UIViewController)TheGame.SharedGame.Services.GetService(typeof(UIViewController));
            
#if !ADVERTISING_OFF
            // Define ADVERTISING_OFF for screenshots
            this._adView = new GADBannerView(
                size: GADAdSizeCons.Banner,
                origin: new CGPoint(0, 0))
            {
                AdUnitID = AdmobID,
                RootViewController = iosGameViewController
            };
#endif
            /*
            var originalFrame = iosGameViewController.View.Frame;

            iosGameViewController.View.Frame = new CoreGraphics.CGRect(
                0,
                originalFrame.Y + this._adView.AdSize.Size.Height,
                originalFrame.Width,
                originalFrame.Height - this._adView.AdSize.Size.Height);
            */

#if !ADVERTISING_OFF
            // Define ADVERTISING_OFF for screenshots
            // Inject our add banner
            iosGameViewController.View.AddSubview(this._adView);

            this._adView.LoadRequest(GADRequest.Request);
#endif

            /* Add in test devices for add banner
            // See: https://developers.google.com/mobile-ads-sdk/docs/admob/ios/targeting
            GADRequest adRequest = new GADRequest();
            adRequest.TestDevices = 
            */

            // TODO: Leaving below in until we have ads/game view stabilized

            /* TODO: Should we use this, doesn't seem to work on simulator?
            adView.AdReceived += (sender, args) =>
            {
                if (!viewOnScreen)
                {
                    iosGameViewController.View.AddSubview(adView);
                }
                viewOnScreen = true;
            };
            */

            /*
            var controller = new UIViewController();
            this._adView = new GADBannerView(
                size: GADAdSizeCons.Banner,
                origin: new CGPoint(0, 0))
            {
                AdUnitID = AdmobID,
                RootViewController = controller
            };
            controller.View.AddSubview(this._adView);


            // https://developer.apple.com/library/ios/featuredarticles/ViewControllerPGforiPhoneOS/CreatingCustomContainerViewControllers/CreatingCustomContainerViewControllers.html
            iosGameViewController.WillMoveToParentViewController(null);
            iosGameViewController.View.RemoveFromSuperview();
            iosGameViewController.RemoveFromParentViewController();
            
            
            controller.AddChildViewController(iosGameViewController);

            var originalFrame = iosGameViewController.View.Frame;

            iosGameViewController.View.Frame = new CoreGraphics.CGRect(
                0,
                originalFrame.Y + this._adView.AdSize.Size.Height,
                originalFrame.Width,
                originalFrame.Height - this._adView.AdSize.Size.Height);

            controller.View.AddSubview(iosGameViewController.View);

            iosGameViewController.DidMoveToParentViewController(controller);

            
            mainWindow.RootViewController = controller;
            // mainWindow.Add(controller.View);
            */

            // Initialize director
            CCDirector pDirector = CCDirector.SharedDirector;
            pDirector.SetOpenGlView();

            // TODO: How does taking this out affect us?
            // 2D projection
            // pDirector.Projection = CCDirectorProjection.Projection2D;

            // Turn on display FPS
            // pDirector.DisplayStats = true;

            // Set FPS. the default value is 1.0/60 if you don't call this
            pDirector.AnimationInterval = 1.0 / 60;

            // TODO: Which fonts do we want to pre-register?
            CCSpriteFontCache.FontRoot = "Fonts";
            CCSpriteFontCache.RegisterFont("arial-unicode", 18, 24, 32, 50);
            
            CCDrawManager.SetDesignResolutionSize(DESIGN_WIDTH,
                                                  DESIGN_HEIGHT,
                                                  CCResolutionPolicy.NoBorder);

            // Load inapp service on main thread
            var inAppService = new InappService();
            inAppService.Initialize();

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