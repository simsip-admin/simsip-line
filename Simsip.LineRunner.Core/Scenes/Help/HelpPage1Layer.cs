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
                0.9f  * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Start button
            var startButtonImage = new CCSprite("Images/Icons/StartButtonNormal");
            startButtonImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            startButtonImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.7f * this.ContentSize.Height);
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
            startButtonDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            startButtonDescription.Position = new CCPoint(
                0.4f * this.ContentSize.Width, 
                0.7f * this.ContentSize.Height);
            this.AddChild(startButtonDescription);

            // Ratings button
            var ratingsButtonImage = new CCSprite("Images/Icons/RatingsButtonNormal");
            ratingsButtonImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            ratingsButtonImage.Position = new CCPoint(
                0.3f  * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
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
            ratingsButtonDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            ratingsButtonDescription.Position = new CCPoint(
                0.4f  * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(ratingsButtonDescription);

            // Options button
            var optionsButtonImage = new CCSprite("Images/Icons/OptionsButtonNormal");
            optionsButtonImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            optionsButtonImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width, 
                0.4f * this.ContentSize.Height);
            this.AddChild(optionsButtonImage);
            var helpOptionsDescriptionText = string.Empty;
#if ANDROID
            helpOptionsDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.HelpOptionsDescription);
#elif IOS
            helpOptionsDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.HelpOptionsDescription, Strings.HelpOptionsDescription);
#else
            helpOptionsDescriptionText = AppResources.HelpOptionsDescription;
#endif
            var optionsButtonDescription = new CCLabelTTF(helpOptionsDescriptionText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            optionsButtonDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            optionsButtonDescription.Position = new CCPoint(
                0.4f * this.ContentSize.Width, 
                0.4f * this.ContentSize.Height);
            this.AddChild(optionsButtonDescription);

            // Help button
            var helpButtonImage = new CCSprite("Images/Icons/HelpButtonNormal");
            helpButtonImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            helpButtonImage.Position = new CCPoint(
                0.3f  * this.ContentSize.Width, 
                0.25f * this.ContentSize.Height);
            this.AddChild(helpButtonImage);
            var helpHelpDescriptionText = string.Empty;
#if ANDROID
            helpHelpDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.HelpHelpDescription);
#elif IOS
            helpHelpDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.HelpHelpDescription, Strings.HelpHelpDescription);
#else
            helpHelpDescriptionText = AppResources.HelpHelpDescription;
#endif

            var helpButtonDescription = new CCLabelTTF(helpHelpDescriptionText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            helpButtonDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            helpButtonDescription.Position = new CCPoint(
                0.4f  * this.ContentSize.Width, 
                0.25f * this.ContentSize.Height);
            this.AddChild(helpButtonDescription);
        }
    }
}