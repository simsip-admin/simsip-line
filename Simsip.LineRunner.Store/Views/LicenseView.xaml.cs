﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml.Printing;
using Simsip.LineRunner.ViewModels;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Simsip.LineRunner.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LicenseView : Page
    {
        public LicenseView()
        {
            this.InitializeComponent();

            this.Loaded += LicenseView_Loaded;
        }

        private void LicenseView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri targetUri = new Uri("http://linerunner3d.com/license");
                webView1.Navigate(targetUri);
            }
            catch (Exception Error)
            {
                var exception = "Exception: " + Error;
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
            }

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        /*
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        */

    }
}