using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Linq;
#if ANDROID
using Simsip.LineRunner.Services.Inapp;
#endif
#if IOS
using Simsip.LineRunner.Services.Inapp;
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Admin
{
    public class AdminLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Start page/line
        private CCTextFieldTTF _startPageTextField;
        private CCTextFieldTTF _startLineTextField;

#if ANDROID || IOS
        // Services we'll need
        private IInappService _inAppService;
#endif

        public AdminLayer(CoreScene parent)
        {
            this._parent = parent;

#if ANDROID || IOS
            // Services we'll need
            this._inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
#endif

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(0.8f * screenSize.Width,
                                          0.8f * screenSize.Height);
            // Layer transition in/out
            var layerEndPosition = new CCPoint(
                    0.1f * screenSize.Width,
                    0.1f * screenSize.Height);
            var layerStartPosition = new CCPoint(
                layerEndPosition.X,
                screenSize.Height);
            var layerStartPlacementAction = new CCPlace(layerStartPosition);
            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
            this._layerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
            );
            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
            var layerNavigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
            this._layerActionOut = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, layerNavigateAction })
            );

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
            this._startPageTextField = new CCTextFieldTTF(GameManager.SharedGameManager.GameStartPageNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
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
            this._startLineTextField = new CCTextFieldTTF(GameManager.SharedGameManager.GameStartLineNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
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
            killOnText = Program.SharedProgram.Resources.GetString(Resource.String.AdminKillsOn);
#elif IOS
            killOnText = NSBundle.MainBundle.LocalizedString(Strings.AdminKillsOn, Strings.AdminKillsOn);
#else
            killOnText = AppResources.AdminKillsOn;
#endif
            var killsOnLabel = new CCLabelTTF(killOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var killsOnItem = new CCMenuItemLabel(killsOnLabel);
            var killOffText = string.Empty;
#if ANDROID
            killOffText = Program.SharedProgram.Resources.GetString(Resource.String.AdminKillsOff);
#elif IOS
            killOffText = NSBundle.MainBundle.LocalizedString(Strings.AdminKillsOff, Strings.AdminKillsOff);
#else
            killOffText = AppResources.AdminKillsOff;
#endif
            var killsOffLabel = new CCLabelTTF(killOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var killsOffItem = new CCMenuItemLabel(killsOffLabel);
            CCMenuItemToggle killToggle =
                new CCMenuItemToggle((obj) => KillsTogglePressed(),
                new CCMenuItem[] { killsOnItem, killsOffItem });
            if (GameManager.SharedGameManager.GameAreKillsAllowed == false)
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

            // Are particles on?
            var particlesOnText = string.Empty;
#if ANDROID
            particlesOnText = Program.SharedProgram.Resources.GetString(Resource.String.AdminParticlesOn);
#elif IOS
            particlesOnText = NSBundle.MainBundle.LocalizedString(Strings.AdminParticlesOn, Strings.AdminParticlesOn);
#else
            particlesOnText = AppResources.AdminParticlesOn;
#endif
            var particlesOnLabel = new CCLabelTTF(particlesOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var particlesOnItem = new CCMenuItemLabel(particlesOnLabel);
            var particlesOffText = string.Empty;
#if ANDROID
            particlesOffText = Program.SharedProgram.Resources.GetString(Resource.String.AdminParticlesOff);
#elif IOS
            particlesOffText = NSBundle.MainBundle.LocalizedString(Strings.AdminParticlesOff, Strings.AdminParticlesOff);
#else
            particlesOffText = AppResources.AdminParticlesOff;
#endif

            var particlesOffLabel = new CCLabelTTF(particlesOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var particlesOffItem = new CCMenuItemLabel(particlesOffLabel);
            CCMenuItemToggle particlesToggle =
                new CCMenuItemToggle((obj) => ParticlesTogglePressed(),
                new CCMenuItem[] { particlesOnItem, particlesOffItem });
            if (GameManager.SharedGameManager.AdminAreParticlesAllowed == false)
            {
                particlesToggle.SelectedIndex = 1; // Particles are OFF
            }
            var particlesMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        particlesToggle,
                    });
            particlesMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(particlesMenu);

            // Are upgrades on?
            var upgradesOnText = string.Empty;
#if ANDROID
            upgradesOnText = Program.SharedProgram.Resources.GetString(Resource.String.AdminUpgradesOn);
#elif IOS
            upgradesOnText = NSBundle.MainBundle.LocalizedString(Strings.AdminUpgradesOn, Strings.AdminUpgradesOn);
#else
            upgradesOnText = AppResources.AdminUpgradesOn;
#endif
            var upgradesOnLabel = new CCLabelTTF(upgradesOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var upgradesOnItem = new CCMenuItemLabel(upgradesOnLabel);
            var upgradesOffText = string.Empty;
#if ANDROID
            upgradesOffText = Program.SharedProgram.Resources.GetString(Resource.String.AdminUpgradesOff);
#elif IOS
            upgradesOffText = NSBundle.MainBundle.LocalizedString(Strings.AdminUpgradesOff, Strings.AdminUpgradesOff);
#else
            upgradesOffText = AppResources.AdminUpgradesOff;
#endif
            var upgradesOffLabel = new CCLabelTTF(upgradesOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var upgradesOffItem = new CCMenuItemLabel(upgradesOffLabel);
            CCMenuItemToggle upgradesToggle =
                new CCMenuItemToggle((obj) => UpgradesTogglePressed((obj as CCMenuItemToggle).SelectedIndex),
                new CCMenuItem[] { upgradesOnItem, upgradesOffItem });
            if (GameManager.SharedGameManager.AdminAreUpgradesAllowed == false)
            {
                upgradesToggle.SelectedIndex = 1; // Upgrades are OFF
            }
            var upgradesMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        upgradesToggle,
                    });
            upgradesMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(upgradesMenu);

#if ANDROID || IOS
            // Refund
            var refundText = "refund";
            var refundLabel = new CCLabelTTF(refundText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var refundItem = new CCMenuItemLabel(refundLabel,
                (obj) => { this.Refund(); });
            var refundMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        refundItem,
                    });
            refundMenu.Position = new CCPoint(
                 0.5f * this.ContentSize.Width,
                 0.3f * this.ContentSize.Height);
            this.AddChild(refundMenu);
#endif

            // Back
            CCMenuItemImage backButton =
                new CCMenuItemImage(
                    "Images/Icons/BackButtonNormal.png",
                    "Images/Icons/BackButtonSelected.png",
                    (obj) => { this.GoBack(); });
            var backMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        backButton, 
                    });
            backMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.2f * this.ContentSize.Height);
            this.AddChild(backMenu);
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate layer
            this.RunAction(this._layerActionIn);

            // Allow admins to set page/line we will start on
            this._startPageTextField.Text = GameManager.SharedGameManager.GameStartPageNumber.ToString();
            this._startLineTextField.Text = GameManager.SharedGameManager.GameStartLineNumber.ToString();
        }

        #endregion

        #region Helper methods

        private void KillsTogglePressed()
        {
            if (GameManager.SharedGameManager.GameAreKillsAllowed)
            {
                GameManager.SharedGameManager.GameAreKillsAllowed = false;
            }
            else
            {
                GameManager.SharedGameManager.GameAreKillsAllowed = true;
            }
        }

        private void ParticlesTogglePressed()
        {
            if (GameManager.SharedGameManager.AdminAreParticlesAllowed)
            {
                GameManager.SharedGameManager.AdminAreParticlesAllowed = false;
            }
            else
            {
                GameManager.SharedGameManager.AdminAreParticlesAllowed = true;
            }
        }

        private void UpgradesTogglePressed(int selectedIndex)
        {
            if (selectedIndex == 0)
            {
                GameManager.SharedGameManager.AdminAreUpgradesAllowed = true;
            }
            else
            {
                GameManager.SharedGameManager.AdminAreUpgradesAllowed = false;
            }
        }

#if ANDROID || IOS
        private void Refund()
        {
            this._inAppService.RefundProduct();
        }
#endif

        private void GoBack()
        {
            // Will animate our exit
            // IMPORTANT: There is a CCCallFunc at end of animation that will call
            // GoBack() on parent.
            this.RunAction(this._layerActionOut);

            // Will flip this in the tests below in case we need a refresh
            bool signalForRefresh = false;

            // Did we change the start page?
            var startPage = Int32.Parse(this._startPageTextField.Text);
            if (startPage != GameManager.SharedGameManager.GameStartPageNumber)
            {
                // Ok, first update our global state for the start page
                GameManager.SharedGameManager.GameStartPageNumber = startPage;

                signalForRefresh = true;
            }

            // Did we change the start line?
            var startLine = Int32.Parse(this._startLineTextField.Text);
            if (startLine != GameManager.SharedGameManager.GameStartLineNumber)
            {
                // Ok, first update our global state for the start page
                GameManager.SharedGameManager.GameStartLineNumber = startLine;

                signalForRefresh = true;
            }

            // If we determined we need a refresh above, kick off a full refresh to load up the new start page/line
            if (signalForRefresh)
            {
                this._parent.Refresh();
            }
        }

        #endregion
    }
}