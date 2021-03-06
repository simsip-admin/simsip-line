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
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(0.8f * screenSize.Width,
                                          0.8f * screenSize.Height);
            // Layer transition in/out
            var layerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
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
            var startPageText = "start page";
            var startPageLabel = new CCLabelTTF(startPageText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startPageLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            startPageLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.8f * this.ContentSize.Height);
            this.AddChild(startPageLabel);
            this._startPageTextField = new CCTextFieldTTF(GameManager.SharedGameManager.GameStartPageNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startPageTextField.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            this._startPageTextField.Position = new CCPoint(
                0.9f * this.ContentSize.Width, 
                0.8f * this.ContentSize.Height);
            this._startPageTextField.AutoEdit = true;
            this._startPageTextField.EditTitle = startPageText;
            var startPageDescriptionText = "enter page to start on";
            this._startPageTextField.EditDescription = startPageDescriptionText;
            this._startPageTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._startPageTextField.ContentSize.Width,
                1f * this._startPageTextField.ContentSize.Height);
            AddChild(this._startPageTextField);

            // Start line
            var startLineText = "start line";
            var startLineLabel = new CCLabelTTF(startLineText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startLineLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            startLineLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(startLineLabel);
            this._startLineTextField = new CCTextFieldTTF(GameManager.SharedGameManager.GameStartLineNumber.ToString(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startLineTextField.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            this._startLineTextField.Position = new CCPoint(
                0.9f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this._startLineTextField.AutoEdit = true;
            this._startLineTextField.EditTitle = startLineText;

            var startLineDescriptionText = "enter line to start on (1-17)";
            this._startLineTextField.EditDescription = startLineDescriptionText;
            this._startLineTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._startLineTextField.ContentSize.Width,
                1f * this._startLineTextField.ContentSize.Height);
            AddChild(this._startLineTextField);

            // Are kills on?
            var killOnText = "kills on";
            var killsOnLabel = new CCLabelTTF(killOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            killsOnLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            killsOnLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
            var killsOnItem = new CCMenuItemLabel(killsOnLabel);
            var killOffText = "kills off";
            var killsOffLabel = new CCLabelTTF(killOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            killsOffLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            killsOffLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
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
            killMenu.AnchorPoint = CCPoint.AnchorMiddle;
            killMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.6f * this.ContentSize.Height);
            this.AddChild(killMenu);

            // Are particles on?
            var particlesOnText = "particles on";
            var particlesOnLabel = new CCLabelTTF(particlesOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            particlesOnLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            particlesOnLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
            var particlesOnItem = new CCMenuItemLabel(particlesOnLabel);
            var particlesOffText = "particles off";
            var particlesOffLabel = new CCLabelTTF(particlesOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            particlesOffLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            particlesOffLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
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
            particlesMenu.AnchorPoint = CCPoint.AnchorMiddle;
            particlesMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(particlesMenu);

            // Are upgrades on?
            var upgradesOnText = "upgrades on";
            var upgradesOnLabel = new CCLabelTTF(upgradesOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            upgradesOnLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            upgradesOnLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
            var upgradesOnItem = new CCMenuItemLabel(upgradesOnLabel);
            var upgradesOffText = "upgrades off";
            var upgradesOffLabel = new CCLabelTTF(upgradesOffText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            upgradesOffLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            upgradesOffLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
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
            upgradesMenu.AnchorPoint = CCPoint.AnchorMiddle;
            upgradesMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(upgradesMenu);

#if ANDROID || IOS
            // Refund
            var refundText = "refund";
            var refundLabel = new CCLabelTTF(refundText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            refundLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            refundLabel.ContentSize *= GameConstants.FONT_SIZE_NORMAL_SCALE;
            var refundItem = new CCMenuItemLabel(refundLabel,
                (obj) => { this.Refund(); });
            var refundMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        refundItem,
                    });
            refundMenu.AnchorPoint = CCPoint.AnchorMiddle;
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
            var backText = string.Empty;
#if ANDROID
            backText = Program.SharedProgram.Resources.GetString(Resource.String.CommonBack);
#elif IOS
            backText = NSBundle.MainBundle.LocalizedString(Strings.CommonBack, Strings.CommonBack);
#else
            backText = AppResources.CommonBack;
#endif
            var backLabel = new CCLabelTTF(backText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            backLabel.Scale = GameConstants.FONT_SIZE_SMALL_SCALE;
            backLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this.AddChild(backLabel);
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