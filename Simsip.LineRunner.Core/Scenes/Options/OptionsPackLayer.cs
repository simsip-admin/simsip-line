using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.GameObjects;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Obstacles;
#if ANDROID || IOS
using Simsip.LineRunner.Data.InApp;
using Simsip.LineRunner.Services.Inapp;
#endif
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Options
{
    public class OptionsPackLayer : GameLayer
    {
        private CoreScene _parent;
        private OptionsMasterLayer _masterLayer;

        // Collection of pack labels we have purchased
        private Dictionary<string, CCLabelTTF> _packLabels;

        // Additional text
        private string _switchingPackText;

        // Services we'll need
        IObstacleCache _obstacleCache;
#if ANDROID || IOS
        private IInappService _inAppService;
        private IInAppPurchaseRepository _inAppPurchaseRepository;

#endif

        public OptionsPackLayer(CoreScene parent, OptionsMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Grab references to services we'll need
            this._obstacleCache = (IObstacleCache)TheGame.SharedGame.Services.GetService(typeof(IObstacleCache));
#if ANDROID || IOS
            this._inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService)); 
            this._inAppPurchaseRepository = new InAppPurchaseRepository();
#endif

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

#if ANDROID || IOS

            // Pack title
            var packText = string.Empty;
#if ANDROID
            packText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLinerunnerPacks);
#elif IOS
            packText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLinerunnerPacks, Strings.OptionsLinerunnerPacks);
#else
            packText = AppResources.OptionsLinerunnerPacks;
#endif
            var packLabel = new CCLabelTTF(packText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            packLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            packLabel.AnchorPoint = CCPoint.AnchorMiddle;
            packLabel.Color = CCColor3B.Blue;
            packLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(packLabel);

            // Used to highlight current pack
            this._packLabels = new Dictionary<string, CCLabelTTF>();

            // Standard
            var standardPackImage = new CCSprite("Images/Misc/LinerunnerPackStandardImage1.png");
            Cocos2DUtils.ResizeSprite(standardPackImage,
                0.1f * this.ContentSize.Width,
                0.1f * this.ContentSize.Width);
            var standardPackItem = new CCMenuItemImage();
            standardPackItem.NormalImage = standardPackImage;
            standardPackItem.SetTarget((obj) => { this.PackSelected(this._inAppService.LinerunnerPackStandardProductId); });
            var standardPackMenu = new CCMenu(
                new CCMenuItem[] 
                                {
                                    standardPackItem, 
                                });
            standardPackMenu.Position = new CCPoint(
                0.2f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(standardPackMenu);
            var standardPackText = string.Empty;
#if ANDROID
            standardPackText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLinerunnerPacksStandardPack);
#elif IOS
            standardPackText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLinerunnerPacksStandardPack, Strings.OptionsLinerunnerPacksStandardPack);
#else
            standardPackText = AppResources.OptionsLinerunnerPacksStandardPack;
#endif
            var standardPackLabel = new CCLabelTTF(standardPackText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._packLabels[this._inAppService.LinerunnerPackStandardProductId] = standardPackLabel;
            var standardPackLabelItem = new CCMenuItemLabel(standardPackLabel,
                                        (obj) => { this.PackSelected(this._inAppService.LinerunnerPackStandardProductId); });
            var standardPackLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        standardPackLabelItem
                    });
            standardPackLabelMenu.Position = new CCPoint(
                0.7f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(standardPackLabelMenu);

            var packPurchases = this._inAppPurchaseRepository.GetAllPurchases()
                .Where(x => x.ProductId.StartsWith(this._inAppService.LinerunnerPackPrefix))
                .ToList();
            var lineHeight = standardPackLabelMenu.Position.Y - (0.1f * this.ContentSize.Height);
            foreach(var packPurchase in packPurchases)
            {
                if (packPurchase.ProductId == this._inAppService.LinerunnerPackProProductId)
                {
                    // Pro
                    var proPackImage = new CCSprite("Images/Misc/LinerunnerPackProImage1.png");
                    Cocos2DUtils.ResizeSprite(proPackImage,
                        0.1f * this.ContentSize.Width,
                        0.1f * this.ContentSize.Width);
                    var proPackItem = new CCMenuItemImage();
                    proPackItem.NormalImage = proPackImage;
                    proPackItem.SetTarget((obj) => { this.PackSelected(this._inAppService.LinerunnerPackProProductId); });
                    var proPackMenu = new CCMenu(
                        new CCMenuItem[] 
                                {
                                    proPackItem, 
                                });
                    proPackMenu.Position = new CCPoint(
                        0.2f * this.ContentSize.Width,
                        lineHeight);
                    this.AddChild(proPackMenu);
                    var proPackText = string.Empty;
#if ANDROID
                    proPackText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLinerunnerPacksProPack);
#elif IOS
                    proPackText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLinerunnerPacksProPack, Strings.OptionsLinerunnerPacksProPack);
#else
                    proPackText = AppResources.OptionsLinerunnerPacksProPack;
#endif
                    var proPackLabel = new CCLabelTTF(proPackText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                    this._packLabels[this._inAppService.LinerunnerPackProProductId] = proPackLabel;
                    var proPackLabelItem = new CCMenuItemLabel(proPackLabel,
                                                (obj) => { this.PackSelected(this._inAppService.LinerunnerPackProProductId); });
                    var proPackLabelMenu = new CCMenu(
                       new CCMenuItem[] 
                    {
                        proPackLabelItem
                    });
                    proPackLabelMenu.Position = new CCPoint(
                        0.7f * this.ContentSize.Width,
                        lineHeight);
                    this.AddChild(proPackLabelMenu);
                }
                else if (packPurchase.ProductId == this._inAppService.LinerunnerPackTvProductId)
                {
                    // Tv
                    var tvPackImage = new CCSprite("Images/Misc/LinerunnerPackTvImage1.png");
                    Cocos2DUtils.ResizeSprite(tvPackImage,
                        0.1f * this.ContentSize.Width,
                        0.1f * this.ContentSize.Width);
                    var tvPackItem = new CCMenuItemImage();
                    tvPackItem.NormalImage = tvPackImage;
                    tvPackItem.SetTarget((obj) => { this.PackSelected(this._inAppService.LinerunnerPackTvProductId); });
                    var tvPackMenu = new CCMenu(
                        new CCMenuItem[] 
                                {
                                    tvPackItem, 
                                });
                    tvPackMenu.Position = new CCPoint(
                        0.2f * this.ContentSize.Width,
                        lineHeight);
                    this.AddChild(tvPackMenu);
                    var tvPackText = string.Empty;
#if ANDROID
                    tvPackText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLinerunnerPacksTvPack);
#elif IOS
                    tvPackText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLinerunnerPacksTvPack, Strings.OptionsLinerunnerPacksTvPack);
#else
                    tvPackText = AppResources.OptionsLinerunnerPacksTvPack;
#endif
                    var tvPackLabel = new CCLabelTTF(tvPackText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                    this._packLabels[this._inAppService.LinerunnerPackTvProductId] = tvPackLabel;
                    var tvPackLabelItem = new CCMenuItemLabel(tvPackLabel,
                                                (obj) => { this.PackSelected(this._inAppService.LinerunnerPackTvProductId); });
                    var tvPackLabelMenu = new CCMenu(
                       new CCMenuItem[] 
                    {
                        tvPackLabelItem
                    });
                    tvPackLabelMenu.Position = new CCPoint(
                        0.7f * this.ContentSize.Width,
                        lineHeight);
                    this.AddChild(tvPackLabelMenu);

                }

                lineHeight -= 0.1f * this.ContentSize.Height;
            }
#endif
            // Additional text strings
            this._switchingPackText = string.Empty;

#if ANDROID
            this._switchingPackText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLinerunnerPacksSwitchingPack);
#elif IOS
            this._switchingPackText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLinerunnerPacksSwitchingPack, Strings.OptionsLinerunnerPacksSwitchingPack);
#else
            _switchingPackText = AppResources.OptionsLinerunnerPacksSwitchingPack;
#endif
        }

        #region Overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Reset all label colors
            foreach(var packLabel in this._packLabels)
            {
                packLabel.Value.Color = CCColor3B.White;
            }

            // Highlight current pack
#if ANDROID || IOS
            var currentPackProductId = this._inAppService.LinerunnerPackPrefix + GameManager.SharedGameManager.LinerunnerPack;
            var currentPackLabel = this._packLabels[currentPackProductId];
            currentPackLabel.Color = CCColor3B.Blue;
#endif
        }

        #endregion

        #region Helper methods

        private void PackSelected(string selectedPackProductId)
        {
            // Provide immediate feedback
            this._parent.TheMessageBoxLayer.Show(
                this._switchingPackText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Reset all label colors
            foreach (var packLabel in this._packLabels)
            {
                packLabel.Value.Color = CCColor3B.White;
            }

            // Highlight current pack
            var currentPackLabel = this._packLabels[selectedPackProductId];
            currentPackLabel.Color = CCColor3B.Blue;

#if ANDROID || IOS
            // Record what was selected
            // IMPORTANT: Note how we use the suffix of the id for the selected pack. This is because the database
            //            has recorded only the suffix due to space/performance concerns
            GameManager.SharedGameManager.LinerunnerPack = InAppUtils.GetPackProductIdSuffix(selectedPackProductId);
#endif

            // Hook up an event handler for end of content loading caused by
            // refresh kicking off background load
            this._obstacleCache.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;

            // Start the background refresh to get the new pad displayed. See 
            // LoadContentAsyncFinishedHandler for how we clean-up after refresh is finished
            this._parent.Refresh();
        }

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            // We only want to react to a refresh event
            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                // Unhook so we are a one-shot event handler
                this._obstacleCache.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

                // We can now let this layer's ui resume
                this._parent.TheMessageBoxLayer.Hide();
            }
        }

        #endregion

    }
}