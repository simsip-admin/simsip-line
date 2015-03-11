using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Simsip.LineRunner
{
    public class Program
    {
        public static Program SharedProgram { get; private set; }

        public Program()
        {
            Program.SharedProgram = this;
        }

        public void RateApp()
        {
            var marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        public void LaunchOAuth()
        {
            (App.Current.RootVisual as PhoneApplicationFrame).
                Navigate(new Uri("/Views/OAuthView.xaml", UriKind.Relative));
        }


    }
}
