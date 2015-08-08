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

            // Hud sub-title
            var hudText = string.Empty;
#if ANDROID
            hudText = Program.SharedProgram.Resources.GetString(Resource.String.HelpHud);
#elif IOS
            hudText = NSBundle.MainBundle.LocalizedString(Strings.HelpHud, Strings.HelpHud);
#else
            hudText = AppResources.HelpHud;
#endif
            var hudSubtitle = new CCLabelTTF(hudText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            hudSubtitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            hudSubtitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.75f * this.ContentSize.Height);
            this.AddChild(hudSubtitle);

            // Zoom
            var zoomInImage = new CCSprite("Images/Icons/ZoomInButtonNormal.png");
            zoomInImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            zoomInImage.Position = new CCPoint(
                0.4f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            Cocos2DUtils.ResizeSprite(zoomInImage,
                0.05f * this.ContentSize.Height,
                0.05f * this.ContentSize.Height);
            this.AddChild(zoomInImage);
            var zoomOutImage = new CCSprite("Images/Icons/ZoomOutButtonNormal.png");
            zoomOutImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            zoomOutImage.Position = new CCPoint(
                0.6f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            Cocos2DUtils.ResizeSprite(zoomOutImage,
                0.05f * this.ContentSize.Height,
                0.05f * this.ContentSize.Height);
            this.AddChild(zoomOutImage);
            var zoomInOutText = string.Empty;
#if ANDROID
            zoomInOutText = Program.SharedProgram.Resources.GetString(Resource.String.HelpZoomInOut);
#elif IOS
            zoomInOutText = NSBundle.MainBundle.LocalizedString(Strings.HelpZoomInOut, Strings.HelpZoomInOut);
#else
            zoomInOutText = AppResources.HelpZoomInOut;
#endif
            var zoomInOutLabel = new CCLabelTTF(zoomInOutText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            zoomInOutLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            zoomInOutLabel.AnchorPoint = CCPoint.AnchorMiddle;
            zoomInOutLabel.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(zoomInOutLabel);

            // Speed
            var increaseImage = new CCSprite("Images/Icons/IncreaseButtonNormal.png");
            increaseImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            increaseImage.Position = new CCPoint(
                0.4f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            Cocos2DUtils.ResizeSprite(increaseImage,
                0.05f * this.ContentSize.Height,
                0.05f * this.ContentSize.Height);
            this.AddChild(increaseImage);
            var decreaseImage = new CCSprite("Images/Icons/DecreaseButtonNormal.png");
            decreaseImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            decreaseImage.Position = new CCPoint(
                0.6f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            Cocos2DUtils.ResizeSprite(decreaseImage,
                0.05f * this.ContentSize.Height,
                0.05f * this.ContentSize.Height);
            this.AddChild(decreaseImage);
            var increaseDecreaseText = string.Empty;
#if ANDROID
            increaseDecreaseText = Program.SharedProgram.Resources.GetString(Resource.String.HelpIncreaseDecrease);
#elif IOS
            increaseDecreaseText = NSBundle.MainBundle.LocalizedString(Strings.HelpIncreaseDecrease, Strings.HelpIncreaseDecrease);
#else
            increaseDecreaseText = AppResources.HelpIncreaseDecrease;
#endif
            var increaseDecreaseLabel = new CCLabelTTF(increaseDecreaseText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            increaseDecreaseLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            increaseDecreaseLabel.AnchorPoint = CCPoint.AnchorMiddle;
            increaseDecreaseLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(increaseDecreaseLabel);

            // Pause/resume
            var pauseImage = new CCSprite("Images/Icons/PauseButtonNormal.png");
            pauseImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            pauseImage.Position = new CCPoint(
                0.4f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            Cocos2DUtils.ResizeSprite(pauseImage,
                0.05f * this.ContentSize.Height,
                0.05f * this.ContentSize.Height);
            this.AddChild(pauseImage);
            var resumeImage = new CCSprite("Images/Icons/ResumeButtonNormal.png");
            resumeImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            resumeImage.Position = new CCPoint(
                0.6f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            Cocos2DUtils.ResizeSprite(resumeImage,
                0.05f * this.ContentSize.Height,
                0.05f * this.ContentSize.Height);
            this.AddChild(resumeImage);
            var pauseResumeText = string.Empty;
#if ANDROID
            pauseResumeText = Program.SharedProgram.Resources.GetString(Resource.String.HelpPauseResume);
#elif IOS
            pauseResumeText = NSBundle.MainBundle.LocalizedString(Strings.HelpPauseResume, Strings.HelpPauseResume);
#else
            pauseResumeText = AppResources.HelpPauseResume;
#endif
            var pauseResumeLabel = new CCLabelTTF(pauseResumeText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pauseResumeLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            pauseResumeLabel.AnchorPoint = CCPoint.AnchorMiddle;
            pauseResumeLabel.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(pauseResumeLabel);

        }
    }
}