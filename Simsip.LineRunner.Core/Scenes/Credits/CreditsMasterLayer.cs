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
    public class CreditsMasterLayer : UILayer
    {
        private CoreScene _parent;

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
        private CCLabelTTF _previousLabel;
        private CCMenu _nextMenu;
        private CCLabelTTF _nextLabel;

        // Track page we are on
        private int _currentPage;
        private int _totalPages;

        public CreditsMasterLayer(CoreScene parent)
        {
            this._parent = parent;

            // We want touches so we can handle selection of resource pack/pad images
            this.TouchEnabled = true;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                0.9f * screenSize.Width,
                0.9f * screenSize.Height);


            // Layer transition in/out
            var layerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.05f * screenSize.Width,
                0.05f * screenSize.Height);
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
            creditsTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
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
            var versionHeader = new CCLabelTTF(versionText + "-" + FileUtils.GetVersion(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
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
            var previousText = string.Empty;
#if ANDROID
            previousText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPrevious);
#elif IOS
            previousText = NSBundle.MainBundle.LocalizedString(Strings.CommonPrevious, Strings.CommonPrevious);
#else
            previousText = AppResources.CommonPrevious;
#endif
            this._previousLabel = new CCLabelTTF(previousText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            this._previousLabel.AnchorPoint = CCPoint.AnchorMiddleLeft;
            this._previousLabel.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this.AddChild(this._previousLabel);
            this._previousLabel.Visible = false;

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
            var nextText = string.Empty;
#if ANDROID
            nextText = Program.SharedProgram.Resources.GetString(Resource.String.CommonNext);
#elif IOS
            nextText = NSBundle.MainBundle.LocalizedString(Strings.CommonNext, Strings.CommonNext);
#else
            nextText = AppResources.CommonNext;
#endif
            this._nextLabel = new CCLabelTTF(nextText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            this._nextLabel.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this._nextLabel.AnchorPoint = CCPoint.AnchorMiddleRight;
            this.AddChild(this._nextLabel);
            this._nextLabel.Visible = true;

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
            var backText = string.Empty;
#if ANDROID
            backText = Program.SharedProgram.Resources.GetString(Resource.String.CommonBack);
#elif IOS
            backText = NSBundle.MainBundle.LocalizedString(Strings.CommonBack, Strings.CommonBack);
#else
            backText = AppResources.CommonBack;
#endif
            var backLabel = new CCLabelTTF(backText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
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

            // Determine which navigation ui to show
            // Note: Page visibility is handled in previous/next event handlers below
            UpdatePageNavigationUI();
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
                        exitingLayer = this._creditsPage2Layer;
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
                this._previousLabel.Visible = false;
                this._nextMenu.Visible = true;
                this._nextLabel.Visible = true;

            }
            else if (this._currentPage == this._totalPages)
            {
                this._previousMenu.Visible = true;
                this._previousLabel.Visible = true;
                this._nextMenu.Visible = false;
                this._nextLabel.Visible = false;
            }
            else
            {
                this._previousMenu.Visible = true;
                this._previousLabel.Visible = true;
                this._nextMenu.Visible = true;
                this._nextLabel.Visible = true;
            }
        }

        #endregion
    }
}