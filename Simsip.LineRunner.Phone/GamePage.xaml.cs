using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using MonoGame.Framework.WindowsPhone;
using GoogleAds;

namespace Simsip.LineRunner
{
    public partial class GamePage : PhoneApplicationPage
    {
        private TheGame _game;
        private Program _program;

        // Constructor
        public GamePage()
        {
            InitializeComponent();

            this._program = new Program();

            this._game = XamlGame<TheGame>.Create("", this);

            //
            // For advertisting on WP, we will need to dynamically add in a row into the grid after the call to:
            // this._game = XamlGame<TheGame>.Create("", this); In the GamePage.cs constructor, see source code
            // For XamlGame.cs at c:\dev3\cocos2d-xna\MonoGame\MonoGame.Framework\WindowsPhone
            //
            AdView bannerAd = new AdView
            {
                Format = AdFormats.Banner,
                AdUnitID = "ca-app-pub-1449829469918284/9470451252"
            };
            bannerAd.ReceivedAd += OnAdReceived;
            bannerAd.FailedToReceiveAd += OnFailedToReceiveAd;
            // LayoutRoot.Children.Add(bannerAd);
            Container.Children.Insert(0, bannerAd);
            AdRequest adRequest = new AdRequest();
            adRequest.ForceTesting = true;
            bannerAd.LoadAd(adRequest);
        }

        private void OnAdReceived(object sender, AdEventArgs e)
        {
            Debug.WriteLine("Received ad successfully");
        }

        private void OnFailedToReceiveAd(object sender, AdErrorEventArgs errorCode)
        {
            Debug.WriteLine("Failed to receive ad with error " + errorCode.ErrorCode);
        }
    }
}