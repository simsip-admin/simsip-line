using System.Windows.Navigation;
using Simsip.LineRunner.ViewModels;
using Microsoft.Phone.Controls;
using System;

namespace Simsip.LineRunner.Views
{
    public partial class BrowserView : PhoneApplicationPage
    {
        public BrowserView()
        {
            InitializeComponent();

            // OAuthBrowser.LoadCompleted += Browser_DoResize;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TheBrowser.Navigate(new Uri(Program.SharedProgram.BrowserUrl));
        }

        private void Browser_DoResize(object sender, NavigationEventArgs e)
        {
            string html = TheBrowser.SaveToString();
            string hackstring = "<meta name=\"viewport\" content=\"width=100%,user-scalable=yes\" />";
            html = html.Insert(html.IndexOf("<head>", 0) + 6, hackstring);
            
            TheBrowser.NavigateToString(html);
            
            TheBrowser.LoadCompleted -= Browser_DoResize;
        }
    }
}