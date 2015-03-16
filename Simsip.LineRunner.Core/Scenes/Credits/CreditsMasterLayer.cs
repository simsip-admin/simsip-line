using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
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
    public class CreditsMasterLayer : GameLayer
    {
        private CoreScene _parent;

        // Pane and pane actions
        private PaneModel _paneModel;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Options pages
        private CreditsPage1Layer _creditsPage1Layer;
        private CreditsPage2Layer _creditsPage2Layer;
        private CreditsPage3Layer _creditsPage3Layer;

        // Page actions
        private CCAction _pageActionInFromLeft;
        private CCAction _pageActionInFromRight;
        private CCAction _pageActionOutToLeft;
        private CCAction _pageActionOutToRight;

        // Previous/next buttons
        private CCMenu _previousMenu;
        private CCMenu _nextMenu;

        // Track page we are on
        private int _currentPage;
        private int _totalPages;

        public CreditsMasterLayer(CoreScene parent)
        {
            this._parent = parent;

            // We want touches so we can handle selection of resource pack/pad images
            this.TouchEnabled = true;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.8f * screenSize.Width,
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
                LogicalHeight = this.ContentSize.Height
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

            // Page transition in/out
            var inFromLeftShow = new CCShow();
            var inFromLeftPlace = new CCPlace(new CCPoint(
                -screenSize.Width,
                0f));
            var inFromLeftMoveTo = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                0f,
                0f));
            this._pageActionInFromLeft = new CCSequence(new CCFiniteTimeAction[] { 
                inFromLeftShow, 
                inFromLeftPlace,
                inFromLeftMoveTo
            });
            var inFromRightShow = new CCShow();
            var inFromRightPlace = new CCPlace(new CCPoint(
                screenSize.Width,
                0f));
            var inFromRightMoveTo = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                0f,
                0f));
            this._pageActionInFromRight = new CCSequence(new CCFiniteTimeAction[] { 
                inFromRightShow, 
                inFromRightPlace,
                inFromRightMoveTo
            });
            var outToLeftMoveTo = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                -screenSize.Width,
                0f));
            var outToLeftHide = new CCHide();
            this._pageActionOutToLeft = new CCSequence(new CCFiniteTimeAction[] { 
                outToLeftMoveTo,
                outToLeftHide
            });
            var outToRightMoveTo = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                screenSize.Width,
                0f));
            var outToRightHide = new CCHide();
            this._pageActionOutToRight = new CCSequence(new CCFiniteTimeAction[] { 
                outToRightMoveTo,
                outToRightHide
            });

            // Credits title
            var creditsText = string.Empty;
#if ANDROID
            creditsText = Program.SharedProgram.Resources.GetString(Resource.String.CreditsTitle);
#elif IOS
            creditsText = NSBundle.MainBundle.LocalizedString(Strings.CreditsTitle, Strings.CreditsTitle);
#else
            creditsText = AppResources.CreditsTitle;
#endif
            var creditsTitle = new CCLabelTTF(creditsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            creditsTitle.AnchorPoint = CCPoint.AnchorMiddleLeft;
            creditsTitle.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(creditsTitle);

            // Header line
            var headerLineImage = new CCSprite("Images/Misc/HeaderLine");
            Cocos2DUtils.ResizeSprite(headerLineImage,
                0.9f * this.ContentSize.Width,
                0.01f * this.ContentSize.Height);
            headerLineImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.85f * this.ContentSize.Height);
            this.AddChild(headerLineImage);

            // Version
            var versionText = string.Empty;
#if ANDROID
            versionText = Program.SharedProgram.Resources.GetString(Resource.String.CommonVersion);
#elif IOS
            versionText = NSBundle.MainBundle.LocalizedString(Strings.CommonVersion, Strings.CommonVersion);
#else
            versionText = AppResources.CommonVersion;
#endif
            var versionHeader = new CCLabelTTF(versionText + " " + this.GetVersion(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            versionHeader.AnchorPoint = CCPoint.AnchorMiddleLeft;
            versionHeader.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(versionHeader);

            // Pages
            this._creditsPage1Layer = new CreditsPage1Layer(this._parent, this);
            this._creditsPage2Layer = new CreditsPage2Layer(this._parent, this);
            this._creditsPage3Layer = new CreditsPage3Layer(this._parent, this);
            this.AddChild(this._creditsPage1Layer);
            this.AddChild(this._creditsPage2Layer);
            this.AddChild(this._creditsPage3Layer);
            this._creditsPage2Layer.Visible = false;
            this._creditsPage3Layer.Visible = false;
            this._currentPage = 1;
            this._totalPages = 3;

            // Previous
            var previousNormal = new CCSprite("Images/Icons/PreviousButtonNormal.png");
            Cocos2DUtils.ResizeSprite(previousNormal,
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            var previousSelected = new CCSprite("Images/Icons/PreviousButtonSelected.png");
            Cocos2DUtils.ResizeSprite(previousSelected,
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            var previousButton = new CCMenuItemImage();
            previousButton.NormalImage = previousNormal;
            previousButton.SelectedImage = previousSelected;
            previousButton.SetTarget((obj) => { this.Previous(); });
            this._previousMenu = new CCMenu(
                new CCMenuItem[] 
                                {
                                    previousButton, 
                                });
            this._previousMenu.Position = new CCPoint(
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            this.AddChild(this._previousMenu);
            this._previousMenu.Visible = false;

            // Next
            var nextNormal = new CCSprite("Images/Icons/NextButtonNormal.png");
            Cocos2DUtils.ResizeSprite(nextNormal,
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            var nextSelected = new CCSprite("Images/Icons/NextButtonSelected.png");
            Cocos2DUtils.ResizeSprite(nextSelected,
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            var nextButton = new CCMenuItemImage();
            nextButton.NormalImage = nextNormal;
            nextButton.SelectedImage = nextSelected;
            nextButton.SetTarget((obj) => { this.Next(); });
            this._nextMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        nextButton, 
                    });
            this._nextMenu.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.1f  * this.ContentSize.Height);
            this.AddChild(this._nextMenu);
            this._nextMenu.Visible = true;

            // Back
            CCMenuItemImage backButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { _parent.GoBack(); });
            var backMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        backButton, 
                    });
            backMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            this.AddChild(backMenu);
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate pane/layer
            this._paneModel.ModelRunAction(this._paneActionIn);
            this.RunAction(this._layerActionIn);

            // Determine which navigation ui to show
            // Note: Page visibility is handled in previous/next event handlers below
            UpdatePageNavigationUI();
        }

        public override void Draw()
        {
            // Draw pane with Cocos2D view, projection and game state
            this._paneModel.DrawViaStationaryCamera();

            base.Draw();
        }

        #endregion

        #region Helper methods

        private void Previous()
        {
            // Update state
            this._currentPage--;
            if (this._currentPage <= 0)
            {
                this._currentPage = 1;
            }

            // Update page navigation ui
            this.UpdatePageNavigationUI();

            // Determine exiting/incoming layers
            CCLayer exitingLayer = null;
            CCLayer incomingLayer = null;
            switch (this._currentPage)
            {
                case 1:
                    {
                        exitingLayer = this._creditsPage2Layer;
                        incomingLayer = this._creditsPage1Layer;
                        break;
                    }
                case 2:
                    {
                        exitingLayer = this._creditsPage3Layer;
                        incomingLayer = this._creditsPage2Layer;
                        break;
                    }

            }

            // Animate page transition
            if (exitingLayer != null &&
                incomingLayer != null)
            {
                exitingLayer.RunAction(this._pageActionOutToRight);
                incomingLayer.RunAction(this._pageActionInFromLeft);
            }
        }

        private void Next()
        {
            // Update state
            this._currentPage++;
            if (this._currentPage > this._totalPages)
            {
                this._currentPage = this._totalPages;
            }

            // Update page navigation ui
            this.UpdatePageNavigationUI();

            // Determine exiting/incoming layers
            CCLayer exitingLayer = null;
            CCLayer incomingLayer = null;
            switch (this._currentPage)
            {
                case 2:
                    {
                        exitingLayer = this._creditsPage1Layer;
                        incomingLayer = this._creditsPage2Layer;
                        break;
                    }
                case 3:
                    {
                        exitingLayer = this._creditsPage3Layer;
                        incomingLayer = this._creditsPage3Layer;
                        break;
                    }
            }

            // Animate page transition
            if (exitingLayer != null &&
                incomingLayer != null)
            {
                exitingLayer.RunAction(this._pageActionOutToLeft);
                incomingLayer.RunAction(this._pageActionInFromRight);
            }
        }

        private void UpdatePageNavigationUI()
        {
            if (this._currentPage == 1)
            {
                this._previousMenu.Visible = false;
                this._nextMenu.Visible = true;

            }
            else if (this._currentPage == this._totalPages)
            {
                this._previousMenu.Visible = true;
                this._nextMenu.Visible = false;
            }
            else
            {
                this._previousMenu.Visible = true;
                this._nextMenu.Visible = true;
            }
        }


        private string GetVersion()
        {
            var versionStr = string.Empty;
            try
            {
#if ANDROID
                versionStr = Program.SharedProgram.PackageManager.GetPackageInfo(Program.SharedProgram.PackageName, 0).VersionName;
#elif IOS
                versionStr = NSBundle.MainBundle.InfoDictionary [new NSString ("CFBundleShortVersionString")].ToString ();
#elif NETFX_CORE
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                versionStr = string.Format("{0}.{1}.{2}.{3}",
                    version.Major, version.Minor, version.Build, version.Revision);
#elif WINDOWS_PHONE
                const string verLabel = "Version=";
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                var startIndex = assemblyName.IndexOf(verLabel) + verLabel.Length;
                var endIndex = assemblyName.IndexOf(',', startIndex + 1);
                versionStr = assemblyName.Substring(startIndex, endIndex - startIndex);
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception caught while attempting to get version number: " + ex);
            }

            return versionStr;
        }

        #endregion
    }
}