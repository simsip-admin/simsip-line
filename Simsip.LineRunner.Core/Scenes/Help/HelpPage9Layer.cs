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
    public class HelpPage9Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        // Practice mode
        private IList<CCSprite> _practiceModeImages;
        private int _currentPracticeModeImage;
        private CCAction _practiceModeAction;

        public HelpPage9Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Practice images (note: placing this first so text is on top of images)
            // Scaling assumes 2 : 1 ratio
            var practiceModeImage1 = new CCSprite("Images/Misc/PracticeModeImage1");
            var practiceModeImage2 = new CCSprite("Images/Misc/PracticeModeImage2");
            var practiceModeImage3 = new CCSprite("Images/Misc/PracticeModeImage3");
            var practiceModeImage4 = new CCSprite("Images/Misc/PracticeModeImage4");
            this._practiceModeImages = new List<CCSprite>();
            this._practiceModeImages.Add(practiceModeImage1);
            this._practiceModeImages.Add(practiceModeImage2);
            this._practiceModeImages.Add(practiceModeImage3);
            this._practiceModeImages.Add(practiceModeImage4);
            foreach (var image in this._practiceModeImages)
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
            practiceModeImage1.Opacity = 255;
            this._practiceModeAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                {
                    new CCDelayTime(2 * GameConstants.DURATION_UPGRADE_IMAGE_DISPLAY),
                    new CCCallFunc(() =>
                        {
                            this._practiceModeImages[this._currentPracticeModeImage].RunAction(new CCFadeOut(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
                            if (this._currentPracticeModeImage == this._practiceModeImages.Count - 1)
                            {
                                this._currentPracticeModeImage = 0;
                            }
                            else
                            {
                                this._currentPracticeModeImage++;
                            }
                            this._practiceModeImages[this._currentPracticeModeImage].RunAction(new CCFadeIn(GameConstants.DURATION_UPGRADE_IMAGE_TRANSITION));
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

            // Practice
            var practiceText = string.Empty;
#if ANDROID
            practiceText = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPractice);
#elif IOS
            practiceText = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPractice, Strings.HelpUpgradesPractice);
#else
            practiceText = AppResources.HelpUpgradesPractice;
#endif
            var practiceTitle = new CCLabelTTF(practiceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            practiceTitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            practiceTitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.68f * this.ContentSize.Height);
            this.AddChild(practiceTitle);

            var practiceDesc1Text = string.Empty;
#if ANDROID
            practiceDesc1Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPracticeDesc1);
#elif IOS
            practiceDesc1Text = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPracticeDesc1, Strings.HelpUpgradesPracticeDesc1);
#else
            practiceDesc1Text = AppResources.HelpUpgradesPracticeDesc1;
#endif
            var practiceDesc1 = new CCLabelTTF(practiceDesc1Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            practiceDesc1.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(practiceDesc1);

            var practiceDesc2Text = string.Empty;
#if ANDROID
            practiceDesc2Text = Program.SharedProgram.Resources.GetString(Resource.String.HelpUpgradesPracticeDesc2);
#elif IOS
            practiceDesc2Text = NSBundle.MainBundle.LocalizedString(Strings.HelpUpgradesPracticeDesc2, Strings.HelpUpgradesPracticeDesc2);
#else
            practiceDesc2Text = AppResources.HelpUpgradesPracticeDesc2;
#endif
            var practiceDesc2 = new CCLabelTTF(practiceDesc2Text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            practiceDesc2.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            this.AddChild(practiceDesc2);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            this._currentPracticeModeImage = 0;
            this.RunAction(this._practiceModeAction);
        }

        public override void OnExit()
        {
            base.OnExit();

            this.ActionManager.RemoveAction(this._practiceModeAction);
        }
    }
}