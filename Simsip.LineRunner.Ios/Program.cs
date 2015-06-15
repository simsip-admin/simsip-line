using System;
using System.Diagnostics;
using UIKit;

using MTiRate;
using Simsip.LineRunner.Views;
using Simsip.LineRunner.Services.Inapp;
using CoreGraphics;

namespace Simsip.LineRunner
{
    [Foundation.Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        
        // TODO: New
        // UIViewController vc;
        private UINavigationController nav;
        private UIViewController vc;
        private OAuthViewController oauth;
        private LicenseViewController license;

        public static Program SharedProgram { get; private set; }

        private TheGame game;

        private bool _allowRating;

        public override void FinishedLaunching(UIApplication app)
        {
            /* TODO: Support for rating see below
            iRate.SharedInstance.ShouldPromptForRating += (sender) =>
            {
                return _allowRating;
            };

            // TODO: Support for OAuth see below
            nav = new UINavigationController();
            vc = new UIViewController();

            window = new UIWindow(UIScreen.MainScreen.Bounds);
            window.RootViewController = nav;
            nav.PushViewController(vc, false);

            window.MakeKeyAndVisible();
            */

            try
            {
                // Fun begins..
                game = new TheGame();
                game.Run();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in game run: " + ex);
            }
        }

        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }

        // See: https://components.xamarin.com/gettingstarted/irate
        public void RateApp()
        {
            _allowRating = true;
            iRate.SharedInstance.PromptIfNetworkAvailable();
            _allowRating = false;
        }

        public void LaunchLicense()
        {
            license = new LicenseViewController();
            nav.PushViewController(license, false);
        }

        public void LaunchOAuth()
        {
            oauth = new OAuthViewController();
            nav.PushViewController(oauth, false);
        }

        public override void WillTerminate(UIApplication application)
        {
            var inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            inAppService.WillTerminate();

            base.WillTerminate(application);
        }
    }

}

