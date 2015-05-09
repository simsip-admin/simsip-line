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
    public class CreditsPage2Layer : GameLayer
    {
        private CoreScene _parent;
        private CreditsMasterLayer _masterLayer;

        public CreditsPage2Layer(CoreScene parent, CreditsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage2Of3);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage2Of3, Strings.CommonPage2Of3);
#else
            pageNumberText = AppResources.CommonPage2Of3;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            var frameworksText = string.Empty;
#if ANDROID
            frameworksText = Program.SharedProgram.Resources.GetString(Resource.String.CreditsFrameworks);
#elif IOS
            frameworksText = NSBundle.MainBundle.LocalizedString(Strings.CreditsFrameworks, Strings.CreditsFrameworks);
#else
            frameworksText = AppResources.CreditsFrameworks;
#endif
            var frameworksTitle = new CCLabelTTF(frameworksText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            frameworksTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(frameworksTitle);

            // Bepu
            var bepuTitle = new CCLabelTTF("BEPUphysics", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            bepuTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(bepuTitle);
            var bepuLicense = new CCLabelTTF("Licensed under Apache License 2.0 (Apache).", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            bepuLicense.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(bepuLicense);

            // Cocos2D-XNA
            var cocosTitle = new CCLabelTTF("Cocos2d-XNA", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            cocosTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(cocosTitle);
            var cocosLicense = new CCLabelTTF("Licensed under GNU Affero General 29 Public License.", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            cocosLicense.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(cocosLicense);

            // Json.net
            var jsonTitle = new CCLabelTTF("Json.NET", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            jsonTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(jsonTitle);
            var jsonLicense = new CCLabelTTF("Licensed under The MIT License (MIT).", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            jsonLicense.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            this.AddChild(jsonLicense);

            // Monogame
            var monogameTitle = new CCLabelTTF("Monogame", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            monogameTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(monogameTitle);
            var monogameLicense = new CCLabelTTF("Licensed under Microsoft Public License (Ms-PL).", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            monogameLicense.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.35f * this.ContentSize.Height);
            this.AddChild(monogameLicense);

            // Voxeliq
            var voxeliqTitle = new CCLabelTTF("Voxeliq", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            voxeliqTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(voxeliqTitle);
            var voxeliqLicense = new CCLabelTTF("Licensed under Microsoft Public License (MS-PL).", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            voxeliqLicense.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(voxeliqLicense);
        }
    }
}