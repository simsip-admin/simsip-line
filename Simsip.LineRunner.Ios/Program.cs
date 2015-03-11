using System;
using System.Diagnostics;
using UIKit;

using GoogleAdMobAds;

using MTiRate;
using Simsip.LineRunner.Views;

namespace Simsip.LineRunner
{
    [Foundation.Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        UIWindow window;
        GADBannerView bannerView;
        
        // TODO: New
        // UIViewController vc;
        private UINavigationController nav;
        private UIViewController vc;
        private OAuthViewController oauth;

        public static Program SharedProgram { get; private set; }

        private TheGame game;

        private bool _allowRating;

        public override void FinishedLaunching(UIApplication app)
        {
            /*
            iRate.SharedInstance.ShouldPromptForRating += (sender) =>
            {
                return _allowRating;
            };

            // TODO: New
            // vc = new UIViewController();
            nav = new UINavigationController();
            vc = new UIViewController();

            window = new UIWindow(UIScreen.MainScreen.Bounds);

            // TODO: New
            // window.RootViewController = vc;
            window.RootViewController = nav;
            nav.PushViewController(vc, false);

            window.MakeKeyAndVisible();

            bannerView = new GADBannerView(GADAdSizeCons.Banner, new PointF(0, vc.View.Frame.Height - GADAdSizeCons.Banner.Size.Height));
            // TODO: Get actual unit id
            bannerView.AdUnitID = "MY_BANNER_UNIT_ID";
            bannerView.RootViewController = vc;

            // TODO: See https://components.xamarin.com/view/googleadmob to stabilize this
            vc.View.AddSubview(bannerView);
            bannerView.LoadRequest(GADRequest.Request);
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

        public void LaunchOAuth()
        {
            oauth = new OAuthViewController();
            nav.PushViewController(oauth, false);
        }
    }

}

