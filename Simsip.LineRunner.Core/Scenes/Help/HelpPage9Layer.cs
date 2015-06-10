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

            // Page number
            var pageNumberText = string.Empty;
#if ANDROID
            pageNumberText = Program.SharedProgram.Resources.GetString(Resource.String.CommonPage);
#elif IOS
            pageNumberText = NSBundle.MainBundle.LocalizedString(Strings.CommonPage, Strings.CommonPage);
#else
            pageNumberText = AppResources.CommonPage;
#endif
            var pageNumberHeader = new CCLabelTTF(pageNumberText + " 9", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            pageNumberHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            pageNumberHeader.Position = new CCPoint(
                0.95f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(pageNumberHeader);

            // Practice images (note: placing this first so text is on top of images)
            var practiceModeImage1 = new CCSprite("Models/Pads/Pad1-thumbnail");
            var practiceModeImage2 = new CCSprite("Models/Pads/Pad1-thumbnail");
            var practiceModeImage3 = new CCSprite("Models/Pads/Pad1-thumbnail");
            var practiceModeImage4 = new CCSprite("Models/Pads/Pad1-thumbnail");
            this._practiceModeImages = new List<CCSprite>();
            this._practiceModeImages.Add(practiceModeImage1);
            this._practiceModeImages.Add(practiceModeImage2);
            this._practiceModeImages.Add(practiceModeImage3);
            this._practiceModeImages.Add(practiceModeImage4);
            foreach (var image in this._practiceModeImages)
            {
                image.Opacity = 0;
                image.AnchorPoint = CCPoint.AnchorMiddleBottom;
                image.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    0.1f * this.ContentSize.Height);
                this.AddChild(image);
            }
            practiceModeImage1.Opacity = 255;
            this._practiceModeAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                {
                    new CCDelayTime(GameConstants.DURATION_UPGRADE_IMAGE),
                    new CCCallFunc(() =>
                        {
                            this._practiceModeImages[this._currentPracticeModeImage].RunAction(new CCFadeOut(GameConstants.DURATION_UPGRADE_IMAGE));
                            if (this._currentPracticeModeImage == this._practiceModeImages.Count)
                            {
                                this._currentPracticeModeImage = 0;
                            }
                            else
                            {
                                this._currentPracticeModeImage++;
                            }
                            this._practiceModeImages[this._currentPracticeModeImage].RunAction(new CCFadeIn(GameConstants.DURATION_UPGRADE_IMAGE));
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
            var practiceTitle = new CCLabelTTF(practiceText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            practiceTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
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
                0.4f * this.ContentSize.Height);
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
                0.3f * this.ContentSize.Height);
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