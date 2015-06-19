using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.InApp;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.InApp;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.Services.Inapp;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if ANDROID
using Android.App;
using Xamarin.InAppBilling;
#endif
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
        
        // Description lines
        private string _practiceDesc1Text;
        private string _practiceDesc2Text;
        private CCLabelTTF _practiceDesc1Label;
        private CCLabelTTF _practiceDesc2Label;

        // Status line
        private CCLabelTTF _statusLabel;

        // Restore
        private CCMenu _restoreMenu;
        private CCMenu _restoreLabelMenu;

        // Buy
        private CCMenu _buyMenu;
        private CCMenu _buyLabelMenu;

        // Price
        private string _priceText;
        private CCLabelTTF _priceLabel;

        // Purchased on
        private string _purchasedOnText;
        private CCLabelTTF _purchasedOnLabel;

        // Message box messages
        private string _purchasingUpgradeText;
        private string _restoringUpgradeText;

        // Error messages
        private string _purchaseErrorText;
        private string _restoreErrorText;

        // How to text to display after purchase
        private string _howToText;

        // Services we'll need
        private IInappService _inAppService;
        private IInAppSkuRepository _inAppSkuRepository;
        private IInAppPurchaseRepository _inAppPurchaseRepository;

        public UpgradesPage1Layer(CoreScene parent, UpgradesMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Services and event handlers we'll need
            this._inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            this._inAppService.OnPurchaseProduct += OnPurchaseProduct;
            this._inAppService.OnPurchaseProductError += OnPurchaseProductError;
            this._inAppService.OnRestoreProducts += OnRestoreProducts;
            this._inAppService.OnRestoreProductsError += OnRestoreProductsError;
            this._inAppSkuRepository = new InAppSkuRepository();
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
                            if (this._currentPracticeModeImage == this._practiceModeImages.Count - 1)
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
            var practiceTitle = new CCLabelTTF(practiceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            practiceTitle.Color = CCColor3B.Blue;
            practiceTitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.78f * this.ContentSize.Height);
            this.AddChild(practiceTitle);

            // Practice desc1 - See UpdatePracticeDesc1()

            this._practiceDesc1Text = string.Empty;
#if ANDROID
            this._practiceDesc1Text = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPracticeDesc1);
#elif IOS
            this._practiceDesc1Text = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPracticeDesc1, Strings.UpgradesPracticeDesc1);
#else
            this._practiceDesc1Text = AppResources.UpgradesPracticeDesc1;
#endif

            // Status line - See UpdateStatusLabel

            // Practice desc2 - See UpdatePracticeDesc2
            this._practiceDesc2Text = string.Empty;
#if ANDROID
            this._practiceDesc2Text = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPracticeDesc2);
#elif IOS
            this._practiceDesc2Text = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPracticeDesc2, Strings.UpgradesPracticeDesc2);
#else
            this._practiceDesc2Text = AppResources.UpgradesPracticeDesc2;
#endif

            // Restore
            CCMenuItemImage restoreButton =
                new CCMenuItemImage("Images/Icons/RestoreButtonNormal.png",
                                    "Images/Icons/RestoreButtonSelected.png",
                                    (obj) => { this.RestoreProducts(); });
            this._restoreMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        restoreButton, 
                    });
            this._restoreMenu.Position = new CCPoint(
                0.2f * this.ContentSize.Width,
                0.2f  * this.ContentSize.Height);
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
                (obj) => { this.RestoreProducts(); });
            this._restoreLabelMenu = new CCMenu(
                           new CCMenuItem[] 
                    {
                        restoreItem,
                    });
            this._restoreLabelMenu.Position = new CCPoint(
                 0.2f * this.ContentSize.Width,
                 0.1f * this.ContentSize.Height);
            this.AddChild(this._restoreLabelMenu);

            // Price - See UpdatePriceLabel()
            this._priceText = string.Empty;
#if ANDROID
            this._priceText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPrice);
#elif IOS
            this._priceText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPrice, Strings.UpgradesPrice);
#else
            this._priceText = AppResources.UpgradesPrice;
#endif

            // Purchased on - see UpdatePurchaseOn()
            this._purchasedOnText = string.Empty;
#if ANDROID
            this._purchasedOnText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPurchasedOn);
#elif IOS
            this._purchasedOnText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPurchasedOn, Strings.UpgradesPurchasedOn);
#else
            this._purchasedOnText = AppResources.UpgradesPurchasedOn;
#endif

            // Buy
            CCMenuItemImage buyButton =
                new CCMenuItemImage("Images/Icons/BuyButtonNormal.png",
                                    "Images/Icons/BuyButtonSelected.png",
                        (obj) => { this.PurchaseProduct(); });
            this._buyMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        buyButton, 
                    });
            this._buyMenu.Position = new CCPoint(
                0.8f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
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
                (obj) => { this.PurchaseProduct(); });
            this._buyLabelMenu = new CCMenu(
                           new CCMenuItem[] 
                    {
                        buyItem,
                    });
            this._buyLabelMenu.Position = new CCPoint(
                 0.8f * this.ContentSize.Width,
                 0.1f * this.ContentSize.Height);
            this.AddChild(this._buyLabelMenu);

            // Message box messages
            this._purchasingUpgradeText = string.Empty;
#if ANDROID
            this._purchasingUpgradeText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPurchaseStatus);
#elif IOS
            this._purchasingUpgradeText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPurchaseStatus, Strings.UpgradesPurchaseStatus);
#else
            this._purchasingUpgradeText = AppResources.UpgradesPurchaseStatus;
#endif
            this._restoringUpgradeText = string.Empty;
#if ANDROID
            this._restoringUpgradeText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesRestoreStatus);
#elif IOS
            this._restoringUpgradeText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesRestoreStatus, Strings.UpgradesRestoreStatus);
#else
            this._restoringUpgradeText = AppResources.UpgradesRestoreStatus;
#endif

            // Error messages
            this._purchaseErrorText = string.Empty;
#if ANDROID
            this._purchaseErrorText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPurchaseError);
#elif IOS
            this._purchaseErrorText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPurchaseError, Strings.UpgradesPurchaseError);
#else
            this._purchaseErrorText = AppResources.UpgradesPurchaseError;
#endif
            this._restoreErrorText = string.Empty;
#if ANDROID
            this._restoreErrorText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesRestoreError);
#elif IOS
            this._restoreErrorText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesRestoreError, Strings.UpgradesRestoreError);
#else
            this._restoreErrorText = AppResources.UpgradesRestoreError;
#endif

            // How to text to display after purchase
            this._howToText = string.Empty;
#if ANDROID
            this._howToText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPracticeDesc3);
#elif IOS
            this._howToText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPracticeDesc3, Strings.UpgradesPracticeDesc3);
#else
            this._howToText = AppResources.UpgradesPracticeDesc3;
#endif
        }

        #region Overrides

        public override void OnEnter()
        {
            base.OnEnter();

            this._currentPracticeModeImage = 0;
            this.RunAction(this._practiceModeAction);

            this.UpdateUI();
        }

        public override void OnExit()
        {
            base.OnExit();

            this.ActionManager.RemoveAction(this._practiceModeAction);
        }

        #endregion

        #region Event handlers

        private void PurchaseProduct()
        {
            // Provide immediate feedback
            this._parent.TheMessageBoxLayer.Show(
                this._purchasingUpgradeText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Go for the purchase
#if ANDROID
            // IMPORTANT: Call to BuyProduct must be called from MainActivity on its main thread
            Application.SynchronizationContext.Post(Program.SharedProgram.BuyProduct, this._inAppService.PracticeModeProductId);
#elif IOS
            this._inAppService.PurchaseProduct(this._inAppService.PracticeModeProductId);
#endif
        }

        private void OnPurchaseProduct()
        {
            this._parent.TheMessageBoxLayer.Hide();

            this.UpdateUI();
        }

        private void OnPurchaseProductError(int responseCode, string sku)
        {
            this._parent.TheMessageBoxLayer.Hide();

            this.UpdateUI(this._restoreErrorText);
        }

        private void RestoreProducts()
        {
            // Provide immediate feedback
            this._parent.TheMessageBoxLayer.Show(
                this._restoringUpgradeText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Go for the restore
            this._inAppService.RestoreProducts();
        }

        private void OnRestoreProducts()
        {
            this._parent.TheMessageBoxLayer.Hide();

            this.UpdateUI();
        }

        private void OnRestoreProductsError(int responseCode, IDictionary<string, object> skuDetails)
        {
            this._parent.TheMessageBoxLayer.Hide();

            this.UpdateUI(this._restoreErrorText);
        }

        #endregion

        private void UpdateUI(string statusLine="")
        {
            // Get latest price
            var price = string.Empty;
            var practiceProduct =
                this._inAppSkuRepository.GetSkuByProductId(this._inAppService.PracticeModeProductId);
            if (practiceProduct != null)
            {
                price = practiceProduct.Price;
            }
            
            // Determine if purchased
            var practicePurchase =
                this._inAppPurchaseRepository.GetPurchaseByProductId(this._inAppService.PracticeModeProductId);
            if (practicePurchase != null ||
                GameManager.SharedGameManager.AdminAreUpgradesAllowed)
            {
                // UI for purchased upgrade
                this.UpdateDesc1Label(this._howToText);
                this.UpdateDesc2Label(string.Empty);

                this._restoreMenu.Visible = false;
                this._restoreLabelMenu.Visible = false;

                this.UpdatePriceLabel(string.Empty);

                // Note, need to do this because of AdminAreUpgradesAllowed gating above
                if (practicePurchase != null)
                {
                    this.UpdatePurchasedOnLabel(this._purchasedOnText + " " + practicePurchase.PurchaseTime.ToString("g"));
                }
                else
                {
                    this.UpdatePurchasedOnLabel(this._purchasedOnText + " " + DateTime.Now.ToString("g"));
                }

                this._buyMenu.Visible = false;
                this._buyLabelMenu.Visible = false;
            }
            else
            {
                // UI for non-purchased upgrade
                this.UpdateDesc1Label(this._practiceDesc1Text);
                this.UpdateDesc2Label(this._practiceDesc2Text);

                this._restoreMenu.Visible = true;
                this._restoreLabelMenu.Visible = true;

                this.UpdatePriceLabel(this._priceText + " " + price);

                this.UpdatePurchasedOnLabel(string.Empty);

                this._buyMenu.Visible = true;
                this._buyLabelMenu.Visible = true;
            }

            // Did we request a status line
            if (!string.IsNullOrEmpty(statusLine))
            {
                this.UpdateStatusLabel(statusLine);
            }
            else
            {
                this.UpdateStatusLabel(string.Empty);
            }
        }

        private void UpdateDesc1Label(string text)
        {
            if (this._practiceDesc1Label != null)
            {
                this.RemoveChild(this._practiceDesc1Label);
            }

            this._practiceDesc1Label = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._practiceDesc1Label.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(this._practiceDesc1Label);
        }

        private void UpdateDesc2Label(string text)
        {
            if (this._practiceDesc2Label != null)
            {
                this.RemoveChild(this._practiceDesc2Label);
            }

            this._practiceDesc2Label = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._practiceDesc2Label.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(this._practiceDesc2Label);
        }

        private void UpdateStatusLabel(string text)
        {
            if (this._statusLabel != null)
            {
                this.RemoveChild(this._statusLabel);
            }

            this._statusLabel = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._statusLabel.Color = CCColor3B.Red;
            this._statusLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(this._statusLabel);
        }

        private void UpdatePriceLabel(string text)
        {
            if (this._priceLabel != null)
            {
                this.RemoveChild(this._priceLabel);
            }

            this._priceLabel = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._priceLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(this._priceLabel);
        }

        private void UpdatePurchasedOnLabel(string text)
        {
            if (this._purchasedOnLabel != null)
            {
                this.RemoveChild(this._purchasedOnLabel);
            }

            this._purchasedOnLabel = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._purchasedOnLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(this._purchasedOnLabel);
        }
    }
}