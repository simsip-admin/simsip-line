using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AuditApp.Android;
using AuditApp.Common;
// TODO: Moving to Xamarin component
// using Com.Google.Android.Gms.Ads;
using Microsoft.Xna.Framework;
using System;
using Android.Content;
#if CHINA
using Com.Mobvista.Cloud.Sdk;
#else
using Android.Gms.Ads;
#endif

// TODO: Kamcord
// using Com.Kamcord.Android;
using Simsip.LineRunner.Views;
using Android.Runtime;
using System.Threading.Tasks;
using System.Diagnostics;
using Simsip.LineRunner.Services.Inapp;

namespace Simsip.LineRunner
{

    [Activity(Label = "Linerunner 3D"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        // , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        // IMPORTANT: Had to change this from - Android.Content.PM.LaunchMode.SingleInstance
        , LaunchMode = Android.Content.PM.LaunchMode.SingleTop
        // , ScreenOrientation = ScreenOrientation.SensorPortrait had to remove and now done programatically as there is a generated typo introduced
        // into the AndroidManaifest.xml
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class Program : AndroidGameActivity, IReviewAppPlaystoreCallback
    {

#if STOPWATCH
        public static Stopwatch TheStopwatch = new Stopwatch();
#endif

        public static Program SharedProgram { get; private set; }

#if CHINA
        private MobvistaAd _mobvistaAd;
        public View Ad { get; private set; }
#else
        public AdView Ad { get; private set; }
#endif


        private bool _allowRating;

        public const string HOCKEYAPP_APPID = "ddf54d39d923783f48d51918bf6b615d";

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
#if STOPWATCH
                System.Diagnostics.Debug.WriteLine("Program.OnCreate: 0");
                Program.TheStopwatch.Start();
#endif
                Program.SharedProgram = this;

                this.TheInAppService = new InappService();
                this.TheInAppService.Initialize();

                base.OnCreate(bundle);

                RequestedOrientation = ScreenOrientation.Portrait;

                AndroidPlaystoreAudit.Instance.OnReviewAppPlaystore = this;

                // Create the add banner.
#if CHINA
                //Initialization SDK
                this._mobvistaAd = new MobvistaAd(this, "21448", "331b27b08dab30a7ac76f9f9ff757d99");
                this.Ad = this._mobvistaAd.GetBannerAdView(this, null);
                this.Ad.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                this.Ad.Id = 1;
#else
                this.Ad = new AdView(this);
                this.Ad.AdUnitId = "ca-app-pub-1449829469918284/9470451252";
                this.Ad.AdSize = AdSize.Banner;
                this.Ad.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                this.Ad.Id = 1;
#endif

                // Create the game
                // TODO: New monogame
                // TheGame.Activity = this;
                var game = new TheGame();
                // TODO: New monogame
                // game.Window.Id = 2;
                var androidGameWindow = game.Window as AndroidGameWindow;
                androidGameWindow.GameView.Id = 2;

                // Creating a new RelativeLayout to stitch the ad and game together
                // Way it is done here is the only one that has worked so far
                RelativeLayout relativeLayout = new RelativeLayout(this);
                RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(
                    RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                lp.AddRule(LayoutRules.Below, Ad.Id);
                relativeLayout.AddView(Ad);
                // TODO: New monogame
                // relativeLayout.AddView(game.Window, lp);
                relativeLayout.AddView(androidGameWindow.GameView, lp);
                this.SetContentView(relativeLayout);

#if !CHINA
                // Create an ad request
                AdRequest adRequest = new AdRequest.Builder()
                    .AddTestDevice("03f38988437cd73a")  // LGE Nexus 5
                    .Build();

                // Start loading the ad in the background.
                this.Ad.LoadAd(adRequest);
#endif

                // Register the crash manager before Initializing the trace writer
                HockeyApp.CrashManager.Register(this, HOCKEYAPP_APPID);

                //Register to with the Update Manager
                HockeyApp.UpdateManager.Register(this, HOCKEYAPP_APPID);

                // Initialize the Trace Writer
                HockeyApp.TraceWriter.Initialize();

                // Wire up Unhandled Expcetion handler from Android
                AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
                {
                    // Use the trace writer to log exceptions so HockeyApp finds them
                    HockeyApp.TraceWriter.WriteTrace(args.Exception);
                    args.Handled = true;
                };

                // Wire up the .NET Unhandled Exception handler
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.ExceptionObject);

                // Wire up the unobserved task exception handler
                TaskScheduler.UnobservedTaskException +=
                    (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.Exception);

                // TODO: What is this?
                // PlatformManager.Startup(game);

                /* TODO: Kamcord
                 Kamcord.InitKeyAndSecret(
                     "NHNvCMLPBXYwbkrS0VxZEv5xtknDc6SURI3QcmQp6m7",
                     "kn5KjNHJkGUbGlQyaTcX68Xi8fGGM2rDHGuzawPiyuA",
                     "LineRunner");
                 Kamcord.InitActivity(this);
                */

                game.Run(GameRunBehavior.Asynchronous);
            }
            catch(Exception ex)
            {
                // TODO: Log
                string temp = "remove";
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Ask the open service connection's billing handler to process this request
            var inappService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            if (inappService != null)
            {
                inappService.HandleActivityResult(requestCode, resultCode, data);
            }
        }

        public void BuyProduct(object state)
        {
            var productId = state as string;
            var inappService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            inappService.PurchaseProduct(productId);
        }

        protected override void OnDestroy()
        {
#if CHINA
            this._mobvistaAd.OnDestory();
#else
            // Destroy the AdView.
            this.Ad.Destroy();
#endif

            // If we are attached to the Google Play Service then disconnect
#if !CHINA
            var inappService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            if (inappService != null)
            {
                inappService.OnDestroy();
            }
#endif

            base.OnDestroy();
        }

#if CHINA
        protected override void OnResume() 
        {
            base.OnResume();
            this._mobvistaAd.OnResume();
        }
    
        protected override void OnPause() 
        {
            base.OnPause();
            this._mobvistaAd.OnPause();
        }

        protected override void OnStart() 
        {
            base.OnStart();
            this._mobvistaAd.OnStart();
        }
    
        protected override void OnStop() 
        {
            base.OnStop();
            this._mobvistaAd.OnStop();
        }
#endif

        // Summary:
        //     Return 'True' if you want to proceed to the app store without interuption
        //     such as you showing another prompt.  Return 'False' if you want to stop the
        //     process and show the user a prompt before proceeding NOTE: You can manually
        //     call the static method OpenAppStore(...) after showing user your own prompt.
        //      true to avoid interuption with flow; otherwise, false.
        //
        // Returns:
        //     true to avoid interuption with flow; otherwise, false.
        // See:
        //     https://components.xamarin.com/gettingstarted/auditapp
        public bool OnReviewAppPlaystoreResponse()
        {
            return _allowRating;
        }

        // See: https://components.xamarin.com/gettingstarted/auditapp
        public void RateApp()
        {
            _allowRating = true;
            AndroidPlaystoreAudit.Instance.ShowPrompt(this);
            _allowRating = false;
        }

        public string BrowserUrl { get; private set; }

        public void LaunchBrowser(string browserUrl)
        {
            this.BrowserUrl = browserUrl;

            var intent = new Intent(this, typeof(BrowserView));
            StartActivity(intent);
        }

        public void LaunchOAuth()
        {
            var intent = new Intent(this, typeof(OAuthView));
            StartActivity(intent);
        }

        public IInappService TheInAppService { get; private set; }

    }
}

