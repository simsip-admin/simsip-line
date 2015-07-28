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
            var packLabel = new CCLabelTTF(packText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            packLabel.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            packLabel.AnchorPoint = CCPoint.AnchorMiddle;
            packLabel.Color = CCColor3B.Blue;
            packLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(packLabel);

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
                0.6f * this.ContentSize.Height);
            this.AddChild(standardPackMenu);
            var standardPackText = string.Empty;
#if ANDROID
            standardPackText = Program.SharedProgram.Resources.GetString(Resource.String.OptionsLinerunnerPacks);
#elif IOS
            standardPackText = NSBundle.MainBundle.LocalizedString(Strings.OptionsLinerunnerPacks, Strings.OptionsLinerunnerPacks);
#else
            standardPackText = AppResources.OptionsLinerunnerPacks;
#endif

            var packPurchases = this._inAppPurchaseRepository.GetAllPurchases()
                .Where(x => x.ProductId.StartsWith(this._inAppService.LinerunnerPackPrefix));

            var tableStartHeight = packLabel.Position.Y;
            foreach(var packPurchase in packPurchases)
            {
                if (packPurchase.ProductId == this._inAppService.LinerunnerPackProProductId)
                {

                }
                else if (packPurchase.ProductId == this._inAppService.LinerunnerPackTvProductId)
                {

                }
            }
#endif


        }

        #region Overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Highlight current starting page/line
            // See other options upgrade page
        }

        #endregion

        #region Helper methods

        private void PackSelected(string selectedPack)
        {
            // Provide immediate feedback
            this._parent.TheMessageBoxLayer.Show(
                this._switchingPackText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Update ui to reflect selection
            // See other options upgrade page for this

            // Record what was selected
            // IMPORTANT: Note how we use the suffix of the id for the selected pack. This is because the database
            //            has recorded only the suffix due to space/performance concerns
            var selectedPackSuffix = selectedPack.Substring(selectedPack.LastIndexOf("."));
            GameManager.SharedGameManager.LinerunnerPack = selectedPackSuffix;

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