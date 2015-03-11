using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Linq;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Admin
{
    public class AdminLayer : GameLayer
    {
        private CoreScene _parent;

        // Services we'll need
        private IPaneCache _paneCache;

        // Pane and pane actions
        private PaneModel _paneModel;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Start page
        private KeyboardNotificationLayer _startPageNotificationLayer;
        private CCTextFieldTTF _startPageTextField;

        // Start line
        private KeyboardNotificationLayer _startLineNotificationLayer;
        private CCTextFieldTTF _startLineTextField;

        public AdminLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(0.8f * screenSize.Width,
                                          0.8f * screenSize.Height);
            // Pane model
            var paneLogicalOrigin = new CCPoint(
                    0.1f * screenSize.Width, 
                    0.1f * screenSize.Height);
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = paneLogicalOrigin,
                LogicalWidth = this.ContentSize.Width,
                LogicalHeight = this.ContentSize.Height,
            };
            this._paneModel = new PaneModel(paneModelArgs);

            // Pane transition in/out
            var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            var layerStartPosition = new CCPoint(
                paneLogicalOrigin.X, 
                screenSize.Height);
            var layerEndPosition = paneLogicalOrigin;
            var paneStartPosition = XNAUtils.LogicalToWorld(
                layerStartPosition,
                pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var paneEndPosition = XNAUtils.LogicalToWorld(
                layerEndPosition,
                pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var paneStartPlacementAction = new Place(paneStartPosition);
            var paneMoveInAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneEndPosition);
            this._paneActionIn = new EaseBackOut(
                new Sequence(new FiniteTimeAction[] { paneStartPlacementAction, paneMoveInAction })
            );
            var paneMoveOutAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneStartPosition);
            this._paneActionOut = new EaseBackIn(paneMoveOutAction);

            // Layer transition in/out
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
            this._startPageNotificationLayer = new KeyboardNotificationLayer(this._startPageTextField);
            AddChild(this._startPageNotificationLayer);

            // Start line
            var startLineText = "Starting line:";
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
            var startLineDescriptionText = "Enter line to start on (1-17).";
            this._startLineTextField.EditDescription = startLineDescriptionText;
            this._startLineTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._startLineTextField.ContentSize.Width,
                1f * this._startLineTextField.ContentSize.Height);
            this._startLineNotificationLayer = new KeyboardNotificationLayer(this._startLineTextField);
            AddChild(this._startLineNotificationLayer);

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
            var particlesOffItem = new CCMenuItemLabel(killsOffLabel);
            CCMenuItemToggle particlesToggle =
                new CCMenuItemToggle((obj) => ParticlesTogglePressed(),
                new CCMenuItem[] { particlesOnItem, particlesOffItem });
            if (GameManager.SharedGameManager.AdminIsParticlesAllowed == false)
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
                0.4f * this.ContentSize.Height);
            this.AddChild(killMenu);

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

            // Animate pane/layer
            this._paneModel.ModelRunAction(this._paneActionIn);
            this.RunAction(this._layerActionIn);

            // Allow admins to set page/line we will start on
            this._startPageTextField.Text = GameManager.SharedGameManager.AdminStartPageNumber.ToString();
            this._startLineTextField.Text = GameManager.SharedGameManager.AdminStartLineNumber.ToString();
        }

        #endregion

        #region Helper methods

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

        private void ParticlesTogglePressed()
        {
            if (GameManager.SharedGameManager.AdminIsParticlesAllowed)
            {
                GameManager.SharedGameManager.AdminIsParticlesAllowed = false;
            }
            else
            {
                GameManager.SharedGameManager.AdminIsParticlesAllowed = true;
            }
        }

        private void GoBack()
        {
            // Will animate our exit
            // IMPORTANT: There is a CCCallFunc at end that will deregister pane model from cache
            // and navigate us back using GoBack()
            this._paneModel.ModelRunAction(this._paneActionOut);
            this.RunAction(this._layerActionOut);

            // Will flip this in the tests below in case we need a refresh
            bool signalForRefresh = false;

            // Did we change the start page?
            var startPage = Int32.Parse(this._startPageTextField.Text);
            if (startPage != GameManager.SharedGameManager.AdminStartPageNumber)
            {
                // Ok, first update our global state for the start page
                GameManager.SharedGameManager.AdminStartPageNumber = startPage;

                // Then update what we should have as a starting score for this page
                var pageObstaclesRepository = new PageObstaclesRepository();
                var obstacles = pageObstaclesRepository.GetObstacles();
                var startScore = obstacles
                    .Where(x => x.IsGoal == true &&
                                x.PageNumber < startPage)
                    .Count();
                GameManager.SharedGameManager.AdminStartScore = startScore;

                signalForRefresh = true;
            }

            // Did we change the start line?
            var startLine = Int32.Parse(this._startLineTextField.Text);
            if (startLine != GameManager.SharedGameManager.AdminStartLineNumber)
            {
                // Ok, first update our global state for the start page
                GameManager.SharedGameManager.AdminStartLineNumber = startLine;

                // Then update what we should have as a starting score for this page
                var pageObstaclesRepository = new PageObstaclesRepository();
                var obstacles = pageObstaclesRepository.GetObstacles();
                var startScore = obstacles
                    .Where(x => x.IsGoal == true &&
                                x.PageNumber < (startPage + 1) &&
                                x.LineNumber < startLine)
                    .Count();
                GameManager.SharedGameManager.AdminStartScore = startScore;

                signalForRefresh = true;
            }

            // If we determined we need a refresh above, kick off a full refresh to load up the new start page/line
            if (signalForRefresh)
            {
                this._parent.Refresh();
            }
        }

        public override void Draw()
        {
            // Draw pane with Cocos2D view, projection and game state
            this._paneModel.DrawViaStationaryCamera();

            base.Draw();
        }

        #endregion
    }
}