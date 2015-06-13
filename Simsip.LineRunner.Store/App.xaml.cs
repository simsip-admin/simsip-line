﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Simsip.LineRunner.Views;


// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Simsip.LineRunner
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var gamePage = Window.Current.Content as GamePage;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (gamePage == null)
            {
                // Create a main GamePage
                gamePage = new GamePage(args);

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the GamePage in the current Window
                Window.Current.Content = gamePage;
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity

            deferral.Complete();
        }

        // References:
        // http://stackoverflow.com/questions/14293036/launching-the-windows-store-listing-from-win-rt-windows-8-app
        // http://www.sharpgis.net/post/2012/09/11/Linking-to-Your-Windows-Store-App
        public static async void RateApp()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=PACKAGENAME"));
        }

        public void LaunchLicense()
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(LicenseView));
        }

        public void LaunchOAuth()
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(OAuthView));
        }
    }
}
