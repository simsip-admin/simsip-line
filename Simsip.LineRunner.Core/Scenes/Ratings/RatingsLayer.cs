using System;
using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Ratings
{
    public class RatingsLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        public RatingsLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                0.6f * screenSize.Width,
                0.6f * screenSize.Height);

            // Layer transition in/out
            var layerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.2f * screenSize.Width,
                0.2f * screenSize.Height);
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

            // Ratings title
            var ratingsTitleText = string.Empty;
#if ANDROID
            ratingsTitleText = Program.SharedProgram.Resources.GetString(Resource.String.RatingsRateUs);
#elif IOS
            ratingsTitleText = NSBundle.MainBundle.LocalizedString(Strings.RatingsRateUs, Strings.RatingsRateUs);
#else
            ratingsTitleText = AppResources.RatingsRateUs;
#endif
            var ratingsTitle = new CCLabelTTF(ratingsTitleText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            ratingsTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            ratingsTitle.AnchorPoint = CCPoint.AnchorMiddleLeft;
            ratingsTitle.Position = new CCPoint(
                0.05f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(ratingsTitle);

            // Header line
            var headerLineImage = new CCSprite("Images/Misc/HeaderLine");
            Cocos2DUtils.ResizeSprite(headerLineImage,
                0.9f * this.ContentSize.Width,
                0.01f * this.ContentSize.Height);
            headerLineImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.85f * this.ContentSize.Height);
            this.AddChild(headerLineImage);

            // Ratings description
            var ratingsDescText = string.Empty;
#if ANDROID
            ratingsDescText = Program.SharedProgram.Resources.GetString(Resource.String.RatingsDescription);
#elif IOS
            ratingsDescText = NSBundle.MainBundle.LocalizedString(Strings.RatingsDescription, Strings.RatingsDescription);
#else
            ratingsDescText = AppResources.RatingsDescription;
#endif
            var ratingsDescLabel = new CCLabelTTF(ratingsDescText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            ratingsDescLabel.Scale = GameConstants.FONT_SIZE_SMALL_SCALE;
            ratingsDescLabel.AnchorPoint = CCPoint.AnchorMiddle;
            ratingsDescLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(ratingsDescLabel);

            // Yes menu item
            var yesText = string.Empty;
#if ANDROID
            yesText = Program.SharedProgram.Resources.GetString(Resource.String.CommonYes);
#elif IOS
            yesText = NSBundle.MainBundle.LocalizedString(Strings.CommonYes, Strings.CommonYes);
#else
            yesText = AppResources.CommonYes;
#endif
            var yesLabel = new CCLabelTTF(yesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            yesLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            var yesItem = new CCMenuItemLabel(yesLabel,
                (obj) => { this.OnRatingsYes(); });

            // No menu item
            var noText = string.Empty;
#if ANDROID
            noText = Program.SharedProgram.Resources.GetString(Resource.String.CommonNo);
#elif IOS
            noText = NSBundle.MainBundle.LocalizedString(Strings.CommonNo, Strings.CommonNo);
#else
            noText = AppResources.CommonNo;
#endif
            var noLabel = new CCLabelTTF(noText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            noLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            var noItem = new CCMenuItemLabel(noLabel,
                (obj) => { this.OnRatingsNo(); });

            // Later menu item
            var laterText = string.Empty;
#if ANDROID
            laterText = Program.SharedProgram.Resources.GetString(Resource.String.CommonLater);
#elif IOS
            laterText = NSBundle.MainBundle.LocalizedString(Strings.CommonLater, Strings.CommonLater);
#else
            laterText = AppResources.CommonLater;
#endif
            var laterLabel = new CCLabelTTF(laterText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            laterLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            var laterItem = new CCMenuItemLabel(laterLabel,
                (obj) => { this.OnRatingsLater(); });

            // Ratings menu
            var ratingsLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        yesItem,
                        noItem,
                        laterItem
                    });
            ratingsLabelMenu.Position = new CCPoint(
                 0.5f * this.ContentSize.Width,
                 0.1f * this.ContentSize.Height);
            ratingsLabelMenu.AlignItemsHorizontally();
            this.AddChild(ratingsLabelMenu);

            // Back
            var backButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { _parent.GoBack(); });
            var backMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        backButton 
                    });
            backMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.1f * this.ContentSize.Height);
            this.AddChild(backMenu, 0);
            var backText = string.Empty;
#if ANDROID
            backText = Program.SharedProgram.Resources.GetString(Resource.String.CommonBack);
#elif IOS
            backText = NSBundle.MainBundle.LocalizedString(Strings.CommonBack, Strings.CommonBack);
#else
            backText = AppResources.CommonBack;
#endif
            var backLabel = new CCLabelTTF(backText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            backLabel.Scale = GameConstants.FONT_SIZE_SMALL_SCALE;
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
        }

        #endregion

        #region Event handlers

        private void OnRatingsYes()
        {
#if NETFX_CORE
            App.RateApp();
#elif DESKTOP
            Program.RateApp();
#else
            Program.SharedProgram.RateApp();
#endif
            UserDefaults.SharedUserDefault.SetBoolForKey(GameConstants.USER_DEFAULT_KEY_ASK_FOR_RATING, false);
        }

        private void OnRatingsNo()
        {
            // Flip our flag so we don't prompt ever again
            UserDefaults.SharedUserDefault.SetBoolForKey(GameConstants.USER_DEFAULT_KEY_ASK_FOR_RATING, false);
        }

        private void OnRatingsLater()
        {
            // Ok, we'll prompt again in another ratings window period
            UserDefaults.SharedUserDefault.SetDateForKey(GameConstants.USER_DEFAULT_KEY_INSTALL_DATE, DateTime.Now);
        }

        #endregion
    }
}