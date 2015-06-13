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
    public class OptionsPage3Layer : GameLayer
    {
        private CoreScene _parent;
        private OptionsMasterLayer _masterLayer;

        public OptionsPage3Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Sound
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
            var soundMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        soundToggle,
                    });
            soundMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(soundMenu);
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
                soundLabel = new CCLabelTTF(soundOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
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
                soundLabel = new CCLabelTTF(soundOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            }
            var soundItem = new CCMenuItemLabel(soundLabel,
                                        (obj) => { this.SoundTogglePressed(); });
            var soundLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        soundItem
                    });
            soundLabelMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(soundLabelMenu);

            // Leaderboards
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
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(leaderboardButtonMenu);
            var leaderboardsText = string.Empty;
#if ANDROID
            leaderboardsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLeaderboards);
#elif IOS
            leaderboardsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLeaderboards, Strings.OptionsLeaderboards);
#else
            leaderboardsText = AppResources.OptionsLeaderboards;
#endif
            var leaderboardLabel = new CCLabelTTF(leaderboardsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            var leaderboardItem = new CCMenuItemLabel(leaderboardLabel,
                                        (obj) => { _parent.Navigate(LayerTags.AchievementsLayer); });
            var leaderboardLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        leaderboardItem
                    });
            leaderboardLabelMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(leaderboardLabelMenu);

            // Credits
            CCMenuItemImage creditsButton =
                new CCMenuItemImage("Images/Icons/CreditsButtonNormal.png",
                                    "Images/Icons/CreditsButtonSelected.png",
                                    (obj) => { _parent.Navigate(LayerTags.CreditsMasterLayer); });
            var creditsMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        creditsButton, 
                    });
            creditsMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
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
            var creditsLabel = new CCLabelTTF(creditsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            var creditsItem = new CCMenuItemLabel(creditsLabel,
                                        (obj) => { _parent.Navigate(LayerTags.CreditsMasterLayer); });
            var creditsLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        creditsItem
                    });
            creditsLabelMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
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