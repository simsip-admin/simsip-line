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


namespace Simsip.LineRunner.Scenes.Credits
{
    public class CreditsPage3Layer : GameLayer
    {
        private CoreScene _parent;
        private CreditsMasterLayer _masterLayer;

        public CreditsPage3Layer(CoreScene parent, CreditsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

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

            var iconsTitle = new CCLabelTTF("Icons by freepik (freepik.com)", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            iconsTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(iconsTitle);

            /* TODO: Add back in when resource packs are ready
            var resourcePacksText = string.Empty;
#if ANDROID
            resourcePacksText = Program.SharedProgram.Resources.GetString(Resource.String.CreditsResourcePacks);
#elif IOS
            resourcePacksText = NSBundle.MainBundle.LocalizedString(Strings.CreditsResourcePacks, Strings.CreditsResourcePacks);
#else
            resourcePacksText = AppResources.CreditsResourcePacks;
#endif
            var resourcePacksTitle = new CCLabelTTF(resourcePacksText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            resourcePacksTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(resourcePacksTitle);

            var defaultResourceTitle = new CCLabelTTF("Default", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            defaultResourceTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(defaultResourceTitle);

            var defaultResourceLink = new CCLabelTTF("Voxeliq (https://github.com/raistlinthewiz/voxeliq)", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            defaultResourceLink.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(defaultResourceLink);

            var goodResourceTitle = new CCLabelTTF("Good Morning Pack", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            goodResourceTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(goodResourceTitle);

            var goodResourceLink = new CCLabelTTF("Voxeliq (https://github.com/raistlinthewiz/voxeliq)", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            defaultResourceLink.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(defaultResourceLink);

            var upscaledResourceTitle = new CCLabelTTF("Upscaled", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            upscaledResourceTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(upscaledResourceTitle);

            var upscaledResourceLink = new CCLabelTTF("Upscaled (http://www.curse.com/texture-packs/minecraft/upscaled)", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            upscaledResourceLink.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            this.AddChild(upscaledResourceLink);
            */
        }
    }
}