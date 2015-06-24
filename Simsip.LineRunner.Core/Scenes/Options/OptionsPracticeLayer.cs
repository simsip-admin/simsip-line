using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.GameObjects;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Obstacles;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Options
{
    public class OptionsPracticeLayer : GameLayer
    {
        private CoreScene _parent;
        private OptionsMasterLayer _masterLayer;

        // Pages
        private int _pageCount;
        private IList<CCMenuItemToggle> _pageToggles;
        private IList<CCLabelTTF> _pageLabels;

        // Lines
        private int _lineCount;
        private IList<CCMenuItemToggle> _lineToggles;
        private IList<CCLabelTTF> _lineLabels;

        // Kills
        private string _killsOn;
        private string _killsOff;
        private CCLabelTTF _killsLabel;

        // Additional text
        private string _switchingPageText;
        private string _switchingLineText;

        // Services we'll need
        IObstacleCache _obstacleCache;

        public OptionsPracticeLayer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Grab reference to services we'll need
            this._obstacleCache = (IObstacleCache)TheGame.SharedGame.Services.GetService(typeof(IObstacleCache));

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Practice title
            var practiceText = string.Empty;
#if ANDROID
            practiceText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPractice);
#elif IOS
            practiceText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPractice, Strings.OptionsPractice);
#else
            practiceText = AppResources.OptionsPractice;
#endif
            var practiceLabel = new CCLabelTTF(practiceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            practiceLabel.AnchorPoint = CCPoint.AnchorMiddle;
            practiceLabel.Color = CCColor3B.Blue;
            practiceLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(practiceLabel);

            // Start page subtitle
            var startPageText = string.Empty;
#if ANDROID
            startPageText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeStartPage);
#elif IOS
            startPageText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeStartPage, Strings.OptionsPracticeStartPage);
#else
            startPageText = AppResources.OptionsPracticeStartPage;
#endif
            var startPageLabel = new CCLabelTTF(startPageText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            startPageLabel.AnchorPoint = CCPoint.AnchorMiddle;
            startPageLabel.Rotation = -90;
            startPageLabel.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
                0.5f  * this.ContentSize.Height);
            this.AddChild(startPageLabel);

            // Pages
            this._pageCount = 6;
            this._pageToggles = new List<CCMenuItemToggle>();
            this._pageLabels = new List<CCLabelTTF>();
            var pageMenuMargin = (0.7f * this.ContentSize.Height) / (7); // Even spacing vertically for 6 menu items in body of layer
            for (int i = 1; i < this._pageCount + 1; i++)
            {
                var pageToggleOn =
                    new CCMenuItemImage(
                        "Images/Icons/JumpToPageButtonActive.png",
                        "Images/Icons/JumpToPageButtonNormal.png");
                var pageToggleOff =
                    new CCMenuItemImage(
                        "Images/Icons/JumpToPageButtonNormal.png",
                        "Images/Icons/JumpToPageButtonActive.png");
                var pageNumber = i;
                var pageToggle =
                    new CCMenuItemToggle((obj) => PageTogglePressed(pageNumber),
                    new CCMenuItem[] { pageToggleOn, pageToggleOff });
                pageToggle.SelectedIndex = 1; // Not active
                this._pageToggles.Add(pageToggle);
                var pageMenu = new CCMenu(
                    new CCMenuItem[] 
                    {
                        pageToggle,
                    });
                pageMenu.AnchorPoint = CCPoint.AnchorMiddle;
                var pageLabel = new CCLabelTTF(i.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                pageLabel.AnchorPoint = CCPoint.AnchorMiddle;
                this._pageLabels.Add(pageLabel);
                pageMenu.Position = new CCPoint(
                    0.42f * this.ContentSize.Width,
                    0.8f  * this.ContentSize.Height - (i * pageMenuMargin));
                pageLabel.Position = new CCPoint(
                    pageMenu.Position.X + (0.5f * pageToggleOn.ContentSize.Width),
                    pageMenu.Position.Y + (0.5f * pageToggleOn.ContentSize.Height));

                this.AddChild(pageMenu);
                this.AddChild(pageLabel);
            }

            // Start line subtitle
            var startLineText = string.Empty;
#if ANDROID
            startLineText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeStartLine);
#elif IOS
            startLineText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeStartLine, Strings.OptionsPracticeStartLine);
#else
            startLineText = AppResources.OptionsPracticeStartLine;
#endif
            var startLineLabel = new CCLabelTTF(startLineText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            startLineLabel.AnchorPoint = CCPoint.AnchorMiddle;
            startLineLabel.Rotation = -90;
            startLineLabel.Position = new CCPoint(
                0.58f * this.ContentSize.Width,
                0.5f  * this.ContentSize.Height);
            this.AddChild(startLineLabel);

            // Lines
            this._lineCount = 17;
            this._lineToggles = new List<CCMenuItemToggle>();
            this._lineLabels = new List<CCLabelTTF>();
            var lineMenuMargin = (0.7f * this.ContentSize.Height) / (18); // Even spacing vertically for 9 menu items in body of layer
            for (int i = 1; i < this._lineCount + 1; i++)
            {
                var lineToggleOn =
                    new CCMenuItemImage(
                        "Images/Icons/JumpToLineButtonActive.png",
                        "Images/Icons/JumpToLineButtonNormal.png");
                var lineToggleOff =
                    new CCMenuItemImage(
                        "Images/Icons/JumpToLineButtonNormal.png",
                        "Images/Icons/JumpToLineButtonActive.png");
                var lineNumber = i;
                var lineToggle =
                    new CCMenuItemToggle((obj) => LineTogglePressed(lineNumber),
                    new CCMenuItem[] { lineToggleOn, lineToggleOff });
                lineToggle.SelectedIndex = 1; // Not active
                this._lineToggles.Add(lineToggle);
                var lineMenu = new CCMenu(
                    new CCMenuItem[] 
                    {
                        lineToggle,
                    });
                lineMenu.AnchorPoint = CCPoint.AnchorMiddle;
                var lineLabel = new CCLabelTTF(i.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                lineLabel.AnchorPoint = CCPoint.AnchorMiddle;
                this._lineLabels.Add(lineLabel);
                if ( (i % 2) != 0)   
                {
                    // Odd lines to left and number on left
                    lineMenu.Position = new CCPoint(
                        0.7f * this.ContentSize.Width,
                        0.75f * this.ContentSize.Height - 
                        (i * lineMenuMargin));
                    lineLabel.Position = new CCPoint(
                        lineMenu.Position.X - (0.5f * lineToggleOn.ContentSize.Width),
                        lineMenu.Position.Y + (0.5f * lineToggleOn.ContentSize.Height));
                }
                else                
                {
                    // Even lines to right and number on right
                    // AND drop 1/2 way height-wise between odd lines
                    lineMenu.Position = new CCPoint(
                        0.8f * this.ContentSize.Width,
                        0.75f * this.ContentSize.Height -
                        (i * lineMenuMargin));
                    lineLabel.Position = new CCPoint(
                        lineMenu.Position.X + (0.5f * lineToggleOn.ContentSize.Width),
                        lineMenu.Position.Y + (0.5f * lineToggleOn.ContentSize.Height));
                }

                this.AddChild(lineMenu);
                this.AddChild(lineLabel);
            }

            // Kills
            var killsToggleOn =
                new CCMenuItemImage("Images/Icons/KillsButtonOn.png",
                                    "Images/Icons/KillsButtonOff.png");
            var killsToggleOff =
                new CCMenuItemImage("Images/Icons/KillsButtonOff.png",
                                    "Images/Icons/KillsButtonOn.png");
            var killsToggle =
                new CCMenuItemToggle((obj) => KillsTogglePressed((obj as CCMenuItemToggle).SelectedIndex),
                new CCMenuItem[] { killsToggleOn, killsToggleOff });
            if (!GameManager.SharedGameManager.GameAreKillsAllowed)
            {
                killsToggle.SelectedIndex = 1;
            }
            var killsMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        killsToggle,
                    });
            killsMenu.Position = new CCPoint(
                0.15f * this.ContentSize.Width,
                0.5f  * this.ContentSize.Height);
            this.AddChild(killsMenu);
            this._killsOn = string.Empty;
#if ANDROID
            this._killsOn = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeKillsOn);
#elif IOS
            this._killsOn = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeKillsOn, Strings.OptionsPracticeKillsOn);
#else
            this._killsOn = AppResources.OptionsPracticeKillsOn;
#endif
            this._killsOff = string.Empty;
#if ANDROID
            this._killsOff = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeKillsOff);
#elif IOS
            this._killsOff = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeKillsOff, Strings.OptionsPracticeKillsOff);
#else
            this._killsOff = AppResources.OptionsPracticeKillsOff;
#endif
            this._killsLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._killsLabel.Position = new CCPoint(
                0.15f * this.ContentSize.Width,
                0.42f * this.ContentSize.Height);
            this.AddChild(this._killsLabel);
            if (GameManager.SharedGameManager.GameAreKillsAllowed)
            {
                this._killsLabel.Text = this._killsOn;
            }
            else
            {
                this._killsLabel.Text = this._killsOff;
            }

            // Additional text
            this._switchingPageText = string.Empty;
#if ANDROID
            this._switchingPageText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeSwitchingPage);
#elif IOS
            this._switchingPageText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeSwitchingPage, Strings.OptionsPracticeSwitchingPage);
#else
            this._switchingPageText = AppResources.OptionsPracticeSwitchingPage;
#endif

            this._switchingLineText = string.Empty;
#if ANDROID
            this._switchingLineText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeSwitchingLine);
#elif IOS
            this._switchingLineText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeSwitchingLine, Strings.OptionsPracticeSwitchingLine);
#else
            this._switchingLineText = AppResources.OptionsPracticeSwitchingLine;
#endif
        }

        #region Overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Highlight current starting page/line
            var startPageToggle = this._pageToggles[GameManager.SharedGameManager.GameStartPageNumber - 1];
            startPageToggle.SelectedIndex = 0;
            var startPageLabel = this._pageLabels[GameManager.SharedGameManager.GameStartPageNumber - 1];
            startPageLabel.Color = CCColor3B.Blue;
            var startLineToggle = this._lineToggles[GameManager.SharedGameManager.GameStartLineNumber - 1];
            startLineToggle.SelectedIndex = 0;
            var startLineLabel = this._lineLabels[GameManager.SharedGameManager.GameStartLineNumber - 1];
            startLineLabel.Color = CCColor3B.Blue;
        }

        #endregion

        #region Helper methods

        private void PageTogglePressed(int pageNumber)
        {
            // Provide immediate feedback
            this._parent.TheMessageBoxLayer.Show(
                this._switchingPageText,
                pageNumber.ToString(),
                MessageBoxType.MB_PROGRESS);

            // Update ui to reflect selection
            foreach (var pageToggle in this._pageToggles)
            {
                pageToggle.SelectedIndex = 1;
            }
            this._pageToggles[pageNumber - 1].SelectedIndex = 0;
            foreach (var pageLabel in this._pageLabels)
            {
                pageLabel.Color = CCColor3B.White;
            }
            this._pageLabels[pageNumber - 1].Color = CCColor3B.Blue;

            // Record what was selected
            GameManager.SharedGameManager.GameStartPageNumber = pageNumber;

            // Hook up an event handler for end of content loading caused by
            // refresh kicking off background load
            this._obstacleCache.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;

            // Start the background refresh to get the new pad displayed. See 
            // LoadContentAsyncFinishedHandler for how we clean-up after refresh is finished
            this._parent.Refresh();
        }

        private void LineTogglePressed(int lineNumber)
        {
            // Provide immediate feedback
            this._parent.TheMessageBoxLayer.Show(
                this._switchingLineText,
                lineNumber.ToString(),
                MessageBoxType.MB_PROGRESS);

            // Update ui to reflect selection
            foreach (var lineToggle in this._lineToggles)
            {
                lineToggle.SelectedIndex = 1;
            }
            this._lineToggles[lineNumber - 1].SelectedIndex = 0;
            foreach (var lineLabel in this._lineLabels)
            {
                lineLabel.Color = CCColor3B.White;
            }
            this._lineLabels[lineNumber - 1].Color = CCColor3B.Blue;

            // Record what was selected
            GameManager.SharedGameManager.GameStartLineNumber = lineNumber;

            // Hook up an event handler for end of content loading caused by
            // refresh kicking off background load
            this._obstacleCache.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;

            // Start the background refresh to get the new pad displayed. See 
            // LoadContentAsyncFinishedHandler for how we clean-up after refresh is finished
            this._parent.Refresh();
        }

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            // We only want to react to a refresh event
            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                // Unhook so we are a one-shot event handler
                this._obstacleCache.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

                // We can now let this layer's ui resume
                this._parent.TheMessageBoxLayer.Hide();
            }
        }

        private void KillsTogglePressed(int selected)
        {
            if (selected == 0)
            {
                GameManager.SharedGameManager.GameAreKillsAllowed = true;
                this._killsLabel.Text = this._killsOn;
            }
            else
            {
                GameManager.SharedGameManager.GameAreKillsAllowed = false;
                this._killsLabel.Text = this._killsOff;

                // Signal we have a "Kills Off" event for this game run
                this._parent.TheHudLayer.KillsOffEventRecorded = true;
            }
        }

        #endregion

    }
}