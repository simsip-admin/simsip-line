using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Options
{
    public class OptionsPage2Layer : GameLayer
    {
        private CoreScene _parent;
        private OptionsMasterLayer _masterLayer;

        // Time
        private KeyboardNotificationLayer _timeNotificationLayer;
        private CCTextFieldTTF _timeTextField;

        public OptionsPage2Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage2Of3);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage2Of3, Strings.CommonPage2Of3);
#else
            pageNumberText = AppResources.CommonPage2Of3;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Sound (cell 1/4)
            CCMenuItemImage soundToggleOn =
                new CCMenuItemImage("Images/Icons/SoundButtonOn.png",
                                    "Images/Icons/SoundButtonOff.png");
            CCMenuItemImage soundToggleOff =
                new CCMenuItemImage("Images/Icons/SoundButtonOff.png",
                                    "Images/Icons/SoundButtonOn.png");
            CCMenuItemToggle soundToggle =
                new CCMenuItemToggle((obj) => SoundTogglePressed(),
                new CCMenuItem[] { soundToggleOn, soundToggleOff });
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    GameConstants.USER_DEFAULT_INITIAL_SOUND) == false)
            {
                soundToggle.SelectedIndex = 1; // SFX are OFF
            }
            soundToggle.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.6f  * this.ContentSize.Height);
            this.AddChild(soundToggle);
            CCLabelTTF soundLabel = null;
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    GameConstants.USER_DEFAULT_INITIAL_SOUND) == true)
            {
                var soundOnText = string.Empty;
#if ANDROID
                soundOnText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsSoundOn);
#elif IOS
                soundOnText = NSBundle.MainBundle.LocalizedString(Strings.OptionsSoundOn, Strings.OptionsSoundOn);
#else
                soundOnText = AppResources.OptionsSoundOn;
#endif
                soundLabel = new CCLabelTTF(soundOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            }
            else
            {
                var soundOffText = string.Empty;
#if ANDROID
                soundOffText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsSoundOff);
#elif IOS
                soundOffText = NSBundle.MainBundle.LocalizedString(Strings.OptionsSoundOff, Strings.OptionsSoundOff);
#else
                soundOffText = AppResources.OptionsSoundOff;
#endif
                soundLabel = new CCLabelTTF(soundOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            }
            var soundItem = new CCMenuItemLabel(soundLabel,
                                        (obj) => { this.SoundTogglePressed(); });
            var soundLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        soundItem
                    });
            soundLabelMenu.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.5f  * this.ContentSize.Height);
            this.AddChild(soundLabelMenu);

            // Time (cell 2/4)
            var timeText = string.Empty;
#if ANDROID
            timeText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsTime);
#elif IOS
            timeText = NSBundle.MainBundle.LocalizedString(Strings.OptionsTime, Strings.OptionsTime);
#else
            timeText = AppResources.OptionsTime;
#endif
            var timePageLabel = new CCLabelTTF(timeText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            timePageLabel.Position = new CCPoint(
                0.7f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(timePageLabel);
            this._timeTextField = new CCTextFieldTTF(GameManager.SharedGameManager.AdminStartPageNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._timeTextField.Position = new CCPoint(
                0.9f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this._timeTextField.AutoEdit = true;
            this._timeTextField.EditTitle = timeText;
            var timeDescriptionText = string.Empty;
#if ANDROID
            timeDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsTimeDescription);
#elif IOS
            timeDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.OptionsTimeDescription, Strings.OptionsTimeDescription);
#else
            timeDescriptionText = AppResources.OptionsTimeDescription;
#endif
            this._timeTextField.EditDescription = timeDescriptionText;
            this._timeTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._timeTextField.ContentSize.Width,
                1f * this._timeTextField.ContentSize.Height);
            this._timeNotificationLayer = new KeyboardNotificationLayer(this._timeTextField);
            AddChild(this._timeNotificationLayer);

            // Leaderboards (cell 3/4)
            var leaderboardButton =
                new CCMenuItemImage("Images/Icons/AchievementsButtonNormal.png",
                                    "Images/Icons/AchievementsButtonSelected.png",
                                    (obj) => { _parent.Navigate(LayerTags.AchievementsLayer); });
            var leaderboardButtonMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        leaderboardButton, 
                    });
            leaderboardButtonMenu.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.3f  * this.ContentSize.Height);
            this.AddChild(leaderboardButtonMenu);
            var leaderboardsText = string.Empty;
#if ANDROID
            leaderboardsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLeaderboards);
#elif IOS
            leaderboardsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLeaderboards, Strings.OptionsLeaderboards);
#else
            leaderboardsText = AppResources.OptionsLeaderboards;
#endif
            var leaderboardLabel = new CCLabelTTF(leaderboardsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var leaderboardItem = new CCMenuItemLabel(leaderboardLabel,
                                        (obj) => { _parent.Navigate(LayerTags.AchievementsLayer); });
            var leaderboardLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        leaderboardItem
                    });
            leaderboardLabelMenu.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.2f  * this.ContentSize.Height);
            this.AddChild(leaderboardLabelMenu);

            // Credits (cell 4/4)
            CCMenuItemImage creditsButton =
                new CCMenuItemImage("Images/Icons/RatingsButtonNormal.png",
                                    "Images/Icons/RatingsButtonSelected.png",
                                    (obj) => { _parent.Navigate(LayerTags.CreditsMasterLayer); });
            var creditsMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        creditsButton, 
                    });
            creditsMenu.Position = new CCPoint(
                0.7f * this.ContentSize.Width, 
                0.3f * this.ContentSize.Height);
            this.AddChild(creditsMenu);
            var creditsText = string.Empty;
#if ANDROID
            creditsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsCredits);
#elif IOS
            creditsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsCredits, Strings.OptionsCredits);
#else
            creditsText = AppResources.OptionsCredits;
#endif
            var creditsLabel = new CCLabelTTF(creditsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var creditsItem = new CCMenuItemLabel(creditsLabel,
                                        (obj) => { _parent.Navigate(LayerTags.CreditsMasterLayer); });
            var creditsLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        creditsItem
                    });
            creditsLabelMenu.Position = new CCPoint(
                0.7f * this.ContentSize.Width, 
                0.2f  * this.ContentSize.Height);
            this.AddChild(creditsLabelMenu);
        }

        #region Helper methods

        private void SoundTogglePressed() 
        {
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    GameConstants.USER_DEFAULT_INITIAL_SOUND))
            {
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    false);
	        } 
            else
            {
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    true);
	        }
        }

        #endregion
    }
}