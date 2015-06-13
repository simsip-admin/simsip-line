using System.Windows.Navigation;
using Simsip.LineRunner.ViewModels;
using Microsoft.Phone.Controls;
using System;

namespace Simsip.LineRunner.Views
{
    public partial class LicenseView : PhoneApplicationPage
    {
        public LicenseView()
        {
            InitializeComponent();

            // OAuthBrowser.LoadCompleted += Browser_DoResize;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LicenseBrowser.Navigate(new Uri("http://linerunner3d.com/license"));
        }

        private void Browser_DoResize(object sender, NavigationEventArgs e)
        {
            string html = LicenseBrowser.SaveToString();
            string hackstring = "<meta name=\"viewport\" content=\"width=100%,user-scalable=yes\" />";
            html = html.Insert(html.IndexOf("<head>", 0) + 6, hackstring);
            
            LicenseBrowser.NavigateToString(html);
            
            LicenseBrowser.LoadCompleted -= Browser_DoResize;
        }
    }
}