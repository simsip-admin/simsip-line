using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage1Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        public HelpPage1Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Start button
            var startButtonImage = new CCSprite("Images/Icons/StartButtonNormal");
            startButtonImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            startButtonImage.Position = new CCPoint(
                0.3f  * this.ContentSize.Width, 
                0.6f * this.ContentSize.Height);
            this.AddChild(startButtonImage);
            var helpStartDescriptionText = string.Empty;
#if ANDROID
            helpStartDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.HelpStartDescription);
#elif IOS
            helpStartDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.HelpStartDescription, Strings.HelpStartDescription);
#else
            helpStartDescriptionText = AppResources.HelpStartDescription;
#endif
            var startButtonDescription = new CCLabelTTF(helpStartDescriptionText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            startButtonDescription.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            startButtonDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            startButtonDescription.Position = new CCPoint(
                0.4f  * this.ContentSize.Width, 
                0.6f * this.ContentSize.Height);
            this.AddChild(startButtonDescription);

            // Ratings button
            var ratingsButtonImage = new CCSprite("Images/Icons/RatingsButtonNormal");
            ratingsButtonImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            ratingsButtonImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(ratingsButtonImage);
            var helpRatingsDescriptionText = string.Empty;
#if ANDROID
            helpRatingsDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.HelpRatingsDescription);
#elif IOS
            helpRatingsDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.HelpRatingsDescription, Strings.HelpRatingsDescription);
#else
            helpRatingsDescriptionText = AppResources.HelpRatingsDescription;
#endif
            var ratingsButtonDescription = new CCLabelTTF(helpRatingsDescriptionText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            ratingsButtonDescription.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            ratingsButtonDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            ratingsButtonDescription.Position = new CCPoint(
                0.4f  * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(ratingsButtonDescription);

#if DEBUG
            var adminText = string.Empty;
#if ANDROID
            adminText = Program.SharedProgram.Resources.GetString(Resource.String.CommonAdmin);
#elif IOS
            adminText = NSBundle.MainBundle.LocalizedString(Strings.CommonAdmin, Strings.CommonAdmin);
#else
            adminText = AppResources.CommonAdmin;
#endif
            var adminLabel = new CCLabelTTF(adminText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            adminLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            var adminItem = new CCMenuItemLabel(adminLabel,
                (obj) => { this._parent.Navigate(LayerTags.AdminLayer); });
            var adminLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        adminItem
                    });
            adminLabelMenu.Position = new CCPoint(
                 0.2f * this.ContentSize.Width,
                 0.1f * this.ContentSize.Height);
            this.AddChild(adminLabelMenu);
#endif
        }
    }
}