using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
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

        private IList<CCSprite> _pageImages;

        public OptionsPage3Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // We want touches so we can handle selection of page images
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;

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
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Practice mode on/off
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

        #endregion

        #region Helper methods

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