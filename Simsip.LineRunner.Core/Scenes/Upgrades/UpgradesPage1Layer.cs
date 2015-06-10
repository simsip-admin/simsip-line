using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.InApp;
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


namespace Simsip.LineRunner.Scenes.Upgrades
{
    public class UpgradesPage1Layer : GameLayer
    {
        private CoreScene _parent;
        private UpgradesMasterLayer _masterLayer;

        // Practice mode
        private IList<CCSprite> _practiceModeImages;
        private int _currentPracticeModeImage;
        private CCAction _practiceModeAction;
        
        // Restore
        private CCMenu _restoreMenu;
        private CCMenu _restoreLabelMenu;

        // Buy
        private CCMenu _buyMenu;
        private CCMenu _buyLabelMenu;

        // Price
        private CCLabelTTF _priceLabel;

        // Purchased on
        private string _purchasedOnText;
        private CCLabelTTF _purchasedOnLabel;

        // Services we'll need
        private IInAppPurchaseRepository _inAppPurchaseRepository;

        public UpgradesPage1Layer(CoreScene parent, UpgradesMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Services we'll need
            this._inAppPurchaseRepository = new InAppPurchaseRepository();

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

            // Practice images (note: placing this first so text is on top of images)
            var practiceModeImage1 = new CCSprite("Models/Pads/Pad1-thumbnail");
            var practiceModeImage2 = new CCSprite("Models/Pads/Pad1-thumbnail");
            var practiceModeImage3 = new CCSprite("Models/Pads/Pad1-thumbnail");
            var practiceModeImage4 = new CCSprite("Models/Pads/Pad1-thumbnail");
            this._practiceModeImages = new List<CCSprite>();
            this._practiceModeImages.Add(practiceModeImage1);
            this._practiceModeImages.Add(practiceModeImage2);
            this._practiceModeImages.Add(practiceModeImage3);
            this._practiceModeImages.Add(practiceModeImage4);
            foreach (var image in this._practiceModeImages)
            {
                image.Opacity = 0;
                image.AnchorPoint = CCPoint.AnchorMiddleBottom;
                image.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    0.1f * this.ContentSize.Height);
                this.AddChild(image);
            }
            practiceModeImage1.Opacity = 255;
            this._practiceModeAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                {
                    new CCDelayTime(GameConstants.DURATION_UPGRADE_IMAGE),
                    new CCCallFunc(() =>
                        {
                            this._practiceModeImages[this._currentPracticeModeImage].RunAction(new CCFadeOut(GameConstants.DURATION_UPGRADE_IMAGE));
                            if (this._currentPracticeModeImage == this._practiceModeImages.Count)
                            {
                                this._currentPracticeModeImage = 0;
                            }
                            else
                            {
                                this._currentPracticeModeImage++;
                            }
                            this._practiceModeImages[this._currentPracticeModeImage].RunAction(new CCFadeIn(GameConstants.DURATION_UPGRADE_IMAGE));
                        }),
                }));

            // Practice title
            var practiceText = string.Empty;
#if ANDROID
            practiceText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPractice);
#elif IOS
            practiceText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPractice, Strings.UpgradesPractice);
#else
            practiceText = AppResources.UpgradesPractice;
#endif
            var practiceTitle = new CCLabelTTF(practiceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            practiceTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(practiceTitle);

            // Practice desc1
            var practiceDesc1Text = string.Empty;
#if ANDROID
            practiceDesc1Text = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPracticeDesc1);
#elif IOS
            practiceDesc1Text = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPracticeDesc1, Strings.UpgradesPracticeDesc1);
#else
            practiceDesc1Text = AppResources.UpgradesPracticeDesc1;
#endif
            var practiceDesc1 = new CCLabelTTF(practiceDesc1Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            practiceDesc1.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(practiceDesc1);

            // Practice desc2
            var practiceDesc2Text = string.Empty;
#if ANDROID
            practiceDesc2Text = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPracticeDesc2);
#elif IOS
            practiceDesc2Text = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPracticeDesc2, Strings.UpgradesPracticeDesc2);
#else
            practiceDesc2Text = AppResources.UpgradesPracticeDesc2;
#endif
            var practiceDesc2 = new CCLabelTTF(practiceDesc2Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            practiceDesc2.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(practiceDesc2);

            // Restore
            CCMenuItemImage restoreButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { _parent.GoBack(); });
            this._restoreMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        restoreButton, 
                    });
            this._restoreMenu.Position = new CCPoint(
                0.25f * this.ContentSize.Width,
                0.1f  * this.ContentSize.Height);
            this.AddChild(this._restoreMenu);
            var restoreText = string.Empty;
#if ANDROID
            restoreText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesRestore);
#elif IOS
            restoreText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesRestore, Strings.UpgradesRestore);
#else
            restoreText = AppResources.UpgradesRestore;
#endif
            var restoreLabel = new CCLabelTTF(restoreText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var restoreItem = new CCMenuItemLabel(restoreLabel,
                (obj) => { this._parent.GoBack(); });
            this._restoreLabelMenu = new CCMenu(
                           new CCMenuItem[] 
                    {
                        restoreItem,
                    });
            this._restoreLabelMenu.Position = new CCPoint(
                 0.25f * this.ContentSize.Width,
                 0.05f * this.ContentSize.Height);
            this.AddChild(this._restoreLabelMenu);

            // Price
            var priceText = string.Empty;
#if ANDROID
            priceText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPrice);
#elif IOS
            priceText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPrice, Strings.UpgradesPrice);
#else
            priceText = AppResources.UpgradesPrice;
#endif
            this._priceLabel = new CCLabelTTF(priceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._priceLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(this._priceLabel);

            // Purchased on
            this._purchasedOnText = string.Empty;
#if ANDROID
            this._purchasedOnText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPurchasedOn);
#elif IOS
            this._purchasedOnText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPurchasedOn, Strings.UpgradesPurchasedOn);
#else
            this._purchasedOnText = AppResources.UpgradesPurchasedOn;
#endif
            this._purchasedOnLabel = new CCLabelTTF(this._purchasedOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._purchasedOnLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(this._purchasedOnLabel);

            // Buy
            CCMenuItemImage buyButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                        (obj) => { _parent.GoBack(); });
            this._buyMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        buyButton, 
                    });
            this._buyMenu.Position = new CCPoint(
                0.75f * this.ContentSize.Width,
                0.1f * this.ContentSize.Height);
            this.AddChild(this._buyMenu);
            var buyText = string.Empty;
#if ANDROID
            buyText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesBuy);
#elif IOS
            buyText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesBuy, Strings.UpgradesBuy);
#else
            buyText = AppResources.UpgradesBuy;
#endif
            var buyLabel = new CCLabelTTF(buyText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var buyItem = new CCMenuItemLabel(buyLabel,
                (obj) => { this._parent.GoBack(); });
            this._buyLabelMenu = new CCMenu(
                           new CCMenuItem[] 
                    {
                        buyItem,
                    });
            this._buyLabelMenu.Position = new CCPoint(
                 0.75f * this.ContentSize.Width,
                 0.05f * this.ContentSize.Height);
            this.AddChild(this._buyLabelMenu);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            this._currentPracticeModeImage = 0;
            this.RunAction(this._practiceModeAction);

            // Determine if purchased
            var practicePurchase = 
                this._inAppPurchaseRepository.GetPurchaseByProductId(this._inAppPurchaseRepository.PracticeModeProductId);
            if (practicePurchase == null)
            {
                this._restoreMenu.Visible = true;
                this._restoreLabelMenu.Visible = true;
                this._priceLabel.Visible = true;
                this._purchasedOnLabel.Visible = false;
                this._buyMenu.Visible = true;
                this._buyLabelMenu.Visible = true;
            }
            else
            {
                this._restoreMenu.Visible = false;
                this._restoreLabelMenu.Visible = false;
                this._priceLabel.Visible = false;
                this._purchasedOnLabel.Visible = true;
                this._purchasedOnLabel.Text = this._purchasedOnText + " " + practicePurchase.PurchaseTime.ToString("g");
                this._buyMenu.Visible = false;
                this._buyLabelMenu.Visible = false;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            this.ActionManager.RemoveAction(this._practiceModeAction);
        }
    }
}