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
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage, Strings.CommonPage);
#else
            pageNumberText = AppResources.CommonPage;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 3", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Pause/resume
            var pauseImage = new CCSprite("Images/Icons/PauseButtonNormal.png");
            pauseImage.AnchorPoint = CCPoint.AnchorMiddle;
            pauseImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(pauseImage);
            var resumeImage = new CCSprite("Images/Icons/ResumeButtonNormal.png");
            resumeImage.AnchorPoint = CCPoint.AnchorMiddle;
            resumeImage.Position = new CCPoint(
                0.7f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(pauseImage);
            var pauseResumeText = string.Empty;
#if ANDROID
            pauseResumeText = Program.SharedProgram.Resources.GetString(Resource.String.HelpPauseResume);
#elif IOS
            pauseResumeText = NSBundle.MainBundle.LocalizedString(Strings.HelpPauseResume, Strings.HelpPauseResume);
#else
            pauseResumeText = AppResources.HelpPauseResume;
#endif
            var pauseResumeLabel = new CCLabelTTF(pauseResumeText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pauseResumeLabel.AnchorPoint = CCPoint.AnchorMiddle;
            pauseResumeLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(pauseResumeLabel);

            // Speed
            var decreaseImage = new CCSprite("Images/Icons/DecreaseButtonNormal.png");
            decreaseImage.AnchorPoint = CCPoint.AnchorMiddle;
            decreaseImage.Position = new CCPoint(
                0.3f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(decreaseImage);
            var increaseImage = new CCSprite("Images/Icons/IncreaseButtonNormal.png");
            increaseImage.AnchorPoint = CCPoint.AnchorMiddle;
            increaseImage.Position = new CCPoint(
                0.7f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(increaseImage);
            var increaseDecreaseText = string.Empty;
#if ANDROID
            increaseDecreaseText = Program.SharedProgram.Resources.GetString(Resource.String.HelpIncreaseDecrease);
#elif IOS
            increaseDecreaseText = NSBundle.MainBundle.LocalizedString(Strings.HelpIncreaseDecrease, Strings.HelpIncreaseDecrease);
#else
            increaseDecreaseText = AppResources.HelpIncreaseDecrease;
#endif
            var increaseDecreaseLabel = new CCLabelTTF(increaseDecreaseText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            increaseDecreaseLabel.AnchorPoint = CCPoint.AnchorMiddle;
            increaseDecreaseLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(increaseDecreaseLabel);
        }
    }
}