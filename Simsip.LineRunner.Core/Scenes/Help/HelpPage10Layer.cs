using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage10Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        // Practice mode
        private IList<CCSprite> _proPackImages;
        private int _currentProPackImage;
        private CCAction _proPackAction;

        public HelpPage10Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Practice images (note: placing this first so text is on top of images)
            // Scaling assumes 2 : 1 ratio
            var proPackImage1 = new CCSprite("Images/Misc/LinerunnerPackProImage1");
            var proPackImage2 = new CCSprite("Images/Misc/LinerunnerPackProImage2");
            var proPackImage3 = new CCSprite("Images/Misc/LinerunnerPackProImage3");
            var proPackImage4 = new CCSprite("Images/Misc/LinerunnerPackProImage4");
            this._proPackImages = new List<CCSprite>();
            this._proPackImages.Add(proPackImage1);
            this._proPackImages.Add(proPackImage2);
            this._proPackImages.Add(proPackImage3);
            this._proPackImages.Add(proPackImage4);
            foreach (var image in this._proPackImages)
            {
                image.Opacity = 0;
                Cocos2DUtils.ResizeSprite(image,
                    0.6f * this.ContentSize.Height,
                    0.3f * this.ContentSize.Height);
                image.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    0.4f * this.ContentSize.Height);
                this.AddChild(image);
            }
            proPackImage1.Opacity = 255;
            this._proPackAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                {
                    new CCDelayTime(2 * GameConstants.DURATION_UPGRADE_IMAGE_DISPLAY),
                    new CCCallFunc(() =>
                        {
                            this._proPackImages[this._currentProPackImage].RunAction(new CCFadeOut(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
                            if (this._currentProPackImage == this._proPackImages.Count - 1)
                            {
                                this._currentProPackImage = 0;
                            }
                            else
                            {
                                this._currentProPackImage++;
                            }
                            this._proPackImages[this._currentProPackImage].RunAction(new CCFadeIn(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
                        }),
                }));

            // Upgrades sub-title
            var upgradesText = string.Empty;
#if ANDROID
            upgradesText = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgrades);
#elif IOS
            upgradesText = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgrades, Strings.HelpUpgrades);
#else
            upgradesText = AppResources.HelpUpgrades;
#endif
            var upgradesSubtitle = new CCLabelTTF(upgradesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            upgradesSubtitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            upgradesSubtitle.Color = CCColor3B.Blue;
            upgradesSubtitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.75f * this.ContentSize.Height);
            this.AddChild(upgradesSubtitle);

            // Pro pack
            var proPackText = string.Empty;
#if ANDROID
            proPackText = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPackPro);
#elif IOS
            proPackText = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPackPro, Strings.HelpUpgradesPackPro);
#else
            proPackText = AppResources.HelpUpgradesPackPro;
#endif
            var proPackTitle = new CCLabelTTF(proPackText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            proPackTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            proPackTitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.68f * this.ContentSize.Height);
            this.AddChild(proPackTitle);

            var proPackDesc1Text = string.Empty;
#if ANDROID
            proPackDesc1Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPackProDesc1);
#elif IOS
            proPackDesc1Text = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPackProDesc1, Strings.HelpUpgradesPackProDesc1);
#else
            proPackDesc1Text = AppResources.HelpUpgradesPackProDesc1;
#endif
            var proPackDesc1 = new CCLabelTTF(proPackDesc1Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            proPackDesc1.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(proPackDesc1);

            var proPackDesc2Text = string.Empty;
#if ANDROID
            proPackDesc2Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPackProDesc2);
#elif IOS
            proPackDesc2Text = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPackProDesc2, Strings.HelpUpgradesPackProDesc2);
#else
            proPackDesc2Text = AppResources.HelpUpgradesPackProDesc2;
#endif
            var proPackDesc2 = new CCLabelTTF(proPackDesc2Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            proPackDesc2.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(proPackDesc2);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            this._currentProPackImage = 0;
            this.RunAction(this._proPackAction);
        }

        public override void OnExit()
        {
            base.OnExit();

            this.ActionManager.RemoveAction(this._proPackAction);
        }
    }
}