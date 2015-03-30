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
        public const string Deferred1SceneEffect = @"Content\Effects\Deferred\Deferred1Scene";
        public const string Deferred2LightsEffect = @"Content\Effects\Deferred\Deferred2Lights";
        public const string Deferred3FinalEffect = @"Content\Effects\Deferred\Deferred3Final";
        
        public const string PerlinNoiseEffect = @"Content\Effects\Sky\PerlinNoise";
        public const string SkyDomeEffect = @"Content\Effects\Sky\SkyDome";
        
        public const string WaterEffect = @"Content\Effects\Water\Water";

        public const string ShadowMapEffect = @"Content\Effects\Shadow\ShadowMap";

        public const string BlockEffect = @"Content\Effects\Voxeliq\BlockEffect";
        public const string DualTexturedEffect = @"Content\Effects\Voxeliq\DualTextured";
        public const string BloomExtractEffect = @"Content\Effects\Voxeliq\PostProcessing\Bloom\BloomExtract";
        public const string BloomCombineEffect = @"Content\Effects\Voxeliq\PostProcessing\Bloom\BloomCombine";
        public const string GaussianBlurEffect = @"Content\Effects\Voxeliq\PostProcessing\Bloom\GaussianBlur";
#else
        public const string Deferred1SceneEffect = @"Content/Effects/Deferred/Deferred1Scene";
        public const string Deferred2LightsEffect = @"Content/Effects/Deferred/Deferred2Lights";
        public const string Deferred3FinalEffect = @"Content/Effects/Deferred/Deferred3Final";
        public const string PerlinNoiseEffect = @"Content/Effects/Sky/PerlinNoise";
        public const string SkyDomeEffect = @"Content/Effects/Sky/SkyDome";
        
        public const string WaterEffect = @"Content/Effects/Water/Water";

        public const string ShadowMapEffect = @"Content/Effects/Shadow/ShadowMap";

        public const string BlockEffect = @"Content/Effects/Voxeliq/BlockEffect";
        public const string DualTexturedEffect = @"Content/Effects/Voxeliq/DualTextured";
        public const string BloomExtractEffect = @"Content/Effects/Voxeliq/PostProcessing/Bloom/BloomExtract";
        public const string BloomCombineEffect = @"Content/Effects/Voxeliq/PostProcessing/Bloom/BloomCombine";
        public const string GaussianBlurEffect = @"Content/Effects/Voxeliq/PostProcessing/Bloom/GaussianBlur";
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

        // Fonts are identified by a string that represents the full path (location + texture name)
        // of the texture to load.
        // IMPORTANT: Note how font path is the location of the font without
        // the "Content" directory prefix
        // TODO: When comfortable with textures, categorize as models are done
        public const string VerdanaFont = @"Fonts/Verdana";

        // Sounds are identified by a string that represents the filename of the sound file to load.
        public const string SoundHeroDieing = "sfx_die.mp3";
        public const string SoundHeroHitting = "sfx_hit.mp3";
        public const string SoundHeroPoint = "sfx_point.mp3";
        public const string SoundHeroSwooshing = "sfx_swooshing.mp3";
        public const string SoundHeroWinging = "sfx_wing.mp3";
    }
}