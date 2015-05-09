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
        
        // Lines
        private CCSprite _linesImage;

        // Worlds
        private CCSprite _worldsImage;
        
        public OptionsPage1Layer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // We want touches so we can handle selection of resource pack/pad images
            this.TouchEnabled = true;
            this.TouchMode = CCTouchMode.OneByOne;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage1Of3);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage1Of3, Strings.CommonPage1Of3);
#else
            pageNumberText = AppResources.CommonPage1Of3;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Resource Packs (cell 1/4)
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

            // Pads (cell 2/4)
            this._padsImage = new CCSprite("Models/Pads/Pad1-thumbnail");
            Cocos2DUtils.ResizeSprite(this._padsImage,
                0.35f * this.ContentSize.Width,
                0.35f * this.ContentSize.Width);
            this._padsImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            this._padsImage.Position = new CCPoint(
                0.75f * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(this._padsImage);
            var padsText = string.Empty;
#if ANDROID
            padsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsPads);
#elif IOS
            padsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsPads, Strings.OptionsPads);
#else
            padsText = AppResources.OptionsPads;
#endif
            var padsLabel = new CCLabelTTF(padsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var padsButton = new CCMenuItemLabel(padsLabel,
                                                 (obj) => { _parent.Navigate(LayerTags.PadsLayer); });
            var padsMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        padsButton
                    });
            padsMenu.Position = new CCPoint(
                0.7f * this.ContentSize.Width, 
                0.5f  * this.ContentSize.Height);
            this.AddChild(padsMenu);

            // Lines (cell 3/4)
            this._linesImage = new CCSprite("Models/Lines/Line1-thumbnail");
            Cocos2DUtils.ResizeSprite(this._linesImage,
                0.3f * this.ContentSize.Width,
                0.3f * this.ContentSize.Width);
            this._linesImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            this._linesImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
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
            var linesLabel = new CCLabelTTF(linesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var linesButton = new CCMenuItemLabel(linesLabel,
                                                 (obj) => { _parent.Navigate(LayerTags.LinesLayer); });
            var linesMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        linesButton
                    });
            linesMenu.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(linesMenu);

            // Worlds (cell 4/4)
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

            // Did we touch a resource pack image?
            var resourcePackBoundingBox = _resourcePackImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref resourcePackBoundingBox, ref location) )
            {
                _parent.Navigate(LayerTags.ResourcePacksLayer);
                return true;
            }

            // Ok, did we touch the area containing the pads image?
            // var padsBoundingBox = _padsImage.BoundingBox;
            var padsBoundingBox = _padsImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref padsBoundingBox, ref location) )
            {
                _parent.Navigate(LayerTags.PadsLayer);
                return true;
            }

            // Ok, did we touch the area containing the lines image?
            var linesBoundingBox = _linesImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref linesBoundingBox, ref location) )
            {
                _parent.Navigate(LayerTags.LinesLayer);
                return true;
            }

            // And finally, did we touch the area containing the worlds image?
            var worldsBoundingBox = this._worldsImage.WorldBoundingBox;
            if (CCRect.ContainsPoint(ref worldsBoundingBox, ref location))
            {
                _parent.Navigate(LayerTags.WorldsLayer);
                return true;
            }

            return true;
        }

        #endregion
    }
}