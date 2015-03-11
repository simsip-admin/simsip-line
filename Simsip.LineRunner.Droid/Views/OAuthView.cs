using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Simsip.LineRunner.ViewModels;

namespace Simsip.LineRunner.Views
{
    [Activity(Label = "Login", 
              MainLauncher = false,
              Theme = "@android:style/Theme.NoTitleBar")]
    public class OAuthView : Activity
    {
        private OAuthViewModel _vm;

        private WebView _webView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _vm = new OAuthViewModel();

            SetContentView(Resource.Layout.View_OAuth);

            _webView = FindViewById<WebView>(Resource.Id.webview);
            _webView.SetWebViewClient(new OAuthViewClient(_vm));
            _webView.Settings.JavaScriptEnabled = true;
            _webView.LoadUrl(_vm.AuthorizationUri.ToString());
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && _webView.CanGoBack())
            {
                _webView.GoBack();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public class OAuthViewClient : WebViewClient
        {
            private readonly OAuthViewModel _vm;

            public OAuthViewClient(OAuthViewModel vm)
            {
                _vm = vm;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                _vm.NavigatingCommand(new Uri(url));

                view.LoadUrl(url);

                return true;
            }
        }

    }

}