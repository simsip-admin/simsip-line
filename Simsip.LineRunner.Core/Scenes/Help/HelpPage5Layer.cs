using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage5Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        // Finger press
        private CCSprite _fingerPressImage;
        private CCAction _fingerPressAction;

        public HelpPage5Layer(CoreScene parent, HelpMasterLayer masterLayer)
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
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 5", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

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

            // Scroll text
            var scrollText = string.Empty;
#if ANDROID
            scrollText = Program.SharedProgram.Resources.GetString(Resource.String.HelpScroll);
#elif IOS
            scrollText = NSBundle.MainBundle.LocalizedString(Strings.HelpScroll, Strings.HelpScroll);
#else
            scrollText = AppResources.HelpScroll;
#endif
            var scrollLabel = new CCLabelTTF(scrollText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            scrollLabel.AnchorPoint = CCPoint.AnchorMiddle;
            scrollLabel.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(scrollLabel);

            // Scroll image
            var joystickLeftImage = new CCSprite("Images/Icons/JoystickLeftNormal.png");
            joystickLeftImage.AnchorPoint = CCPoint.AnchorMiddle;
            joystickLeftImage.Position = new CCPoint(
                0.3f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            this.AddChild(joystickLeftImage);
            var joystickRightImage = new CCSprite("Images/Icons/JoystickRightNormal.png");
            joystickRightImage.AnchorPoint = CCPoint.AnchorMiddle;
            joystickRightImage.Position = new CCPoint(
                0.7f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            this.AddChild(joystickRightImage);
            var joystickUpImage = new CCSprite("Images/Icons/JoystickUpNormal.png");
            joystickUpImage.AnchorPoint = CCPoint.AnchorMiddle;
            joystickUpImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(joystickUpImage);
            var joystickDownImage = new CCSprite("Images/Icons/JoystickDownNormal.png");
            joystickDownImage.AnchorPoint = CCPoint.AnchorMiddle;
            joystickDownImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.35f * this.ContentSize.Height);
            this.AddChild(joystickDownImage);
            var joystickText = string.Empty;
#if ANDROID
            joystickText = Program.SharedProgram.Resources.GetString(Resource.String.HudJoystick);
#elif IOS
            joystickText = NSBundle.MainBundle.LocalizedString(Strings.HudJoystick, Strings.HudJoystick);
#else
            joystickText = AppResources.HudJoystick;
#endif
            var joystickLabel = new CCLabelTTF(joystickText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            joystickLabel.AnchorPoint = CCPoint.AnchorMiddle;
            joystickLabel.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            this.AddChild(joystickLabel);

            // Finger press
            this._fingerPressImage = new CCSprite("Images/Misc/FingerPressMedium.png");
            this._fingerPressImage.AnchorPoint = CCPoint.AnchorMiddle;
            this.AddChild(this._fingerPressImage);
            this._fingerPressAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                { 
                    new CCPlace(new CCPoint(
                        0.7f  * this.ContentSize.Width,
                        0.45f * this.ContentSize.Height)),
                    new CCShow(),
                    new CCDelayTime(GameConstants.DURATION_LAYER_TRANSITION),
                    new CCHide(),
                    new CCDelayTime(GameConstants.DURATION_LAYER_TRANSITION),
                    new CCPlace(new CCPoint(
                        0.5f  * this.ContentSize.Width,
                        0.55f * this.ContentSize.Height)),
                    new CCShow(),
                    new CCDelayTime(GameConstants.DURATION_LAYER_TRANSITION),
                    new CCHide()
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
    }
}