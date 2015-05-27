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

        // Resource pack
        private CCSprite _resourcePackImage;
        
        // Pads
        private CCSprite _padsImage;
        
        // Worlds
        private CCSprite _worldsImage;
        
        public OptionsPage1Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // We want touches so we can handle selection of pad image
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            this._padsImage = new CCSprite("Models/Pads/Pad1-thumbnail");
            this._padsImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            this._padsImage.Position = new CCPoint(
                0.6f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            this.AddChild(this._padsImage);

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

            // TODO: Resource Packs (cell 1/4)
            /*
            _resourcePackImage = new CCSprite("Images/Misc/OptionsResourcePack1");
            Cocos2DUtils.ResizeSprite(_resourcePackImage,
                0.3f * this.ContentSize.Width,
                0.3f * this.ContentSize.Width);
            _resourcePackImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            _resourcePackImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            _resourcePackImage.Visible = true;
            this.AddChild(_resourcePackImage);
            var resourcePacksText = string.Empty;
#if ANDROID
            resourcePacksText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsResourcePacks);
#elif IOS
            resourcePacksText = NSBundle.MainBundle.LocalizedString(Strings.OptionsResourcePacks, Strings.OptionsResourcePacks);
#else
            resourcePacksText = AppResources.OptionsResourcePacks;
#endif
            var resourcePackLabel = new CCLabelTTF(resourcePacksText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            CCMenuItemLabel resourcePackButton = new CCMenuItemLabel(resourcePackLabel,
                                                                (obj) => { _parent.Navigate(LayerTags.ResourcePacksLayer); });
            var resourcePacksMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        resourcePackButton
                    });
            resourcePacksMenu.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.5f  * this.ContentSize.Height);
            this.AddChild(resourcePacksMenu);
            */

            // Pads (cell 1/4)
            var padsText = string.Empty;
#if ANDROID
            padsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPads);
#elif IOS
            padsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPads, Strings.OptionsPads);
#else
            padsText = AppResources.OptionsPads;
#endif
            var padsLabel = new CCLabelTTF(padsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
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

            // TODO: Worlds (cell 4/4)
            /*
            this._worldsImage = new CCSprite("Images/Misc/OptionsResourcePack3");
            Cocos2DUtils.ResizeSprite(this._worldsImage, 
                0.3f * this.ContentSize.Width, 
                0.3f * this.ContentSize.Width);
            this._worldsImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            this._worldsImage.Position = new CCPoint(
                0.7f  * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(this._worldsImage);
            var worldsText = string.Empty;
#if ANDROID
            worldsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsWorlds);
#elif IOS
            worldsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsWorlds, Strings.OptionsWorlds);
#else
            worldsText = AppResources.OptionsWorlds;
#endif
            var worldsLabel = new CCLabelTTF(worldsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            CCMenuItemLabel worldsButton = new CCMenuItemLabel(worldsLabel,
                                                               (obj) => { _parent.Navigate(LayerTags.WorldsLayer); });
            var worldsMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        worldsButton
                    });
            worldsMenu.Position = new CCPoint(
                0.7f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(worldsMenu);
            */
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void Draw()
        {
            base.Draw();
        }

        #endregion

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

            // TODO: Did we touch a resource pack image?
            /*
            var resourcePackBoundingBox = _resourcePackImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref resourcePackBoundingBox, ref location) )
            {
                _parent.Navigate(LayerTags.ResourcePacksLayer);
                return true;
            }
            */

            // Ok, did we touch the area containing the pads image?
            var padsBoundingBox = _padsImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref padsBoundingBox, ref location) )
            {
                _parent.Navigate(LayerTags.PadsLayer);
                return true;
            }

            // TODO: And finally, did we touch the area containing the worlds image?
            /*
            var worldsBoundingBox = this._worldsImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref worldsBoundingBox, ref location))
            {
                _parent.Navigate(LayerTags.WorldsLayer);
                return true;
            }
            */

            return true;
        }

        #endregion
    }
}