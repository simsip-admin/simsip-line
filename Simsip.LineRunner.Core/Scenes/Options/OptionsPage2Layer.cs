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

        // Lines
        private CCSprite _linesImage;

        public OptionsPage2Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // We want touches so we can handle selection of line images
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage, Strings.CommonPage);
#else
            pageNumberText = AppResources.CommonPage;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 2", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Lines
            this._linesImage = new CCSprite("Models/Lines/Line-thumbnail");
            this._linesImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            this._linesImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(this._linesImage);
            var linesText = string.Empty;
#if ANDROID
            linesText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLines);
#elif IOS
            linesText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLines, Strings.OptionsLines);
#else
            linesText = AppResources.OptionsLines;
#endif
            var linesLabel = new CCLabelTTF(linesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            var linesButton = new CCMenuItemLabel(linesLabel,
                                                 (obj) => { _parent.Navigate(LayerTags.LinesLayer); });
            var linesMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        linesButton
                    });
            linesMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(linesMenu);


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

            // Ok, did we touch the area containing the lines image?
            var linesBoundingBox = _linesImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref linesBoundingBox, ref location))
            {
                _parent.Navigate(LayerTags.LinesLayer);
                return true;
            }

            return true;
        }

        #endregion
    }
}