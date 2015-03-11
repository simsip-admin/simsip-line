using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage3Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        public HelpPage3Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage3Of4);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage3Of4, Strings.CommonPage3Of4);
#else
            pageNumberText = AppResources.CommonPage3Of4;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Free flight sub-title
            var freeFlightText = string.Empty;
#if ANDROID
            freeFlightText = Program.SharedProgram.Resources.GetString(Resource.String.HelpFreeFlight);
#elif IOS
            freeFlightText = NSBundle.MainBundle.LocalizedString(Strings.HelpFreeFlight, Strings.HelpFreeFlight);
#else
            freeFlightText = AppResources.HelpFreeFlight;
#endif
            var freeflightSubtitle = new CCLabelTTF(freeFlightText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            freeflightSubtitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(freeflightSubtitle);

            // Free flight description 1
            var freeFlightDescription1Text = string.Empty;
#if ANDROID
            freeFlightDescription1Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpfreeFlightDescription1);
#elif IOS
            freeFlightDescription1Text = NSBundle.MainBundle.LocalizedString(Strings.HelpfreeFlightDescription1, Strings.HelpfreeFlightDescription1);
#else
            freeFlightDescription1Text = AppResources.HelpfreeFlightDescription1;
#endif
            var freeFlightDescription1 = new CCLabelTTF(freeFlightDescription1Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            freeFlightDescription1.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(freeFlightDescription1);

            // Phone
            var phoneImage = new CCSprite("Images/Misc/HelpPhone2");
            Cocos2DUtils.ResizeSprite(phoneImage,
                0.7f  * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            phoneImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.43f * this.ContentSize.Height);
            this.AddChild(phoneImage);

            // Free flight description 2
            var freeFlightDescription2Text = string.Empty;
#if ANDROID
            freeFlightDescription2Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpfreeFlightDescription2);
#elif IOS
            freeFlightDescription2Text = NSBundle.MainBundle.LocalizedString(Strings.HelpfreeFlightDescription2, Strings.HelpfreeFlightDescription2);
#else
            freeFlightDescription2Text = AppResources.HelpfreeFlightDescription2;
#endif

            var howToDescription2 = new CCLabelTTF(freeFlightDescription2Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            howToDescription2.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(howToDescription2);
        }
    }
}