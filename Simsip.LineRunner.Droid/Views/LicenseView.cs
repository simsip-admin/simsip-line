using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Simsip.LineRunner.ViewModels;

namespace Simsip.LineRunner.Views
{
    [Activity(Label = "License", 
              MainLauncher = false,
              Theme = "@android:style/Theme.NoTitleBar")]
    public class LicenseView : Activity
    {
        private WebView _webView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_OAuth);

            _webView = FindViewById<WebView>(Resource.Id.webview);
            _webView.SetWebViewClient(new LicenseViewClient());
            _webView.Settings.JavaScriptEnabled = true;
            _webView.LoadUrl("http://linerunner3d.com/license");
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

        public class LicenseViewClient : WebViewClient
        {
            public LicenseViewClient()
            {
            }

            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.LoadUrl(url);

                return true;
            }
        }

    }

}