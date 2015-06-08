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


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpMasterLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Help pages
        private HelpPage1Layer _helpPage1Layer;
        private HelpPage2Layer _helpPage2Layer;
        private HelpPage3Layer _helpPage3Layer;
        private HelpPage4Layer _helpPage4Layer;
        private HelpPage5Layer _helpPage5Layer;
        private HelpPage6Layer _helpPage6Layer;
        private HelpPage7Layer _helpPage7Layer;
        private HelpPage8Layer _helpPage8Layer;

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

        public HelpMasterLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.8f * screenSize.Width,
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

            // Help title
            var helpText = string.Empty;
#if ANDROID
            helpText = Program.SharedProgram.Resources.GetString(Resource.String.HelpTitle);
#elif IOS
            helpText = NSBundle.MainBundle.LocalizedString(Strings.HelpTitle, Strings.HelpTitle);
#else
            helpText = AppResources.HelpTitle;
#endif
            var helpTitle = new CCLabelTTF(helpText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            helpTitle.AnchorPoint = CCPoint.AnchorMiddleLeft;
            helpTitle.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.9f  * this.ContentSize.Height);
            this.AddChild(helpTitle);

            // Header line
            var headerLineImage = new CCSprite("Images/Misc/HeaderLine");
            Cocos2DUtils.ResizeSprite(headerLineImage,
                0.9f * this.ContentSize.Width,
                0.01f * this.ContentSize.Height);
            headerLineImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
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
                0.8f  * this.ContentSize.Height);
            this.AddChild(versionHeader);

            // 1 page create up front, rest created on-demand, see GetHelpPage() below
            this._helpPage1Layer = new HelpPage1Layer(this._parent, this);
            this.AddChild(this._helpPage1Layer);
            this._currentPage = 1;
            this._totalPages = 8;

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

            // Animate layer
            this.RunAction(this._layerActionIn);

            // Determine which navigation ui to show
            // Note: Page visibility is handled in previous/next event handlers below
            UpdatePageNavigationUI();
        }

        #endregion

        #region Helper methods

        private GameLayer GetHelpPage(int pageNumber)
        {
            GameLayer returnHelpPage = null;

            switch (pageNumber)
                {
                    case 1:
                    {
                        if (this._helpPage1Layer == null)
                        {
                            this._helpPage1Layer = new HelpPage1Layer(this._parent, this);
                            this.AddChild(this._helpPage1Layer);
                        }
                        returnHelpPage = this._helpPage1Layer;

                        break;
                    }
                    case 2:
                    {
                        if (this._helpPage2Layer == null)
                        {
                            this._helpPage2Layer = new HelpPage2Layer(this._parent, this);
                            this.AddChild(this._helpPage2Layer);
                        }
                        returnHelpPage = this._helpPage2Layer;

                        break;
                    }
                    case 3:
                    {
                        if (this._helpPage3Layer == null)
                        {
                            this._helpPage3Layer = new HelpPage3Layer(this._parent, this);
                            this.AddChild(this._helpPage3Layer);
                        }
                        returnHelpPage = this._helpPage3Layer;

                        break;
                    }
                    case 4:
                    {
                        if (this._helpPage4Layer == null)
                        {
                            this._helpPage4Layer = new HelpPage4Layer(this._parent, this);
                            this.AddChild(this._helpPage4Layer);
                        }
                        returnHelpPage = this._helpPage4Layer;

                        break;
                    }
                    case 5:
                    {
                        if (this._helpPage5Layer == null)
                        {
                            this._helpPage5Layer = new HelpPage5Layer(this._parent, this);
                            this.AddChild(this._helpPage5Layer);
                        }
                        returnHelpPage = this._helpPage5Layer;

                        break;
                    }
                    case 6:
                    {
                        if (this._helpPage6Layer == null)
                        {
                            this._helpPage6Layer = new HelpPage6Layer(this._parent, this);
                            this.AddChild(this._helpPage6Layer);
                        }
                        returnHelpPage = this._helpPage6Layer;

                        break;
                    }
                    case 7:
                    {
                        if (this._helpPage7Layer == null)
                        {
                            this._helpPage7Layer = new HelpPage7Layer(this._parent, this);
                            this.AddChild(this._helpPage7Layer);
                        }
                        returnHelpPage = this._helpPage7Layer;

                        break;
                    }
                    case 8:
                    {
                        if (this._helpPage8Layer == null)
                        {
                            this._helpPage8Layer = new HelpPage8Layer(this._parent, this);
                            this.AddChild(this._helpPage8Layer);
                        }
                        returnHelpPage = this._helpPage8Layer;

                        break;
                    }
                }

                return returnHelpPage;
        }

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
            switch(this._currentPage)
            {
                case 1:
                    {
                        exitingLayer = this.GetHelpPage(2);
                        incomingLayer = this.GetHelpPage(1);
                        break;
                    }
                case 2:
                    {
                        exitingLayer = this.GetHelpPage(3);
                        incomingLayer = this.GetHelpPage(2);
                        break;
                    }
                case 3:
                    {
                        exitingLayer = this.GetHelpPage(4);
                        incomingLayer = this.GetHelpPage(3);
                        break;
                    }
                case 4:
                    {
                        exitingLayer = this.GetHelpPage(5);
                        incomingLayer = this.GetHelpPage(4);
                        break;
                    }
                case 5:
                    {
                        exitingLayer = this.GetHelpPage(6);
                        incomingLayer = this.GetHelpPage(5);
                        break;
                    }
                case 6:
                    {
                        exitingLayer = this.GetHelpPage(7);
                        incomingLayer = this.GetHelpPage(6);
                        break;
                    }
                case 7:
                    {
                        exitingLayer = this.GetHelpPage(8);
                        incomingLayer = this.GetHelpPage(7);
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
                        exitingLayer = this.GetHelpPage(1);
                        incomingLayer = this.GetHelpPage(2);
                        break;
                    }
                case 3:
                    {
                        exitingLayer = this.GetHelpPage(2);
                        incomingLayer = this.GetHelpPage(3);
                        break;
                    }
                case 4:
                    {
                        exitingLayer = this.GetHelpPage(3);
                        incomingLayer = this.GetHelpPage(4);
                        break;
                    }
                case 5:
                    {
                        exitingLayer = this.GetHelpPage(4);
                        incomingLayer = this.GetHelpPage(5);
                        break;
                    }
                case 6:
                    {
                        exitingLayer = this.GetHelpPage(5);
                        incomingLayer = this.GetHelpPage(6);
                        break;
                    }
                case 7:
                    {
                        exitingLayer = this.GetHelpPage(6);
                        incomingLayer = this.GetHelpPage(7);
                        break;
                    }
                case 8:
                    {
                        exitingLayer = this.GetHelpPage(7);
                        incomingLayer = this.GetHelpPage(8);
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

        #endregion
    }
}