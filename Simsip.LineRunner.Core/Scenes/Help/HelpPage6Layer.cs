using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage6Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        // Finger press
        private CCSprite _fingerPressImage;
        private CCAction _fingerPressAction;

        public HelpPage6Layer(CoreScene parent, HelpMasterLayer masterLayer)
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
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 6", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
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

            // Orbit text
            var orbitText = string.Empty;
#if ANDROID
            orbitText = Program.SharedProgram.Resources.GetString(Resource.String.HelpOrbit);
#elif IOS
            orbitText = NSBundle.MainBundle.LocalizedString(Strings.HelpOrbit, Strings.HelpOrbit);
#else
            orbitText = AppResources.HelpOrbit;
#endif
            var orbitLabel = new CCLabelTTF(orbitText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            orbitLabel.AnchorPoint = CCPoint.AnchorMiddle;
            orbitLabel.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.65f * this.ContentSize.Height);
            this.AddChild(orbitLabel);

            // Orbit image
            var orbitImage = new CCSprite("Images/Misc/Trackball.png");
            orbitImage.AnchorPoint = CCPoint.AnchorMiddle;
            orbitImage.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height);
            this.AddChild(orbitImage);

            // Finger press
            this._fingerPressImage = new CCSprite("Images/Misc/FingerPressMedium.png");
            this._fingerPressImage.AnchorPoint = CCPoint.AnchorMiddle;
            this.AddChild(this._fingerPressImage);
            var dragStartPlacementAction = new CCPlace(new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.45f * this.ContentSize.Height));
            var dragTo1 = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                0.4f  * this.ContentSize.Width,
                0.55f * this.ContentSize.Height));
            var dragTo2 = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, new CCPoint(
                0.6f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height));
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

    }
}