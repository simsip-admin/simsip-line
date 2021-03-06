using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage2Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        public HelpPage2Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // TODO: Just going with description 1 for now
            // How to play sub-title
            /*
            var howToPlayText = string.Empty;
#if ANDROID
            howToPlayText = Program.SharedProgram.Resources.GetString(Resource.String.HelpHowToPlay);
#elif IOS
            howToPlayText = NSBundle.MainBundle.LocalizedString(Strings.HelpHowToPlay, Strings.HelpHowToPlay);
#else
            howToPlayText = AppResources.HelpHowToPlay;
#endif
            var howToSubtitle = new CCLabelTTF(howToPlayText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            howToSubtitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(howToSubtitle);
            */

            // How to play description 1
            var howToPlayDescription1Text = string.Empty;
#if ANDROID
            howToPlayDescription1Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpHowToPlayDescription1);
#elif IOS
            howToPlayDescription1Text = NSBundle.MainBundle.LocalizedString(Strings.HelpHowToPlayDescription1, Strings.HelpHowToPlayDescription1);
#else
            howToPlayDescription1Text = AppResources.HelpHowToPlayDescription1;
#endif
            var howToDescription1 = new CCLabelTTF(howToPlayDescription1Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            howToDescription1.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            howToDescription1.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.75f * this.ContentSize.Height);
            this.AddChild(howToDescription1);

            // Phone
            var phoneImage = new CCSprite("Images/Misc/HelpPhone");
            phoneImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            phoneImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(phoneImage);

            // TODO: Just going with description 1 for now
            // How to play description 2
            /*
            var howToPlayDescription2Text = string.Empty;
#if ANDROID
            howToPlayDescription2Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpHowToPlayDescription2);
#elif IOS
            howToPlayDescription2Text = NSBundle.MainBundle.LocalizedString(Strings.HelpHowToPlayDescription2, Strings.HelpHowToPlayDescription2);
#else
            howToPlayDescription2Text = AppResources.HelpHowToPlayDescription2;
#endif
            var howToDescription2 = new CCLabelTTF(howToPlayDescription2Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            howToDescription2.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(howToDescription2);
            */
        }
    }
}