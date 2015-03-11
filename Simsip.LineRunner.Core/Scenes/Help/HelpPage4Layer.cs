using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage4Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        public HelpPage4Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage4Of4);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage4Of4, Strings.CommonPage4Of4);
#else
            pageNumberText = AppResources.CommonPage4Of4;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Options sub-title
            var optionsText = string.Empty;
#if ANDROID
            optionsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpOptions);
#elif IOS
            optionsText = NSBundle.MainBundle.LocalizedString(Strings.HelpOptions, Strings.HelpOptions);
#else
            optionsText = AppResources.HelpOptions;
#endif
            var optionsSubtitle = new CCLabelTTF(optionsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            optionsSubtitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(optionsSubtitle);

            // Resource packs
            var resourcePacksImage = new CCSprite("Images/Misc/HelpResourcePacks");
            Cocos2DUtils.ResizeSprite(resourcePacksImage,
                0.08f * this.ContentSize.Width,
                0.08f * this.ContentSize.Height);
            resourcePacksImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            resourcePacksImage.Position = new CCPoint(
                0.2f  * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(resourcePacksImage);
            var resourcePacksText = string.Empty;
#if ANDROID
            resourcePacksText = Program.SharedProgram.Resources.GetString(Resource.String.HelpResourcePacks);
#elif IOS
            resourcePacksText = NSBundle.MainBundle.LocalizedString(Strings.HelpResourcePacks, Strings.HelpResourcePacks);
#else
            resourcePacksText = AppResources.HelpResourcePacks;
#endif
            var resourcePacksDescription = new CCLabelTTF(resourcePacksText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            resourcePacksDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            resourcePacksDescription.Position = new CCPoint(
                0.25f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(resourcePacksDescription);

            // Pads
            var padsImage = new CCSprite("Images/Misc/HelpPads");
            Cocos2DUtils.ResizeSprite(padsImage,
                0.08f * this.ContentSize.Width,
                0.08f * this.ContentSize.Height);
            padsImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            padsImage.Position = new CCPoint(
                0.2f  * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(padsImage);
            var padsText = string.Empty;
#if ANDROID
            padsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpPads);
#elif IOS
            padsText = NSBundle.MainBundle.LocalizedString(Strings.HelpPads, Strings.HelpPads);
#else
            padsText = AppResources.HelpPads;
#endif
            var padsDescription = new CCLabelTTF(padsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            padsDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            padsDescription.Position = new CCPoint(
                0.25f * this.ContentSize.Width,
                0.5f  * this.ContentSize.Height);
            this.AddChild(padsDescription);

            // Lines
            var linesImage = new CCSprite("Images/Misc/HelpLines");
            Cocos2DUtils.ResizeSprite(linesImage,
                0.08f * this.ContentSize.Width,
                0.08f * this.ContentSize.Height);
            linesImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            linesImage.Position = new CCPoint(
                0.2f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(linesImage);
            var linesText = string.Empty;
#if ANDROID
            linesText = Program.SharedProgram.Resources.GetString(Resource.String.HelpLines);
#elif IOS
            linesText = NSBundle.MainBundle.LocalizedString(Strings.HelpLines, Strings.HelpLines);
#else
            linesText = AppResources.HelpLines;
#endif
            var linesDescription = new CCLabelTTF(linesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            linesDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            linesDescription.Position = new CCPoint(
                0.25f * this.ContentSize.Width,
                0.4f  * this.ContentSize.Height);
            this.AddChild(linesDescription);

            // Worlds
            var worldsImage = new CCSprite("Images/Misc/HelpWorlds");
            Cocos2DUtils.ResizeSprite(worldsImage,
                0.08f * this.ContentSize.Width,
                0.08f * this.ContentSize.Height);
            worldsImage.AnchorPoint = CCPoint.AnchorMiddleRight;
            worldsImage.Position = new CCPoint(
                0.2f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(worldsImage);
            var worldsText = string.Empty;
#if ANDROID
            worldsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpWorlds);
#elif IOS
            worldsText = NSBundle.MainBundle.LocalizedString(Strings.HelpWorlds, Strings.HelpWorlds);
#else
            worldsText = AppResources.HelpWorlds;
#endif
            var worldsDescription = new CCLabelTTF(worldsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            worldsDescription.AnchorPoint = CCPoint.AnchorMiddleLeft;
            worldsDescription.Position = new CCPoint(
                0.25f * this.ContentSize.Width,
                0.3f  * this.ContentSize.Height);
            this.AddChild(worldsDescription);

            // Options footer
            var optionsFooter = new CCLabelTTF("plus sound, time and more", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            optionsFooter.AnchorPoint = CCPoint.AnchorMiddleLeft;
            optionsFooter.Position = new CCPoint(
                0.25f  * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(optionsFooter);
        }
    }
}