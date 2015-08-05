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


namespace Simsip.LineRunner.Scenes.Credits
{
    public class CreditsPage1Layer : GameLayer
    {
        private CoreScene _parent;
        private CreditsMasterLayer _masterLayer;

        public CreditsPage1Layer(CoreScene parent, CreditsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

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
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 1", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.Scale = GameConstants.FONT_SIZE_NORMAL;
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            var createdByText = string.Empty;
#if ANDROID
            createdByText = Program.SharedProgram.Resources.GetString(Resource.String.CreditsCreatedBy);
#elif IOS
            createdByText = NSBundle.MainBundle.LocalizedString(Strings.CreditsCreatedBy, Strings.CreditsCreatedBy);
#else
            createdByText = AppResources.CreditsCreatedBy;
#endif
            var createdTitle = new CCLabelTTF(createdByText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            createdTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            createdTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(createdTitle);

            var createdBySimsipText = string.Empty;
#if ANDROID
            createdBySimsipText = Program.SharedProgram.Resources.GetString(Resource.String.CreditsCreatedBySimsip);
#elif IOS
            createdBySimsipText = NSBundle.MainBundle.LocalizedString(Strings.CreditsCreatedBySimsip, Strings.CreditsCreatedBySimsip);
#else
            createdBySimsipText = AppResources.CreditsCreatedBySimsip;
#endif
            var createdDescription = new CCLabelTTF(createdBySimsipText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_X_LARGE);
            createdDescription.Scale = GameConstants.FONT_SIZE_X_LARGE_SCALE;
            createdDescription.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(createdDescription);
        }
    }
}