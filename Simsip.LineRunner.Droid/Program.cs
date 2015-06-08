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
using Android.Gms.Ads;
// TODO: Kamcord
// using Com.Kamcord.Android;
using Simsip.LineRunner.Views;
using Android.Runtime;
using System.Threading.Tasks;
using System.Diagnostics;
using Simsip.LineRunner.Services.Inapp;

namespace Simsip.LineRunner
{

    [Activity(Label = "Line Runner"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        // , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        // , ScreenOrientation = ScreenOrientation.SensorPortrait had to remove and now done programatically as there is a generated typo introduced
        // into the AndroidManaifest.xml
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class Program : AndroidGameActivity, IReviewAppPlaystoreCallback
    {

#if STOPWATCH
        public static Stopwatch TheStopwatch = new Stopwatch();
#endif

        public static Program SharedProgram { get; private set; }

        public AdView Ad { get; private set; }

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

                base.OnCreate(bundle);

                RequestedOrientation = ScreenOrientation.Portrait;

                AndroidPlaystoreAudit.Instance.OnReviewAppPlaystore = this;

                // Create the add banner.
                Ad = new AdView(this);
                Ad.AdUnitId = "ca-app-pub-1449829469918284/9470451252";
                Ad.AdSize = AdSize.Banner;
                Ad.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                Ad.Id = 1;

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

                // Create an ad request.
                AdRequest adRequest = new AdRequest.Builder()
                    .AddTestDevice("03f38988437cd73a")  // LGE Nexus 5
                    .Build();

                // Start loading the ad in the background.
                Ad.LoadAd(adRequest);

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

                Program.SharedProgram = this;

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

        protected override void OnDestroy()
        {
            // Destroy the AdView.
            Ad.Destroy();

            // If we are attached to the Google Play Service then disconnect
            var inappService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            if (inappService != null)
            {
                inappService.OnDestroy();
            }

            base.OnDestroy();
        }

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

        public void LaunchOAuth()
        {
            var intent = new Intent(this, typeof(OAuthView));
            StartActivity(intent);
        }
    }
}

