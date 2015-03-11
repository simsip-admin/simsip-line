using System;
using CoreGraphics;
using UIKit;
using Foundation;
using Simsip.LineRunner.ViewModels;

namespace Simsip.LineRunner.Views
{
    [Foundation.Register("OAuthView")]
    public class OAuthView : UIView
    {
        public OAuthView()
        {
            Initialize();
        }

        public OAuthView(CGRect bounds)
            : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
        }
    }

    [Foundation.Register("OAuthViewController")]
    public class OAuthViewController : UIViewController
    {
        private OAuthViewModel _vm;
        public OAuthViewController()
        {
            _vm = new OAuthViewModel();
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            View = new OAuthView();

            base.ViewDidLoad();

            // Perform any additional setup after loading the view
            var webView = new UIWebView((CGRect)View.Bounds);
            View.AddSubview(webView);

            webView.ScalesPageToFit = true;

            webView.ShouldStartLoad += (view, request, type) =>
                {
                    _vm.NavigatingCommand((Uri)request.Url);
                    return true;
                };

            // TODO: New
            /*
            string url = "http://xamarin.com";
            webView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
            */
            NSUrl url = (NSUrl)_vm.AuthorizationUri;
            webView.LoadRequest(new NSUrlRequest(url));
        }
    }
}