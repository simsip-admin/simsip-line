
namespace Simsip.LineRunner.Scenes
{
    public enum LayerTags
    {
        // IMPORTANT: Need to set the start of the enum to make sure
        // CCLayerMultiplex.SwitchTo will work
        AchievementsLayer=100,
        ActionLayer,
        AdminLayer,
        FinishLayer,
        CreditsMasterLayer,
        HelpMasterLayer,
        HudLayer,
        IntroLayer,
        LinesLayer,
        MessageBoxLayer,
        OptionsMasterLayer,
        PadsLayer,
        RatingsLayer,
        StartLayer,
#if ANDROID || IOS
        UpgradesMasterLayer,
#endif
    }
}