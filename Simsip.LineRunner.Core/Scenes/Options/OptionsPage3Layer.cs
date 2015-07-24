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

        // Sound
        private CCMenuItemToggle _soundToggle;
        private CCMenuItemToggle _soundLabelToggle;

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
            this._soundToggle =
                new CCMenuItemToggle((obj) => SoundTogglePressed((obj as CCMenuItemToggle).SelectedIndex),
                new CCMenuItem[] { soundToggleOn, soundToggleOff });
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    GameConstants.USER_DEFAULT_INITIAL_SOUND) == false)
            {
                this._soundToggle.SelectedIndex = 1; // SFX are OFF
            }
            var soundMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._soundToggle,
                    });
            soundMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(soundMenu);

            var soundOnText = string.Empty;
#if ANDROID
            soundOnText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsSoundOn);
#elif IOS
            soundOnText = NSBundle.MainBundle.LocalizedString(Strings.OptionsSoundOn, Strings.OptionsSoundOn);
#else
            soundOnText = AppResources.OptionsSoundOn;
#endif
            var soundOnLabel = new CCLabelTTF(soundOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            soundOnLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            var soundOnItem = new CCMenuItemLabel(soundOnLabel);

            var soundOffText = string.Empty;
#if ANDROID
            soundOffText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsSoundOff);
#elif IOS
            soundOffText = NSBundle.MainBundle.LocalizedString(Strings.OptionsSoundOff, Strings.OptionsSoundOff);
#else
            soundOffText = AppResources.OptionsSoundOff;
#endif
            var soundOffLabel = new CCLabelTTF(soundOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            soundOffLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            var soundOffItem = new CCMenuItemLabel(soundOffLabel);

            this._soundLabelToggle =
                new CCMenuItemToggle((obj) => SoundTogglePressed((obj as CCMenuItemToggle).SelectedIndex),
                new CCMenuItem[] { soundOnItem, soundOffItem });
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    GameConstants.USER_DEFAULT_INITIAL_SOUND) == false)
            {
                this._soundLabelToggle.SelectedIndex = 1; // Sound off
            }
            var soundLabelMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._soundLabelToggle,
                    });
            soundLabelMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(soundLabelMenu);

            // Leaderboards
            var leaderboardButton =
                new CCMenuItemImage(
                    "Images/Icons/AchievementsButtonNormal.png",
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
            leaderboardLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
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
            creditsLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
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

        private void SoundTogglePressed(int selectedIndex)
        {
            // Update ui
            this._soundToggle.SelectedIndex = selectedIndex;
            this._soundLabelToggle.SelectedIndex = selectedIndex;

            // Update state
            if (selectedIndex == 0)
            {
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    true);
            }
            else
            {
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_SOUND,
                    false);
            }
        }

        #endregion
    }
}