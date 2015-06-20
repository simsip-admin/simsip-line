using System;
using CoreGraphics;
using UIKit;
using Foundation;
using Simsip.LineRunner.ViewModels;

namespace Simsip.LineRunner.Views
{
    [Foundation.Register("BrowserView")]
    public class BrowserView : UIView
    {
        public BrowserView()
        {
            Initialize();
        }

        public BrowserView(CGRect bounds)
            : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
        }
    }

    [Foundation.Register("BrowserViewController")]
    public class BrowserViewController : UIViewController
    {
        public BrowserViewController()
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
            View = new BrowserView();

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
            NSUrl url = (NSUrl)new Uri(Program.SharedProgram.BrowserUrl);
            webView.LoadRequest(new NSUrlRequest(url));
        }
    }
}