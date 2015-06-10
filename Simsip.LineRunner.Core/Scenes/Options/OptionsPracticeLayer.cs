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
    public class OptionsPracticeLayer : GameLayer
    {
        private CoreScene _parent;
        private OptionsMasterLayer _masterLayer;

        // Start page/line
        private CCTextFieldTTF _startPageTextField;
        private CCTextFieldTTF _startLineTextField;
        
        public OptionsPracticeLayer(CoreScene parent, OptionsMasterLayer masterLayer)
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
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);


            // Start page
            var startPageText = string.Empty;
#if ANDROID
            startPageText = Program.SharedProgram.Resources.GetString(Resource.String.AdminStartPage);
#elif IOS
            startPageText = NSBundle.MainBundle.LocalizedString(Strings.AdminStartPage, Strings.AdminStartPage);
#else
            startPageText = AppResources.AdminStartPage;
#endif
            var startPageLabel = new CCLabelTTF(startPageText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startPageLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(startPageLabel);
            this._startPageTextField = new CCTextFieldTTF(GameManager.SharedGameManager.AdminStartPageNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startPageTextField.Position = new CCPoint(
                0.9f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this._startPageTextField.AutoEdit = true;
            this._startPageTextField.EditTitle = startPageText;
            var startPageDescriptionText = string.Empty;
#if ANDROID
            startPageDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.AdminStartPageDescription);
#elif IOS
            startPageDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.AdminStartPageDescription, Strings.AdminStartPageDescription);
#else
            startPageDescriptionText = AppResources.AdminStartPageDescription;
#endif
            this._startPageTextField.EditDescription = startPageDescriptionText;
            this._startPageTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._startPageTextField.ContentSize.Width,
                1f * this._startPageTextField.ContentSize.Height);
            AddChild(this._startPageTextField);

            // Start line
            var startLineText = string.Empty;
#if ANDROID
            startLineText = Program.SharedProgram.Resources.GetString(Resource.String.AdminStartLine);
#elif IOS
            startLineText = NSBundle.MainBundle.LocalizedString(Strings.AdminStartLine, Strings.AdminStartLine);
#else
            startLineText = AppResources.AdminStartLine;
#endif
            var startLineLabel = new CCLabelTTF(startLineText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startLineLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(startLineLabel);
            this._startLineTextField = new CCTextFieldTTF(GameManager.SharedGameManager.AdminStartLineNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startLineTextField.Position = new CCPoint(
                0.9f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this._startLineTextField.AutoEdit = true;
            this._startLineTextField.EditTitle = startLineText;

            var startLineDescriptionText = string.Empty;
#if ANDROID
            startLineDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.AdminStartLineDescription);
#elif IOS
            startLineDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.AdminStartLineDescription, Strings.AdminStartLineDescription);
#else
            startLineDescriptionText = AppResources.AdminStartLineDescription;
#endif
            this._startLineTextField.EditDescription = startLineDescriptionText;
            this._startLineTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._startLineTextField.ContentSize.Width,
                1f * this._startLineTextField.ContentSize.Height);
            AddChild(this._startLineTextField);

            // Are kills on?
            var killOnText = string.Empty;
#if ANDROID
            killOnText = Program.SharedProgram.Resources.GetString(Resource.String.AdminKillOn);
#elif IOS
            killOnText = NSBundle.MainBundle.LocalizedString(Strings.AdminKillOn, Strings.AdminKillOn);
#else
            killOnText = AppResources.AdminKillOn;
#endif
            var killsOnLabel = new CCLabelTTF(killOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var killsOnItem = new CCMenuItemLabel(killsOnLabel);
            var killOffText = string.Empty;
#if ANDROID
            killOffText = Program.SharedProgram.Resources.GetString(Resource.String.AdminKillOff);
#elif IOS
            killOffText = NSBundle.MainBundle.LocalizedString(Strings.AdminKillOff, Strings.AdminKillOff);
#else
            killOffText = AppResources.AdminKillOff;
#endif
            var killsOffLabel = new CCLabelTTF(killOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var killsOffItem = new CCMenuItemLabel(killsOffLabel);
            CCMenuItemToggle killToggle =
                new CCMenuItemToggle((obj) => KillTogglePressed(),
                new CCMenuItem[] { killsOnItem, killsOffItem });
            if (GameManager.SharedGameManager.AdminIsKillAllowed == false)
            {
                killToggle.SelectedIndex = 1; // Kills are OFF
            }
            var killMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        killToggle,
                    });
            killMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(killMenu);
        }

        private void KillTogglePressed()
        {
            if (GameManager.SharedGameManager.AdminIsKillAllowed)
            {
                GameManager.SharedGameManager.AdminIsKillAllowed = false;
            }
            else
            {
                GameManager.SharedGameManager.AdminIsKillAllowed = true;
            }
        }

    }
}