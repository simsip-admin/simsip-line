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


namespace Simsip.LineRunner.Scenes.Options
{
    public class OptionsPage1Layer : GameLayer
    {
        private CoreScene _parent;
        private OptionsMasterLayer _masterLayer;

        // Pads
        private CCSprite _padsImage;
        
        public OptionsPage1Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // We want touches so we can handle selection of pad image
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Pads image
            this._padsImage = new CCSprite("Models/Pads/Pad1-thumbnail");
            this._padsImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            Cocos2DUtils.ResizeSprite(this._padsImage,      // Assuming square
                this.ContentSize.Width,
                this.ContentSize.Width);
            this._padsImage.Position = new CCPoint(
                0.6f  * this.ContentSize.Width,
                0.15f * this.ContentSize.Height);
            this.AddChild(this._padsImage);

            // Pads
            var padsText = string.Empty;
#if ANDROID
            padsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPads);
#elif IOS
            padsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPads, Strings.OptionsPads);
#else
            padsText = AppResources.OptionsPads;
#endif
            var padsLabel = new CCLabelTTF(padsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            padsLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            var padsButton = new CCMenuItemLabel(padsLabel,
                                                 (obj) => { _parent.Navigate(LayerTags.PadsLayer); });
            var padsMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        padsButton
                    });
            padsMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
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
            var padsBoundingBox = _padsImage.WorldBoundingBox;
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