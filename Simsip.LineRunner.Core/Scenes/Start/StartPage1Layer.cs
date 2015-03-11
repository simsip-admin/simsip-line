using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Start
{
    public class StartPage1Layer : GameLayer
    {
        private CoreScene _parent;

        // Pane
        private PaneModel _paneModel;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;
        
        // Layer actions
        private CCAction _layerActionIn;
        private CCFiniteTimeAction _layerActionOut;

        // Menu
        private CCMenuItemImage _startButton;
        private CCMenu _startMenu;

        // Allows for controling holding off from starting game from finish screen
        // when navigating quicker than move to start flyby
        private bool _isStartEnabled;

        public StartPage1Layer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.9f * screenSize.Width,
                0.2f * screenSize.Height);
            
            // Pane model
            var paneLogicalOrigin = new CCPoint(
                0.05f  * screenSize.Width,
                0.05f * screenSize.Height);
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = paneLogicalOrigin,
                LogicalWidth = this.ContentSize.Width,
                LogicalHeight = this.ContentSize.Height
            };
            this._paneModel = new PaneModel(paneModelArgs);

            // Pane transition in/out
            var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            var layerStartPosition = new CCPoint(
                 paneLogicalOrigin.X,
                -this.ContentSize.Height);
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
                new Sequence(new FiniteTimeAction[] { paneStartPlacementAction, paneMoveInAction }));
            var paneMoveOutAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneStartPosition);
            this._paneActionOut = new EaseBackIn(paneMoveOutAction);

            // Layer transition in/out
            var layerStartPlacementAction = new CCPlace(layerStartPosition);
            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
            this._layerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction }));
            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
            this._layerActionOut = layerMoveOutAction; 

            // Start menu
            this._isStartEnabled = true;
            this._startButton =
                new CCMenuItemImage("Images/Icons/StartButtonNormal.png",
                                    "Images/Icons/StartButtonSelected.png",
                                    (obj) => { this.NavigateStartPage2(); });
            var ratingsButton =
                new CCMenuItemImage("Images/Icons/RatingsButtonNormal.png",
                                    "Images/Icons/RatingsButtonSelected.png",
                                    (obj) => { this.Rate(); });
            var optionsButton =
                new CCMenuItemImage("Images/Icons/OptionsButtonNormal.png",
                                    "Images/Icons/OptionsButtonSelected.png",
                                     (obj) => { this.NavigateBase(LayerTags.OptionsMasterLayer); });
            var helpButton =
                new CCMenuItemImage("Images/Icons/HelpButtonNormal.png",
                                    "Images/Icons/HelpButtonSelected.png",
                                    (obj) => { this.NavigateBase(LayerTags.HelpMasterLayer); });
            this._startMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._startButton,
                        ratingsButton,
                        optionsButton,
                        helpButton
                    });
            this._startMenu.AlignItemsHorizontallyWithPadding(
                0.1f * this.ContentSize.Width);
            this._startMenu.AnchorPoint = CCPoint.AnchorMiddle;
            this._startMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(this._startMenu);

#if DEBUG
            var adminText = string.Empty;
#if ANDROID
            adminText = Program.SharedProgram.Resources.GetString(Resource.String.StartAdmin);
#elif IOS
            adminText = NSBundle.MainBundle.LocalizedString(Strings.StartAdmin, Strings.StartAdmin);
#else
            adminText = AppResources.StartAdmin;
#endif
            var adminLabel = new CCLabelTTF(adminText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var adminItem = new CCMenuItemLabel(adminLabel,
                (obj) => { this._parent.Navigate(LayerTags.AdminLayer); });
            var adminLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        adminItem
                    });
            adminLabelMenu.Position = new CCPoint(
                 0.5f * this.ContentSize.Width, 
                -0.1f * this.ContentSize.Height );
            this.AddChild(adminLabelMenu);
#endif
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate pane/layer
            this._paneModel.ModelRunAction(this._paneActionIn);
            this.RunAction(this._layerActionIn);

            // If we meet the ratings prompt criteria, display ratings prompt
            CheckForRatingsPrompt();
        }

        public override void Draw()
        {
            // Draw pane with Cocos2D view, projection and game state
            this._paneModel.DrawViaStationaryCamera();

            base.Draw();
        }

        #endregion

        #region Api

        public void EnableStart(bool enableStart)
        {
            // Set state
            this._isStartEnabled = enableStart;

            // Update ui
            if (this._isStartEnabled)
            {
                this._startButton.Color = new CCColor3B(Color.White);
            }
            else
            {
                this._startButton.Color = new CCColor3B(Color.Gray);
            }
        }

        #endregion

        #region Helper methods

        private void Rate()
        {
#if NETFX_CORE
            App.RateApp();
#elif DESKTOP
            Program.RateApp();
#else
           Program.SharedProgram.RateApp();
#endif
        }

        private void NavigateStartPage2()
        {
            if (this._isStartEnabled)
            {
                this.NavigateBase(LayerTags.StartPage2Layer);
            }
        }

        private void NavigateBase(LayerTags layer)
        {
            // Setup navigation to occur after we animate layer out
            var navigateAction = new CCCallFunc(() => { this._parent.Navigate(layer); });
            var layerMoveOutAction = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { this._layerActionOut, navigateAction } )
                );

            // Ok, now run pane/layer actions out
            this._paneModel.ModelRunAction(this._paneActionOut);
            this.RunAction(layerMoveOutAction);
        }

        private void CheckForRatingsPrompt()
        {
            var installDate = UserDefaults.SharedUserDefault.GetDateForKey(GameConstants.USER_DEFAULT_KEY_INSTALL_DATE, DateTime.Now);
            var gamesPlayedCount = UserDefaults.SharedUserDefault.GetIntegerForKey(GameConstants.USER_DEFAULT_KEY_GAMES_PLAYED_COUNT, 1);
            var askForRating = UserDefaults.SharedUserDefault.GetBoolForKey(GameConstants.USER_DEFAULT_KEY_ASK_FOR_RATING, true);

            var installedDuration = DateTime.UtcNow - installDate;
            if (installedDuration.TotalDays > GameConstants.RATING_WINDOW_IN_DAYS &&
                gamesPlayedCount > 9 &&
                askForRating)
            {
                this._parent.Navigate(LayerTags.RatingsLayer);
            }

        }

        #endregion
    }
}