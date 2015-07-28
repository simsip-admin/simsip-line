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
    public class UpgradesPage3Layer : GameLayer
    {
        private CoreScene _parent;
        private UpgradesMasterLayer _masterLayer;

        // Tv pack
        private IList<CCSprite> _tvPackImages;
        private int _currentTvPackImage;
        private CCAction _tvPackAction;
        
        // Description lines
        private string _tvPackDesc1Text;
        private string _tvPackDesc2Text;
        private CCLabelTTF _tvPackDesc1Label;
        private CCLabelTTF _tvPackDesc2Label;

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
        private CCLabelTTF _purchasedOnDateLabel;

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

        public UpgradesPage3Layer(CoreScene parent, UpgradesMasterLayer masterLayer)
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
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 3", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Practice images (note: placing this first so text is on top of images)
            // Scaling assumes 2 : 1 ratio
            var tvPackImage1 = new CCSprite("Images/Misc/LinerunnerPackTvImage1");
            var tvPackImage2 = new CCSprite("Images/Misc/LinerunnerPackTvImage2");
            var tvPackImage3 = new CCSprite("Images/Misc/LinerunnerPackTvImage3");
            var tvPackImage4 = new CCSprite("Images/Misc/LinerunnerPackTvImage4");
            this._tvPackImages = new List<CCSprite>();
            this._tvPackImages.Add(tvPackImage1);
            this._tvPackImages.Add(tvPackImage2);
            this._tvPackImages.Add(tvPackImage3);
            this._tvPackImages.Add(tvPackImage4);
            foreach (var image in this._tvPackImages)
            {
                image.Opacity = 0;
                Cocos2DUtils.ResizeSprite(image,
                    0.6f * this.ContentSize.Height,
                    0.3f * this.ContentSize.Height);
                image.Position = new CCPoint(
                    0.5f  * this.ContentSize.Width,
                    0.45f * this.ContentSize.Height);
                this.AddChild(image);
            }
            tvPackImage1.Opacity = 255;
            this._tvPackAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                {
                    new CCDelayTime(GameConstants.DURATION_UPGRADE_IMAGE_DISPLAY),
                    new CCCallFunc(() =>
                        {
                            this._tvPackImages[this._currentTvPackImage].RunAction(new CCFadeOut(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
                            if (this._currentTvPackImage == this._tvPackImages.Count - 1)
                            {
                                this._currentTvPackImage = 0;
                            }
                            else
                            {
                                this._currentTvPackImage++;
                            }
                            this._tvPackImages[this._currentTvPackImage].RunAction(new CCFadeIn(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
                        }),
                }));

            // Tv pack title
            var tvPackText = string.Empty;
#if ANDROID
            tvPackText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPackTv);
#elif IOS
            tvPackText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPackTv, Strings.UpgradesPackTv);
#else
            tvPackText = AppResources.UpgradesPackTv;
#endif
            var tvPackTitle = new CCLabelTTF(tvPackText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            tvPackTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            tvPackTitle.Color = CCColor3B.Blue;
            tvPackTitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.78f * this.ContentSize.Height);
            this.AddChild(tvPackTitle);

            // Tv pack desc1 - See UpdatePracticeDesc1()

            this._tvPackDesc1Text = string.Empty;
#if ANDROID
            this._tvPackDesc1Text = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPackTvDesc1);
#elif IOS
            this._tvPackDesc1Text = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPackTvDesc1, Strings.UpgradesPackTvDesc1);
#else
            this._tvPackDesc1Text = AppResources.UpgradesPackTvDesc1;
#endif

            // Status line - See UpdateStatusLabel

            // Tv pack desc2 - See UpdatePracticeDesc2
            this._tvPackDesc2Text = string.Empty;
#if ANDROID
            this._tvPackDesc2Text = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPackTvDesc2);
#elif IOS
            this._tvPackDesc2Text = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPackTvDesc2, Strings.UpgradesPackTvDesc2);
#else
            this._tvPackDesc2Text = AppResources.UpgradesPackTvDesc2;
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
            this._howToText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesPackTvDesc3);
#elif IOS
            this._howToText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesPackTvDesc3, Strings.UpgradesPackTvDesc3);
#else
            this._howToText = AppResources.UpgradesPackTvDesc3;
#endif
        }

        #region Overrides

        public override void OnEnter()
        {
            base.OnEnter();

            this._currentTvPackImage = 0;
            this.RunAction(this._tvPackAction);

            this.UpdateUI();
        }

        public override void OnExit()
        {
            base.OnExit();

            this.ActionManager.RemoveAction(this._tvPackAction);
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
            Application.SynchronizationContext.Post(Program.SharedProgram.BuyProduct, this._inAppService.LinerunnerPackTvProductId);
#elif IOS
            this._inAppService.PurchaseProduct(this._inAppService.LinerunnerPackTvProductId);
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
            var tvPackProduct =
                this._inAppSkuRepository.GetSkuByProductId(this._inAppService.LinerunnerPackTvProductId);
            if (tvPackProduct != null)
            {
                price = tvPackProduct.Price;
            }
            
            // Determine if purchased
            var tvPackPurchase =
                this._inAppPurchaseRepository.GetPurchaseByProductId(this._inAppService.LinerunnerPackTvProductId);
            if (tvPackPurchase != null ||
                GameManager.SharedGameManager.AdminAreUpgradesAllowed)
            {
                // UI for purchased upgrade
                this.UpdateDesc1Label(this._howToText);
                this.UpdateDesc2Label(string.Empty);

                this._restoreMenu.Visible = false;
                this._restoreLabelMenu.Visible = false;

                this.UpdatePriceLabel(string.Empty);

                // Note, need to do this because of AdminAreUpgradesAllowed gating above
                if (tvPackPurchase != null)
                {
                    this.UpdatePurchasedOnLabel(tvPackPurchase.PurchaseTime.ToString("g"));
                }
                else
                {
                    this.UpdatePurchasedOnLabel(DateTime.Now.ToString("g"));
                }

                this._buyMenu.Visible = false;
                this._buyLabelMenu.Visible = false;
            }
            else
            {
                // UI for non-purchased upgrade
                this.UpdateDesc1Label(this._tvPackDesc1Text);
                this.UpdateDesc2Label(this._tvPackDesc2Text);

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
            if (this._tvPackDesc1Label != null)
            {
                this.RemoveChild(this._tvPackDesc1Label);
            }

            this._tvPackDesc1Label = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._tvPackDesc1Label.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(this._tvPackDesc1Label);
        }

        private void UpdateDesc2Label(string text)
        {
            if (this._tvPackDesc2Label != null)
            {
                this.RemoveChild(this._tvPackDesc2Label);
            }

            this._tvPackDesc2Label = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._tvPackDesc2Label.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(this._tvPackDesc2Label);
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
                0.5f  * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(this._priceLabel);
        }

        private void UpdatePurchasedOnLabel(string text)
        {
            if (this._purchasedOnLabel != null)
            {
                this.RemoveChild(this._purchasedOnLabel);
            }
            if (this._purchasedOnDateLabel != null)
            {
                this.RemoveChild(this._purchasedOnDateLabel);
            }

            this._purchasedOnLabel = new CCLabelTTF(this._purchasedOnText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            this._purchasedOnLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(this._purchasedOnLabel);

            this._purchasedOnDateLabel = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            this._purchasedOnDateLabel.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(this._purchasedOnDateLabel);
        }
    }
}