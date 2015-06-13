using System;
using CoreGraphics;
using UIKit;
using Foundation;
using Simsip.LineRunner.ViewModels;

namespace Simsip.LineRunner.Views
{
    [Foundation.Register("LicenseView")]
    public class LicenseView : UIView
    {
        public LicenseView()
        {
            Initialize();
        }

        public LicenseView(CGRect bounds)
            : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
        }
    }

    [Foundation.Register("LicenseViewController")]
    public class LicenseViewController : UIViewController
    {
        public LicenseViewController()
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            View = new LicenseView();

            base.ViewDidLoad();

            // Perform any additional setup after loading the view
            var webView = new UIWebView((CGRect)View.Bounds);
            View.AddSubview(webView);

            webView.ScalesPageToFit = true;

            webView.ShouldStartLoad += (view, request, type) =>
                {
                    return true;
                };

            // TODO: New
            /*
            string url = "http://xamarin.com";
            webView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
            */
            NSUrl url = (NSUrl)new Uri("http://linerunner3d.com/license");
            webView.LoadRequest(new NSUrlRequest(url));
        }
    }
}