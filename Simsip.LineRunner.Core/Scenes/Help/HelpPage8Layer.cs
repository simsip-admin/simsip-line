using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Help
{
    public class HelpPage8Layer : GameLayer
    {
        private CoreScene _parent;
        private HelpMasterLayer _masterLayer;

        public HelpPage8Layer(CoreScene parent, HelpMasterLayer masterLayer)
        {
            this._parent = parent;
            this._masterLayer = masterLayer;

            // Get this setup for relative positioning
            this.ContentSize = this._masterLayer.ContentSize;

            // Options sub-title
            var optionsText = string.Empty;
#if ANDROID
            optionsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpOptions);
#elif IOS
            optionsText = NSBundle.MainBundle.LocalizedString(Strings.HelpOptions, Strings.HelpOptions);
#else
            optionsText = AppResources.HelpOptions;
#endif
            var optionsSubtitle = new CCLabelTTF(optionsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            optionsSubtitle.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            optionsSubtitle.Position = new CCPoint(
                0.5f  * this.ContentSize.Width,
                0.75f * this.ContentSize.Height);
            this.AddChild(optionsSubtitle);

            // Sound
            var soundImage = new CCSprite("Images/Icons/SoundButtonOn.png");
            soundImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            soundImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.55f * this.ContentSize.Height);
            this.AddChild(soundImage);
            var soundText = string.Empty;
#if ANDROID
            soundText = Program.SharedProgram.Resources.GetString(Resource.String.HelpSound);
#elif IOS
            soundText = NSBundle.MainBundle.LocalizedString(Strings.HelpSound, Strings.HelpSound);
#else
            soundText = AppResources.HelpSound;
#endif
            var soundDescription = new CCLabelTTF(soundText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            soundDescription.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(soundDescription);

            // Leaderboards
            var leaderboardsImage = new CCSprite("Images/Icons/AchievementsButtonNormal.pn");
            leaderboardsImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
            leaderboardsImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(leaderboardsImage);
            var leaderboardsText = string.Empty;
#if ANDROID
            leaderboardsText = Program.SharedProgram.Resources.GetString(Resource.String.HelpLeaderboards);
#elif IOS
            leaderboardsText = NSBundle.MainBundle.LocalizedString(Strings.HelpLeaderboards, Strings.HelpLeaderboards);
#else
            leaderboardsText = AppResources.HelpLeaderboards;
#endif
            var leaderboardsDescription = new CCLabelTTF(leaderboardsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            leaderboardsDescription.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.25f * this.ContentSize.Height);
            this.AddChild(leaderboardsDescription);
        }
    }
}