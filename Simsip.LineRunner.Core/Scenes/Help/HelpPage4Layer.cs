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

        // Finger press
        private CCSprite _fingerPressImage;
        private CCAction _fingerPressAction;

        public HelpPage4Layer(CoreScene parent, HelpMasterLayer masterLayer)
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
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 2", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Rotate text
            var rotateText = string.Empty;
#if ANDROID
            rotateText = Program.SharedProgram.Resources.GetString(Resource.String.HelpRotate);
#elif IOS
            rotateText = NSBundle.MainBundle.LocalizedString(Strings.HelpRotate, Strings.HelpRotate);
#else
            rotateText = AppResources.HelpRotate;
#endif
            var rotateLabel = new CCLabelTTF(rotateText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            rotateLabel.AnchorPoint = CCPoint.AnchorMiddle;
            rotateLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(rotateLabel);

            // Rotate image
            var rotateImage = new CCSprite("Images/Misc/HelpRotate.png");
            rotateImage.AnchorPoint = CCPoint.AnchorMiddle;
            rotateImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(rotateImage);

            // Finger press
            this._fingerPressImage = new CCSprite("Images/Misc/FingerPress.png");
            this._fingerPressImage.AnchorPoint = CCPoint.AnchorMiddle;
            this.AddChild(this._fingerPressImage);
            var dragStartPlacementAction = new CCPlace(new CCPoint(
                0.6f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height));
            var dragConfig1 = new CCBezierConfig()
                {
                    ControlPoint1 = new CCPoint(
                        0.2f * this.ContentSize.Width,
                        0.5f * this.ContentSize.Height),
                    ControlPoint2 = new CCPoint(
                        0.2f * this.ContentSize.Width,
                        0.6f * this.ContentSize.Height),
                    EndPosition = new CCPoint(
                        0.4f * this.ContentSize.Width,
                        0.7f * this.ContentSize.Height)
                };
            var dragTo1 = new CCBezierTo(GameConstants.DURATION_LAYER_TRANSITION, dragConfig1);
            var dragConfig2 = new CCBezierConfig()
            {
                ControlPoint1 = new CCPoint(
                    0.8f * this.ContentSize.Width,
                    0.6f * this.ContentSize.Height),
                ControlPoint2 = new CCPoint(
                    0.8f * this.ContentSize.Width,
                    0.5f * this.ContentSize.Height),
                EndPosition = new CCPoint(
                    0.6f * this.ContentSize.Width,
                    0.4f * this.ContentSize.Height)
            };
            var dragTo2 = new CCBezierTo(GameConstants.DURATION_LAYER_TRANSITION, dragConfig2);
            this._fingerPressAction =  new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                { 
                    new CCShow(),
                    dragTo1,
                    dragTo2,
                    new CCHide(),
                    new CCDelayTime(GameConstants.DURATION_LAYER_TRANSITION)
                }));
            
            // Double tap text
            var doubleTapText = string.Empty;
#if ANDROID
            doubleTapText = Program.SharedProgram.Resources.GetString(Resource.String.CommonReset);
#elif IOS
            doubleTapText = NSBundle.MainBundle.LocalizedString(Strings.CommonReset, Strings.CommonReset);
#else
            doubleTapText = AppResources.CommonReset;
#endif
            var doubleTapLabel = new CCLabelTTF(doubleTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            doubleTapLabel.AnchorPoint = CCPoint.AnchorMiddle;
            doubleTapLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(doubleTapLabel);

        }

        public override void OnEnter()
        {
            base.OnEnter();

            this._fingerPressImage.RunAction(this._fingerPressAction);
        }
    }
}