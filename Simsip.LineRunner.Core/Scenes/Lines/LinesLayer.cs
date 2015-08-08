using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects;
#if IOS
using Foundation;
#endif
#if NETFX_CORE
using Windows.Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Lines
{
    public class LinesLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Lines
        private ILineRepository _lineRepository;
        private IPageLinesRepository _pageLinesRepository;
        private IList<LineEntity> _lineEntities;

        // Services we'll need
        private ICharacterCache _characterCache;

        public LinesLayer(CoreScene parent)
        {
            this._parent = parent;

            // Grab reference to services we'll need
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                0.9f * screenSize.Width,
                0.9f * screenSize.Height);

            // Layer transition in/out
            var layerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.05f * screenSize.Width,
                0.05f * screenSize.Height);
            var layerStartPosition = new CCPoint(
                layerEndPosition.X,
                screenSize.Height);
            var layerStartPlacementAction = new CCPlace(layerStartPosition);
            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
            this._layerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
            );
            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
            var layerNavigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
            this._layerActionOut = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, layerNavigateAction })
            );

            // Setup strings
            var blueText = string.Empty;
#if ANDROID
            blueText = Program.SharedProgram.Resources.GetString(Resource.String.LinesBlue);
#elif IOS
            blueText = NSBundle.MainBundle.LocalizedString(Strings.LinesBlue, Strings.LinesBlue);
#else
            blueText = AppResources.LinesBlue;
#endif
            var lightBlueText = string.Empty;
#if ANDROID
            lightBlueText = Program.SharedProgram.Resources.GetString(Resource.String.LinesLightBlue);
#elif IOS
            lightBlueText = NSBundle.MainBundle.LocalizedString(Strings.LinesLightBlue, Strings.LinesLightBlue);
#else
            lightBlueText = AppResources.LinesLightBlue;
#endif
            var lightGreenText = string.Empty;
#if ANDROID
            lightGreenText = Program.SharedProgram.Resources.GetString(Resource.String.LinesLightGreen);
#elif IOS
            lightGreenText = NSBundle.MainBundle.LocalizedString(Strings.LinesLightGreen, Strings.LinesLightGreen);
#else
            lightGreenText = AppResources.LinesLightGreen;
#endif
            var greenText = string.Empty;
#if ANDROID
            greenText = Program.SharedProgram.Resources.GetString(Resource.String.LinesGreen);
#elif IOS
            greenText = NSBundle.MainBundle.LocalizedString(Strings.LinesGreen, Strings.LinesGreen);
#else
            greenText = AppResources.LinesGreen;
#endif
            var darkGreenText = string.Empty;
#if ANDROID
            darkGreenText = Program.SharedProgram.Resources.GetString(Resource.String.LinesDarkGreen);
#elif IOS
            darkGreenText = NSBundle.MainBundle.LocalizedString(Strings.LinesDarkGreen, Strings.LinesDarkGreen);
#else
            darkGreenText = AppResources.LinesDarkGreen;
#endif
            var orangeText = string.Empty;
#if ANDROID
            orangeText = Program.SharedProgram.Resources.GetString(Resource.String.LinesOrange);
#elif IOS
            orangeText = NSBundle.MainBundle.LocalizedString(Strings.LinesOrange, Strings.LinesOrange);
#else
            orangeText = AppResources.LinesOrange;
#endif
            var lightOrangeText = string.Empty;
#if ANDROID
            lightOrangeText = Program.SharedProgram.Resources.GetString(Resource.String.LinesLightOrange);
#elif IOS
            lightOrangeText = NSBundle.MainBundle.LocalizedString(Strings.LinesLightOrange, Strings.LinesLightOrange);
#else
            lightOrangeText = AppResources.LinesLightOrange;
#endif
            var darkOrangeText = string.Empty;
#if ANDROID
            darkOrangeText = Program.SharedProgram.Resources.GetString(Resource.String.LinesDarkOrange);
#elif IOS
            darkOrangeText = NSBundle.MainBundle.LocalizedString(Strings.LinesDarkOrange, Strings.LinesDarkOrange);
#else
            darkOrangeText = AppResources.LinesDarkOrange;
#endif
            var redText = string.Empty;
#if ANDROID
            redText = Program.SharedProgram.Resources.GetString(Resource.String.LinesRed);
#elif IOS
            redText = NSBundle.MainBundle.LocalizedString(Strings.LinesRed, Strings.LinesRed);
#else
            redText = AppResources.LinesRed;
#endif

            // Setup dictionary to lookup strings
            var lineDisplayNames = new Dictionary<string, string>();
            lineDisplayNames.Add("blue", blueText);
            lineDisplayNames.Add("light blue", lightBlueText);
            lineDisplayNames.Add("light green", lightGreenText);
            lineDisplayNames.Add("green", greenText);
            lineDisplayNames.Add("dark green", darkGreenText);
            lineDisplayNames.Add("orange", orangeText);
            lineDisplayNames.Add("light orange", lightOrangeText);
            lineDisplayNames.Add("dark orange", darkOrangeText);
            lineDisplayNames.Add("red", redText);

            // Lines menu
            this._lineEntities = new List<LineEntity>();
            this._lineRepository = new LineRepository();
            this._pageLinesRepository = new PageLinesRepository();
            var pageLinesEntity = this._pageLinesRepository.GetLine(1, 1);
            this._lineEntities = this._lineRepository.GetLines();
            var y = 0.9f * this.ContentSize.Height;
            foreach (var line in this._lineEntities)
            {
                var lineImage = new CCSprite("Models/Lines/" + line.ModelName + "-thumbnail");
                Cocos2DUtils.ResizeSprite(lineImage,
                    0.5f * this.ContentSize.Width,
                    0.2f * this.ContentSize.Width);
                lineImage.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    y + (0.01f * this.ContentSize.Height));
                this.AddChild(lineImage);

                var lineLabel = new CCLabelTTF(lineDisplayNames[line.DisplayName], GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                lineLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
                lineLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
                var lineEntity = line;  // Need to do this for correct lambda resolution inside a foreach loop
                var lineButton = new CCMenuItemLabel(lineLabel,
                                                     (obj) => { LineSelected(lineEntity); });

                var labelMenu = new CCMenu();
                labelMenu.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    y);
                labelMenu.AddChild(lineButton);
                this.AddChild(labelMenu);

                y -= 0.09f * this.ContentSize.Height;
            }


            // Back
            CCMenuItemImage backButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { this._parent.GoBack(); });
            var backMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        backButton, 
                    });
            backMenu.AnchorPoint = CCPoint.AnchorMiddle;
            backMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.1f * this.ContentSize.Height);
            this.AddChild(backMenu);
            var backText = string.Empty;
#if ANDROID
            backText = Program.SharedProgram.Resources.GetString(Resource.String.CommonBack);
#elif IOS
            backText = NSBundle.MainBundle.LocalizedString(Strings.CommonBack, Strings.CommonBack);
#else
            backText = AppResources.CommonBack;
#endif
            var backLabel = new CCLabelTTF(backText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            backLabel.Scale = GameConstants.FONT_SIZE_SMALL_SCALE;
            backLabel.AnchorPoint = CCPoint.AnchorMiddle;
            backLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this.AddChild(backLabel);
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate layer
            this.RunAction(this._layerActionIn);
        }

        #endregion

        #region Helper methods

        private void LineSelected(LineEntity line)
        {
            // Provide immediate feedback
            var switchingLinesText = string.Empty;
#if ANDROID
            switchingLinesText = Program.SharedProgram.Resources.GetString(Resource.String.LinesSwitchingLines);
#elif IOS
            switchingLinesText = NSBundle.MainBundle.LocalizedString(Strings.LinesSwitchingLines, Strings.LinesSwitchingLines);
#else
            switchingLinesText = AppResources.LinesSwitchingLines;
#endif

            this._parent.TheMessageBoxLayer.Show(
                switchingLinesText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Record what was selected
            UserDefaults.SharedUserDefault.SetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_LINE,
                line.ModelName);

            // Hook up an event handler for end of content loading caused by
            // refresh kicking off background load
            this._characterCache.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;

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
                this._characterCache.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

                // We can now let this layer's ui resume
                this._parent.TheMessageBoxLayer.Hide();
            }
        }

        #endregion
    }
}