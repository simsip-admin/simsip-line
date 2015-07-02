using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.Data.InApp;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if ANDROID
using Simsip.LineRunner.Services.Inapp;
using Xamarin.InAppBilling;
#endif
#if IOS
using Simsip.LineRunner.Services.Inapp;
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Options
{
    public class OptionsMasterLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Options pages
        private IList<CCLayer> _optionsPages;
        private OptionsPracticeLayer _practicePage;

        // Page actions
        private CCAction _pageActionInFromLeft;
        private CCAction _pageActionInFromRight;
        private CCAction _pageActionOutToLeft;
        private CCAction _pageActionOutToRight;

        // Page number header
        private string _pageNumberText;
        private CCLabelTTF _pageNumberHeader;

        // Previous/next buttons
        private CCMenu _previousMenu;
        private CCMenu _nextMenu;
        private CCLabelTTF _previousLabel;
        private CCLabelTTF _nextLabel;

        // Track page we are on
        private int _currentPage;
        private int _totalPages;

#if ANDROID || IOS
        // Services we'll need
        private IInappService _inAppService;
        private IInAppPurchaseRepository _inAppPurchaseRepository;
#endif

        public OptionsMasterLayer(CoreScene parent)
        {
            this._parent = parent;

#if ANDROID || IOS
            // Grab references to services we'll need
            this._inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService)); 
            this._inAppPurchaseRepository = new InAppPurchaseRepository();
#endif

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

            // Options title
            var optionsText = string.Empty;
#if ANDROID
            optionsText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsTitle);
#elif IOS
            optionsText = NSBundle.MainBundle.LocalizedString(Strings.OptionsTitle, Strings.OptionsTitle);
#else
            optionsText = AppResources.OptionsTitle;
#endif
            var optionsTitle = new CCLabelTTF(optionsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            optionsTitle.AnchorPoint = CCPoint.AnchorMiddleLeft;
            optionsTitle.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(optionsTitle);

            // Page number
            this._pageNumberText = string.Empty;
#if ANDROID
            this._pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage);
#elif IOS
            this._pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage, Strings.CommonPage);
#else
            this._pageNumberText = AppResources.CommonPage;
#endif
            this._pageNumberHeader = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            this._pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(this._pageNumberHeader);

            // Header line
            var headerLineImage = new CCSprite("Images/Misc/HeaderLine");
            Cocos2DUtils.ResizeSprite(headerLineImage,
                0.9f * this.ContentSize.Width,
                0.01f * this.ContentSize.Height);
            headerLineImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.85f * this.ContentSize.Height);
            this.AddChild(headerLineImage);

            // Pages
            this._optionsPages = new List<CCLayer>();
            this._optionsPages.Add(new OptionsPage1Layer(this._parent, this));
            this._optionsPages.Add(new OptionsPage2Layer(this._parent, this));
            this._optionsPages.Add(new OptionsPage3Layer(this._parent, this));
            foreach (var page in this._optionsPages)
            {
                this.AddChild(page);
                page.Visible = false;
            }
            this._optionsPages[0].Visible = true;
            this._currentPage = 1;
            this._totalPages = this._optionsPages.Count;

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
                0.95f  * this.ContentSize.Width,
                0.02f  * this.ContentSize.Height);
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
                0.5f  * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this.AddChild(backLabel);
        }

        #region Properties
        
        public int UpgradeCount { get; private set; }
        
        #endregion

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate layer
            this.RunAction(this._layerActionIn);

            // Reset state
            foreach(var optionPage in this._optionsPages)
            {
                optionPage.Visible = false;
            }

#if ANDROID || IOS
            // Determine if upgrade practice mode was purchased
            // Note: Override available from admin screen
            var practicePurchase =
                this._inAppPurchaseRepository.GetPurchaseByProductId(this._inAppService.PracticeModeProductId);
            if (practicePurchase != null ||
                GameManager.SharedGameManager.AdminAreUpgradesAllowed)
            {
                // OK, we have the purchase, do we need to insert into our pages?
                if (this._practicePage == null)
                {
                    this._practicePage = new OptionsPracticeLayer(this._parent, this);
                    this.AddChild(this._practicePage);
                    this._optionsPages.Insert(0, this._practicePage);
                    this._totalPages = this._optionsPages.Count;
                }
            }
#endif

            // Flip on the last options page that was active
            // before leaving options and make sure it is 
            // repositioned correctly
            this._optionsPages[this._currentPage-1].Visible = true;
            this._optionsPages[this._currentPage-1].Position = new CCPoint(
                0f,
                0f);

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
            var exitingLayer = this._optionsPages[this._currentPage];
            var incomingLayer = this._optionsPages[this._currentPage - 1];

            // Animate page transition
            exitingLayer.RunAction(this._pageActionOutToRight);
            incomingLayer.RunAction(this._pageActionInFromLeft);
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
            var exitingLayer = this._optionsPages[this._currentPage - 2];
            var incomingLayer = this._optionsPages[this._currentPage - 1];

            // Animate page transition
            exitingLayer.RunAction(this._pageActionOutToLeft);
            incomingLayer.RunAction(this._pageActionInFromRight);
        }

        private void UpdatePageNavigationUI()
        {
            // Page number
            this._pageNumberHeader.Text = this._pageNumberText + " " + this._currentPage;

            // Previous/next arrows
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