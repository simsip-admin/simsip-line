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
    public class HelpPage11Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        // Practice mode
        private IList<CCSprite> _tvPackImages;
        private int _currentTvPackImage;
        private CCAction _tvPackAction;

        public HelpPage11Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Practice images (note: placing this first so text is on top of images)
            // Scaling assumes 2 : 1 ratio
            var tvPackImage1 = new CCSprite("Images/Misc/LinerunnerPackTvImage1");
            var tvPackImage2 = new CCSprite("Images/Misc/LinerunnerPackTvImage2");
            var tvPackImage3 = new CCSprite("Images/Misc/LinerunnerPackTvImage3");
            var tvPackImage4 = new CCSprite("Images/Misc/LinerunnerPackTvImage4");
            this._tvPackImages = new List<CCSprite>();
            this._tvPackImages.Add(tvPackImage1);
            this._tvPackImages.Add(tvPackImage2);
            this._tvPackImages.Add(tvPackImage3);
            this._tvPackImages.Add(tvPackImage4);
            foreach (var image in this._tvPackImages)
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
            tvPackImage1.Opacity = 255;
            this._tvPackAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                {
                    new CCDelayTime(2 * GameConstants.DURATION_UPGRADE_IMAGE_DISPLAY),
                    new CCCallFunc(() =>
                        {
                            this._tvPackImages[this._currentTvPackImage].RunAction(new CCFadeOut(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
                            if (this._currentTvPackImage == this._tvPackImages.Count - 1)
                            {
                                this._currentTvPackImage = 0;
                            }
                            else
                            {
                                this._currentTvPackImage++;
                            }
                            this._tvPackImages[this._currentTvPackImage].RunAction(new CCFadeIn(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
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

            // Tv pack
            var tvPackText = string.Empty;
#if ANDROID
            tvPackText = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPackTv);
#elif IOS
            tvPackText = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPackTv, Strings.HelpUpgradesPackTv);
#else
            tvPackText = AppResources.HelpUpgradesPackTv;
#endif
            var tvPackTitle = new CCLabelTTF(tvPackText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            tvPackTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            tvPackTitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.68f * this.ContentSize.Height);
            this.AddChild(tvPackTitle);

            var tvPackDesc1Text = string.Empty;
#if ANDROID
            tvPackDesc1Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPackTvDesc1);
#elif IOS
            tvPackDesc1Text = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPackTvDesc1, Strings.HelpUpgradesPackTvDesc1);
#else
            tvPackDesc1Text = AppResources.HelpUpgradesPackTvDesc1;
#endif
            var tvPackDesc1 = new CCLabelTTF(tvPackDesc1Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            tvPackDesc1.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(tvPackDesc1);

            var tvPackDesc2Text = string.Empty;
#if ANDROID
            tvPackDesc2Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPackTvDesc2);
#elif IOS
            tvPackDesc2Text = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPackTvDesc2, Strings.HelpUpgradesPackTvDesc2);
#else
            tvPackDesc2Text = AppResources.HelpUpgradesPackTvDesc2;
#endif
            var tvPackDesc2 = new CCLabelTTF(tvPackDesc2Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            tvPackDesc2.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(tvPackDesc2);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            this._currentTvPackImage = 0;
            this.RunAction(this._tvPackAction);
        }

        public override void OnExit()
        {
            base.OnExit();

            this.ActionManager.RemoveAction(this._tvPackAction);
        }
    }
}