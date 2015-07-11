using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;


namespace Engine.Assets
{
    public class Asset
    {
        // Models are identified by a string that represents the name of the model.
        // Upon loading via IAssetManager.GetModel, an enum is passed in to identify
        // the model type and, in turn, the location to pull the model from.
        /* TODO: These are .x models that were not compiling with the new Pipeline tool
        public const string AimedBlockModel = "AimedBlock";
        public const string SkyDomeModel = "SkyDome";
        */

        // Effects are identitfied by a string that represents the full path (location + effect name)
        // to load the effect from.
        // IMPORTANT: Note how effect path is the location of the effect including the
        // "Content" directory prefix.
        // ALSO: The suffix ".mgfxo" will be added programatically when loading.
        // TODO: When comfortable with effect, categorize as models are done
#if NETFX_CORE
        public const string Deferred1SceneEffect  = @"Content\Effects\Deferred\Deferred1Scene.mgfxo";
        public const string Deferred2LightsEffect = @"Content\Effects\Deferred\Deferred2Lights.mgfxo";
        public const string Deferred3FinalEffect  = @"Content\Effects\Deferred\Deferred3Final.mgfxo";
        public const string StockBasicEffect      = @"Content\Effects\Stock\StockBasicEffect.mgfxo";
        public const string PerlinNoiseEffect     = @"Content\Effects\Sky\PerlinNoise.mgfxo";
        public const string SkyDomeEffect         = @"Content\Effects\Sky\SkyDome.mgfxo";
        public const string WaterEffect           = @"Content\Effects\Water\Water.mgfxo";
        public const string ShadowMapEffect       = @"Content\Effects\Shadow\ShadowMap.mgfxo";
        public const string BlockEffect           = @"Content\Effects\Voxeliq\BlockEffect.mgfxo";
        public const string DualTexturedEffect    = @"Content\Effects\Voxeliq\DualTextured.mgfxo";
        public const string BloomExtractEffect    = @"Content\Effects\Voxeliq\PostProcessing\Bloom\BloomExtract.mgfxo";
        public const string BloomCombineEffect    = @"Content\Effects\Voxeliq\PostProcessing\Bloom\BloomCombine.mgfxo";
        public const string GaussianBlurEffect    = @"Content\Effects\Voxeliq\PostProcessing\Bloom\GaussianBlur.mgfxo";
#else
        public const string Deferred1SceneEffect  = @"Content/Effects/Deferred/Deferred1Scene.mgfxo";
        public const string Deferred2LightsEffect = @"Content/Effects/Deferred/Deferred2Lights.mgfxo";
        public const string Deferred3FinalEffect  = @"Content/Effects/Deferred/Deferred3Final.mgfxo";
        public const string StockBasicEffect      = @"Content/Effects/Stock/StockBasicEffect.mgfxo";
        public const string PerlinNoiseEffect     = @"Content/Effects/Sky/PerlinNoise.mgfxo";
        public const string SkyDomeEffect         = @"Content/Effects/Sky/SkyDome.mgfxo";
        public const string WaterEffect           = @"Content/Effects/Water/Water.mgfxo";
        public const string ShadowMapEffect       = @"Content/Effects/Shadow/ShadowMap.mgfxo";
        public const string BlockEffect           = @"Content/Effects/Voxeliq/BlockEffect.mgfxo";
        public const string DualTexturedEffect    = @"Content/Effects/Voxeliq/DualTextured.mgfxo";
        public const string BloomExtractEffect    = @"Content/Effects/Voxeliq/PostProcessing/Bloom/BloomExtract.mgfxo";
        public const string BloomCombineEffect    = @"Content/Effects/Voxeliq/PostProcessing/Bloom/BloomCombine.mgfxo";
        public const string GaussianBlurEffect    = @"Content/Effects/Voxeliq/PostProcessing/Bloom/GaussianBlur.mgfxo";
#endif
        // Textures are identified by a string that represents the full path (location + texture name)
        // of the texture to load.
        // IMPORTANT: Note how texture path is the location of the texture without
        // the "Content" directory prefix
        // TODO: When comfortable with textures, categorize as models are done
        public const string CrackTextureAtlas = @"Images/Textures/Cracks";
        public const string AimedBlockTexture = @"Images/Textures/AimedBlock";
        public const string CrossHairNormalTexture = @"Images/Textures/NormalCrosshairs";
        public const string CrossHairShovelTexture = @"Images/Textures/ShovelCrosshairs";
        public const string CloudTexture = @"Images/Textures/CloudTexture";
        public const string CloudMapTexture = @"Images/Maps/CloudMap";
        public const string StarMapTexture = @"Images/Maps/StarMap";

        // IMPORTANT: This reference can vary as different resource packs are chosen
        public static string BlockTextureAtlas = 
            @"ResourcePacks/" +
            UserDefaults.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_RESOURCE_PACK,
                GameConstants.USER_DEFAULT_INITIAL_CURRENT_RESOURCE_PACK) +
            @"/Terrain";

        // Sounds are identified by a string that represents the filename of the sound file to load.
        public const string SoundHeroDieing = "sfx_die.mp3";
        public const string SoundHeroHitting = "sfx_hit.mp3";
        public const string SoundHeroPoint = "sfx_point.mp3";
        public const string SoundHeroSwooshing = "sfx_swooshing.mp3";
        public const string SoundHeroWinging = "sfx_wing.mp3";

        public const string SoundBeep1 = "beep-1.wav";
        public const string SoundBeep2 = "beep-2.wav";
        public const string SoundBeep3 = "beep-3.wav";
        public const string SoundBeep4 = "beep-4.wav";
        public const string SoundBeep5 = "beep-5.wav";
        public const string SoundBeep6 = "beep-6.wav";
        public const string SoundBeep7 = "beep-7.wav";
        public const string SoundScribble1 = "Pencil_quick_scribble_1.wav";
        public const string SoundScribble2 = "Pencil_quick_scribble_2.wav";
        public const string SoundScribble3 = "Pencil_quick_scribble_3.wav";
        public const string SoundSound10 = "sound10.mp3";
        public const string SoundSound12 = "sound12.mp3";
        public const string SoundSound35 = "sound35.mp3";
        public const string SoundSound46 = "sound46.mp3";
        public const string SoundSound88 = "sound88.mp3";

    }
}