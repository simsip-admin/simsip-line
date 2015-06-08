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
using System.Collections.Generic;
using System.Diagnostics;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Upgrades
{
    public class UpgradesPage1Layer : GameLayer
    {
        private CoreScene _parent;
        private UpgradesMasterLayer _masterLayer;

        // Practice mode
        private CCSprite _practiceModeImage;
        
        public UpgradesPage1Layer(CoreScene parent, UpgradesMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // We want touches so we can handle selection of practice mode image
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            this._practiceModeImage = new CCSprite("Models/Pads/Pad1-thumbnail");
            this._practiceModeImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            this._practiceModeImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            this.AddChild(this._practiceModeImage);

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage, Strings.CommonPage);
#else
            pageNumberText = AppResources.CommonPage;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 1", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Practice
            var practiceText = string.Empty;
#if ANDROID
            practiceText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPractice);
#elif IOS
            practiceText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPractice, Strings.UpgradesPractice);
#else
            practiceText = AppResources.UpgradesPractice;
#endif
            var padsLabel = new CCLabelTTF(practiceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            var padsButton = new CCMenuItemLabel(padsLabel,
                                                 (obj) => { _parent.Navigate(LayerTags.PadsLayer); });
            var padsMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        padsButton
                    });
            padsMenu.Position = new CCPoint(
                0.85f * this.ContentSize.Width, 
                0.5f  * this.ContentSize.Height);
            this.AddChild(padsMenu);

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
            var location = touch.Location;

            // Ok, did we touch the area containing the pads image?
            var padsBoundingBox = _practiceModeImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref padsBoundingBox, ref location) )
            {
                _parent.Navigate(LayerTags.PadsLayer);
                return true;
            }

            return true;
        }

        #endregion
    }
}