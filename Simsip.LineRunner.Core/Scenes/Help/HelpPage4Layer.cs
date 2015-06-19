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
            hudSubtitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.75f * this.ContentSize.Height);
            this.AddChild(hudSubtitle);

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
                0.5f  * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(rotateLabel);

            // Rotate image
            var rotateImage = new CCSprite("Images/Misc/HelpRotate.png");
            rotateImage.AnchorPoint = CCPoint.AnchorMiddle;
            rotateImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            this.AddChild(rotateImage);

            // Finger press
            this._fingerPressImage = new CCSprite("Images/Misc/FingerPressSmall.png");
            this._fingerPressImage.AnchorPoint = CCPoint.AnchorMiddle;
            this.AddChild(this._fingerPressImage);
            var dragStartPlacementAction = new CCPlace(new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.46f * this.ContentSize.Height));
            var dragTo1 = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                0.48f * this.ContentSize.Width,
                0.5f  * this.ContentSize.Height));
            var dragTo2 = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                0.55f * this.ContentSize.Width,
                0.54f * this.ContentSize.Height));
            this._fingerPressAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                { 
                    dragStartPlacementAction,
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
                0.5f  * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(doubleTapLabel);

        }

        public override void OnEnter()
        {
            base.OnEnter();

            this._fingerPressImage.RunAction(this._fingerPressAction);
        }

        public override void OnExit()
        {
            base.OnExit();

            this._fingerPressImage.StopAction(this._fingerPressAction);
        }
    }
}