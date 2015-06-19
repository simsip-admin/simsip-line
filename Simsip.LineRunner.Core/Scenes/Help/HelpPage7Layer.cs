using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage7Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        public HelpPage7Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Options sub-title
            var optionsText = string.Empty;
#if ANDROID
            optionsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpOptions);
#elif IOS
            optionsText = NSBundle.MainBundle.LocalizedString(Strings.HelpOptions, Strings.HelpOptions);
#else
            optionsText = AppResources.HelpOptions;
#endif
            var optionsSubtitle = new CCLabelTTF(optionsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            optionsSubtitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.75f * this.ContentSize.Height);
            this.AddChild(optionsSubtitle);

            // Pads
            var padsImage = new CCSprite("Images/Misc/HelpPads.png");
            padsImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            padsImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(padsImage);
            var padsText = string.Empty;
#if ANDROID
            padsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpPads);
#elif IOS
            padsText = NSBundle.MainBundle.LocalizedString(Strings.HelpPads, Strings.HelpPads);
#else
            padsText = AppResources.HelpPads;
#endif
            var padsDescription = new CCLabelTTF(padsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            padsDescription.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(padsDescription);

            // Lines
            var linesImage = new CCSprite("Images/Misc/HelpLines.png");
            linesImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            linesImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(linesImage);
            var linesText = string.Empty;
#if ANDROID
            linesText = Program.SharedProgram.Resources.GetString(Resource.String.HelpLines);
#elif IOS
            linesText = NSBundle.MainBundle.LocalizedString(Strings.HelpLines, Strings.HelpLines);
#else
            linesText = AppResources.HelpLines;
#endif
            var linesDescription = new CCLabelTTF(linesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            linesDescription.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(linesDescription);
        }
    }
}