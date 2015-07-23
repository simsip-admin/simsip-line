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
        private HelpPage9Layer _helpPage9Layer;

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
        private CCLabelTTF _previousLabel;
        private CCMenu _nextMenu;
        private CCLabelTTF _nextLabel;

        // Track page we are on
        private int _currentPage;
        private int _totalPages;

        // Urls we link to
        private const string _licenseUrl = "http://linerunner3d.com/license";
        private const string _supportUrl = "http://linerunner3d.com/support";


        public HelpMasterLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                0.8f * screenSize.Width,
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

            // Version
            var versionText = string.Empty;
#if ANDROID
            versionText = Program.SharedProgram.Resources.GetString(Resource.String.CommonVersion);
#elif IOS
            versionText = NSBundle.MainBundle.LocalizedString(Strings.CommonVersion, Strings.CommonVersion);
#else
            versionText = AppResources.CommonVersion;
#endif
            var versionHeader = new CCLabelTTF(versionText + " " + FileUtils.GetVersion(), GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            versionHeader.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.95f * this.ContentSize.Height);
            this.AddChild(versionHeader);


            // Help title
            var helpText = string.Empty;
#if ANDROID
            helpText = Program.SharedProgram.Resources.GetString(Resource.String.HelpTitle);
#elif IOS
            helpText = NSBundle.MainBundle.LocalizedString(Strings.HelpTitle, Strings.HelpTitle);
#else
            helpText = AppResources.HelpTitle;
#endif
            var helpTitle = new CCLabelTTF(helpText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            helpTitle.AnchorPoint = CCPoint.AnchorMiddleLeft;
            helpTitle.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.9f  * this.ContentSize.Height);
            this.AddChild(helpTitle);

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
                0.5f  * this.ContentSize.Width,
                0.85f * this.ContentSize.Height);
            this.AddChild(headerLineImage);

            // License
            // Only for Android for now. Add in as needed. IOS has a default license
            // we can use for now.
            var licenseText = string.Empty;
#if ANDROID
            licenseText = Program.SharedProgram.Resources.GetString(Resource.String.HelpLicense);
#elif IOS
            licenseText = NSBundle.MainBundle.LocalizedString(Strings.HelpLicense, Strings.HelpLicense);
#else
            licenseText = AppResources.HelpLicense;
#endif
            var licenseLabel = new CCLabelTTF(licenseText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            var licenseItem = new CCMenuItemLabel(licenseLabel,
                (obj) => { this.LaunchLicense(); });
            var licenseMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        licenseItem,
                    });
            licenseMenu.Position = new CCPoint(
                 0.2f  * this.ContentSize.Width,
                 0.8f * this.ContentSize.Height);
#if ANDROID
            this.AddChild(licenseMenu);
#endif

            // Support
            // Only add in as needed. Currently both Android and IOS allow specifying a support link 
            // in setting up store.
            var supportText = string.Empty;
#if ANDROID
            supportText = Program.SharedProgram.Resources.GetString(Resource.String.HelpSupport);
#elif IOS
            supportText = NSBundle.MainBundle.LocalizedString(Strings.HelpSupport, Strings.HelpSupport);
#else
            supportText = AppResources.HelpSupport;
#endif
            var supportLabel = new CCLabelTTF(supportText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            var supportItem = new CCMenuItemLabel(supportLabel,
                (obj) => { this.LaunchSupport(); });
            var supportMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        supportItem,
                    });
            supportMenu.Position = new CCPoint(
                 0.8f * this.ContentSize.Width,
                 0.8f * this.ContentSize.Height);
            // this.AddChild(supportMenu);

            // First page create up front, rest created on-demand, see GetHelpPage() below
            this._helpPage1Layer = new HelpPage1Layer(this._parent, this);
            this.AddChild(this._helpPage1Layer);
            this._currentPage = 1;
            this._totalPages = 9;

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
                    case 9:
                    {
                        if (this._helpPage9Layer == null)
                        {
                            this._helpPage9Layer = new HelpPage9Layer(this._parent, this);
                            this.AddChild(this._helpPage9Layer);
                        }
                        returnHelpPage = this._helpPage9Layer;

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
            var exitingLayer = this.GetHelpPage(this._currentPage + 1);
            var incomingLayer = this.GetHelpPage(this._currentPage);

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
            var exitingLayer = this.GetHelpPage(this._currentPage - 1);
            var incomingLayer = this.GetHelpPage(this._currentPage);

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
            // Page number
            this._pageNumberHeader.Text = this._pageNumberText + " " + this._currentPage;

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

        private void LaunchLicense()
        {
#if ANDROID
            Program.SharedProgram.LaunchBrowser(HelpMasterLayer._licenseUrl);
#elif IOS
            Program.SharedProgram.LaunchBrowser(HelpMasterLayer._licenseUrl);
#elif WINDOWS_PHONE
            Program.SharedProgram.LaunchBrowser(HelpMasterLayer._licenseUrl);
#elif NETFX_CORE
            var app = App.Current as App;
            app.LaunchBrowser(HelpMasterLayer._licenseUrl);
#endif
        }

        private void LaunchSupport()
        {
#if ANDROID
            Program.SharedProgram.LaunchBrowser(HelpMasterLayer._supportUrl);
#elif IOS
            Program.SharedProgram.LaunchBrowser(HelpMasterLayer._supportUrl);
#elif WINDOWS_PHONE
            Program.SharedProgram.LaunchBrowser(HelpMasterLayer._supportUrl);
#elif NETFX_CORE
            var app = App.Current as App;
            app.LaunchBrowser(HelpMasterLayer._supportUrl);
#endif
        }

        #endregion
    }
}