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

        // Time
        private KeyboardNotificationLayer _timeNotificationLayer;
        private CCTextFieldTTF _timeTextField;

        // Practice
        private IList<CCSprite> _pageImages;

        public OptionsPage3Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // TODO: We want touches so we can handle selection of page images
            /*
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;
            */

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage3Of3);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage3Of3, Strings.CommonPage3Of3);
#else
            pageNumberText = AppResources.CommonPage3Of3;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Sound (cell 1/2)
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
                0.65f * this.ContentSize.Height);
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
                0.55f * this.ContentSize.Height);
            this.AddChild(soundLabelMenu);

            // TODO: Time (cell 2/4)
            /*
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
            */

            // TODO: Leaderboards (cell 3/4)
            /*
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
                0.3f * this.ContentSize.Height);
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
                0.2f * this.ContentSize.Height);
            this.AddChild(leaderboardLabelMenu);
            */

            // Credits (cell 2/2)
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
                0.35f * this.ContentSize.Height);
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
                0.25f * this.ContentSize.Height);
            this.AddChild(creditsLabelMenu);

            // TODO: Practice mode on/off
            /*
            CCMenuItemImage practiceModeToggleOn =
                new CCMenuItemImage("Images/Icons/SoundButtonOn.png",
                                    "Images/Icons/SoundButtonOff.png");
            CCMenuItemImage practiceModeToggleOff =
                new CCMenuItemImage("Images/Icons/SoundButtonOff.png",
                                    "Images/Icons/SoundButtonOn.png");
            CCMenuItemToggle practiceModeToggle =
                new CCMenuItemToggle((obj) => PracticeModeTogglePressed(),
                new CCMenuItem[] { practiceModeToggleOn, practiceModeToggleOff });
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE,
                GameConstants.USER_DEFAULT_INITIAL_PRACTICE_MODE) == false)
            {
                practiceModeToggle.SelectedIndex = 1; // Practice mode is OFF
            }
            practiceModeToggle.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.6f  * this.ContentSize.Height);
            this.AddChild(practiceModeToggle);
            CCLabelTTF practiceModeLabel = null;
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE,
                GameConstants.USER_DEFAULT_INITIAL_PRACTICE_MODE) == true)
            {
                var practiceModeOnText = string.Empty;
#if ANDROID
                practiceModeOnText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeModeOn);
#elif IOS
                practiceModeOnText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeModeOn, Strings.OptionsPracticeModeOn);
#else
                practiceModeOnText = AppResources.OptionsPracticeModeOn;
#endif
                practiceModeLabel = new CCLabelTTF(practiceModeOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            }
            else
            {
                var practiceModeOffText = string.Empty;
#if ANDROID
                practiceModeOffText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPracticeModeOff);
#elif IOS
                practiceModeOffText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPracticeModeOff, Strings.OptionsPracticeModeOff);
#else
                practiceModeOffText = AppResources.OptionsPracticeModeOff;
#endif
                practiceModeLabel = new CCLabelTTF(practiceModeOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            }
            var practiceModeItem = new CCMenuItemLabel(practiceModeLabel,
                                        (obj) => { this.PracticeModeTogglePressed(); });
            var practiceModeLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        practiceModeItem
                    });
            practiceModeLabelMenu.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.5f  * this.ContentSize.Height);
            this.AddChild(practiceModeLabelMenu);

            this._pageImages = new List<CCSprite>();
            var pageCount = 1;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var page = "Images/Misc/OptionsPracticeMode" + pageCount;
                    this.LoadPageImage(page, row, col);

                    pageCount++;
                }
            }
            */
        }

        #region Touch Implementation

        /*
        http://www.cocos2d-iphone.org/forums/topic/tutorials-dont-mention-cctouchdispatcherremovedelegate/
         
        Setting self.isTouchEnabled to YES in a CCLayer causes RegisterWithTouchDispatcher 
        to be called in onEnter, and CCDirector.SharedDirector.TouchDispatcher.RemoveDelegate(this)
        to be called in onExit.

        RegisterWithTouchDispatcher in CCLayer registers as a Standard touch delegate. 
        So you only need to override it if you want the Targeted touch messages.
            
        Note if you don't set CCTouchMode it will default to CCTouchMode.AllAtOnce, which means
        override TouchesBegan. Otherwise set CCTouchMode to CCTouchMode.OneByOne and override
        TouchBegan.
         
        In TouchBegan, If you return true then ccTouchEnded will called. 
        If you return false then ccTouchEnded will not be called, and the event 
        will go the parent layer
        */

        /// <summary>
        /// If you return true then ccTouchEnded will called. 
        /// If you return false then ccTouchEnded will not be called, and the event will go the parent layer
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        /* TODO
        public override bool TouchBegan(CCTouch touch)
        {
            // Short ciruit if practice mode is off
            if (!UserDefaults.SharedUserDefault.GetBoolForKey(
                GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE,
                GameConstants.USER_DEFAULT_INITIAL_PRACTICE_MODE))
            {
                return true;
            }

            var location = touch.Location;

            // Did we touch a page image?
            bool touchedPageImage = false;
            var pageCount = 1;
            for(var i = 0; i < this._pageImages.Count; i++)
            {
                var boundingBox = this._pageImages[i].BoundingBox;
                if (CCRect.ContainsPoint(ref boundingBox, ref location))
                {
                    touchedPageImage = true;
                    break;
                }

                pageCount++;
            }

            // If so, then navigate to resource packs screen
            if (touchedPageImage)
            {
                UserDefaults.SharedUserDefault.SetIntegerForKey(
                    GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE_START_PAGE,
                    pageCount);
            }

            return true;
        }
        */
        #endregion

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

        private void PracticeModeTogglePressed() 
        {
            if (UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE,
                    GameConstants.USER_DEFAULT_INITIAL_PRACTICE_MODE))
            {
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE,
                    false);
	        } 
            else
            {
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_PRACTICE_MODE,
                    true);
            }
        }

        private void LoadPageImage(string page, int row, int col)
        {
            var pageImage = new CCSprite(page);
            Cocos2DUtils.ResizeSprite(pageImage,
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Width);
            pageImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            pageImage.Position = new CCPoint(
                0.1f * col * this.ContentSize.Width,
                0.1f * row * this.ContentSize.Height);
            pageImage.Visible = true;

            this._pageImages.Add(pageImage);
            this.AddChild(pageImage);
        }


        #endregion
    }
}