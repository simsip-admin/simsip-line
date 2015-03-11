using System.Windows.Navigation;
using Simsip.LineRunner.ViewModels;
using Microsoft.Phone.Controls;

namespace Simsip.LineRunner.Views
{
    public partial class OAuthView : PhoneApplicationPage
    {
        private OAuthViewModel _vm;
        private string _redirectUrl;

        public OAuthView()
        {
            InitializeComponent();

            // OAuthBrowser.LoadCompleted += Browser_DoResize;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _vm = new OAuthViewModel();
            _redirectUrl = _vm.RedirectUri.ToString();

            OAuthBrowser.Navigate(_vm.AuthorizationUri);
        }

        private void OAuthBrowser_OnNavigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(_redirectUrl))
            {
                _vm.NavigatingCommand(e.Uri);    
            }
        }

        private void Browser_DoResize(object sender, NavigationEventArgs e)
        {
            string html = OAuthBrowser.SaveToString();
            string hackstring = "<meta name=\"viewport\" content=\"width=100%,user-scalable=yes\" />";
            html = html.Insert(html.IndexOf("<head>", 0) + 6, hackstring);
            
            OAuthBrowser.NavigateToString(html);
            
            OAuthBrowser.LoadCompleted -= Browser_DoResize;
        }
    }
}