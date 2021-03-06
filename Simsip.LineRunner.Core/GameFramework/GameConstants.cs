using Cocos2D;


namespace Simsip.LineRunner.GameFramework
{
    public class GameConstants
    {
        // Magic numbers
        public const float DEFAULT_PAGE_DEPTH_FROM_CAMERA = 4f;
        public const float DEFAULT_CHARACTER_DEPTH_FROM_PAGE = 0.4f;
        public const float DEFAULT_PANE_DEPTH_FROM_PAGE = 2f;

        // Magic strings
        public const string RandomPrefix = "Random";

        // Folders
        public const string FOLDER_CONFIG = "config";
        public const string FOLDER_DATA = "data";
        public const string FOLDER_LOGS = "logs";
        public const string FOLDER_RESOURCE_PACKS = "resourcepacks";
        public const string FOLDER_SAVES = "saves";
        public const string FOLDER_SHADER_PACKS = "shaderpacks";

        // Font settings
#if ANDROID
        public const string DEBUG_FONT = @"Fonts/arial-core-16";     // To be removed
#elif DESKTOP
        public const string DEBUG_FONT = @"Fonts/arial-core-18";    // To be removed
#elif IOS
        public const string DEBUG_FONT = @"Fonts/arial-core-16";    // To be removed
#elif WINDOWS_PHONE
        public const string DEBUG_FONT = @"Fonts/arial-core-18";    // To be removed
#elif  NETFX_CORE
        public const string DEBUG_FONT = @"Fonts/arial-core-18";    // To be removed
#endif
        public const string FONT_FAMILY_NORMAL = "arial-core";
#if ANDROID
        public const float FONT_SIZE_SMALL = 16f;
        public const float FONT_SIZE_NORMAL = 16f;
        public const float FONT_SIZE_LARGE = 16f;
        public const float FONT_SIZE_X_LARGE = 16f;
        public const float FONT_SIZE_SMALL_SCALE = 1f;      // 16
        public const float FONT_SIZE_NORMAL_SCALE = 1.5f;   // 24
        public const float FONT_SIZE_LARGE_SCALE = 2f;      // 32
        public const float FONT_SIZE_X_LARGE_SCALE = 3f;    // 48
#elif DESKTOP
        public const float FONT_SIZE_SMALL = 18f;
        public const float FONT_SIZE_NORMAL = 24f;
        public const float FONT_SIZE_LARGE = 24f;
        public const float FONT_SIZE_X_LARGE = 24f;
        public const float FONT_SIZE_SMALL_SCALE = 1f;      // 16
        public const float FONT_SIZE_NORMAL_SCALE = 1f;     // 24
        public const float FONT_SIZE_LARGE_SCALE = 1.3f;    // 32
        public const float FONT_SIZE_X_LARGE_SCALE = 2f;    // 48
#elif IOS
        public const float FONT_SIZE_SMALL = 16f;
        public const float FONT_SIZE_NORMAL = 16f;
        public const float FONT_SIZE_LARGE = 16f;
        public const float FONT_SIZE_X_LARGE = 16f;
        public const float FONT_SIZE_SMALL_SCALE = 1f;      // 16
        public const float FONT_SIZE_NORMAL_SCALE = 1.5f;   // 24
        public const float FONT_SIZE_LARGE_SCALE = 2f;      // 32
        public const float FONT_SIZE_X_LARGE_SCALE = 3f;    // 48
#elif WINDOWS_PHONE
        public const float FONT_SIZE_SMALL = 18f;
        public const float FONT_SIZE_NORMAL = 24f;
        public const float FONT_SIZE_LARGE = 24f;
        public const float FONT_SIZE_X_LARGE = 24f;
        public const float FONT_SIZE_SMALL_SCALE = 1f;      // 16
        public const float FONT_SIZE_NORMAL_SCALE = 1f;     // 24
        public const float FONT_SIZE_LARGE_SCALE = 1.3f;    // 32
        public const float FONT_SIZE_X_LARGE_SCALE = 2f;    // 48
#elif NETFX_CORE
        public const float FONT_SIZE_SMALL = 18f;
        public const float FONT_SIZE_NORMAL = 24f;
        public const float FONT_SIZE_LARGE = 24f;
        public const float FONT_SIZE_X_LARGE = 24f;
        public const float FONT_SIZE_SMALL_SCALE = 1f;      // 16
        public const float FONT_SIZE_NORMAL_SCALE = 1f;     // 24
        public const float FONT_SIZE_LARGE_SCALE = 1.3f;    // 32
        public const float FONT_SIZE_X_LARGE_SCALE = 2f;    // 48
#endif

        // Worlds
        public const string WORLD_DEFAULT = "default";
        public const string WORLD_GENERATE_NEW = "generate-new";

        // User Defaults
        public const string USER_DEFAULT_KEY_CURRENT_WORLD = "CurrentWorld";
        public const string USER_DEFAULT_INITIAL_CURRENT_WORLD = "default";
        
        public const string USER_DEFAULT_KEY_CURRENT_RESOURCE_PACK = "CurrentResourcePack";
        public const string USER_DEFAULT_INITIAL_CURRENT_RESOURCE_PACK = "Default";
        
        public const string USER_DEFAULT_KEY_CURRENT_PAD = "CurrentPad";
        public const string USER_DEFAULT_INITIAL_CURRENT_PAD = "Pad1";
        
        public const string USER_DEFAULT_KEY_CURRENT_LINE = "CurrentLine";
        public const string USER_DEFAULT_INITIAL_CURRENT_LINE = "Line1";

        public const string USER_DEFAULT_KEY_INSTALL_DATE = "InstallDate";

        public const string USER_DEFAULT_KEY_GAMES_PLAYED_COUNT = "GamesPlayedCount";

        public const string USER_DEFAULT_KEY_ASK_FOR_RATING = "AskForRating";

        public const string USER_DEFAULT_KEY_HERO_LINEAR_VELOCITY_X = "HeroLinearVelocityY";
        public const int USER_DEFAULT_INITIAL_HERO_LINEAR_VELOCITY_X = 10;

        // These have been fine-tuned to give us an interesting 3D perspective to start from
        public const string USER_DEFAULT_KEY_HUD_OFFSET_X = "HudOffsetX";
        public const float USER_DEFAULT_INITIAL_HUD_OFFSET_X = 0f;
        public const string USER_DEFAULT_KEY_HUD_OFFSET_Y = "HudOffsetY";
        public const float USER_DEFAULT_INITIAL_HUD_OFFSET_Y = 0f;
        public const string USER_DEFAULT_KEY_HUD_OFFSET_YAW = "HudOffsetYaw";
        public const float USER_DEFAULT_INITIAL_HUD_OFFSET_YAW = -0.1742601f;
        public const string USER_DEFAULT_KEY_HUD_OFFSET_PITCH = "HudOffsetPitch";
        public const float USER_DEFAULT_INITIAL_HUD_OFFSET_PITCH = 0.04563053f;
        public const string USER_DEFAULT_KEY_HUD_ORBIT_YAW = "HudOrbitYaw";
        public const float USER_DEFAULT_INITIAL_HUD_ORBIT_YAW = -0.7829438f;
        public const string USER_DEFAULT_KEY_HUD_ORBIT_PITCH = "HudOrbitPitch";
        public const float USER_DEFAULT_INITIAL_HUD_ORBIT_PITCH = -0.1659292f;
        public const string USER_DEFAULT_KEY_HUD_ZOOM = "HudZoom";
        public const float USER_DEFAULT_INITIAL_HUD_ZOOM = 2.800001f;
            // Leaving this in here to represent what it originally was:
            // GameConstants.DEFAULT_PAGE_DEPTH_FROM_CAMERA - GameConstants.DEFAULT_CHARACTER_DEPTH_FROM_PAGE;

        // Upgrades
        public const string USER_DEFAULT_KEY_START_PAGE = "StartPage";
        public const int USER_DEFAULT_INITIAL_START_PAGE = 1;
        public const string USER_DEFAULT_KEY_START_LINE = "StartLine";
        public const int USER_DEFAULT_INITIAL_START_LINE = 1;
        public const string USER_DEFAULT_KEY_KILLS_ALLOWED = "KillsAllowed";
        public const bool USER_DEFAULT_INITIAL_KILLS_ALLOWED = true;

        // Limits
        public const int VELOCITY_LOWER_LIMIT_X = 1;
        public const float HUD_JOYSTICK_LIMIT_X = 1.0f;
        public const float HUD_JOYSTICK_LIMIT_Y = 1.0f;

        // Ratings prompt window (how long before we qualify to prompt user to rate us)
        public const int RATING_WINDOW_IN_DAYS = 10;

        /// <summary>
        /// Determines if background music is played.
        /// 
        /// IMPORTANT: Currently not implementd.
        /// </summary>
        public const string USER_DEFAULT_KEY_MUSIC = "Music";
        public const bool USER_DEFAULT_INITIAL_MUSIC = true;

        /// <summary>
        /// Determines if sound effects are played for game events
        /// (hitting obstacles, moving in game, moving to next line, etc.).
        /// </summary>
        public const string USER_DEFAULT_KEY_SOUND = "Sound";
        public const bool USER_DEFAULT_INITIAL_SOUND = true;

        /// <summary>
        /// Controls whether user is in practice mode.
        /// 
        /// Practice mode allows users to specify which page to start on. In practice mode, users
        /// cannot post scores to leaderboard.
        /// </summary>
        public const string USER_DEFAULT_KEY_PRACTICE_MODE = "PracticeMode";
        public const bool USER_DEFAULT_INITIAL_PRACTICE_MODE = false;

        /// <summary>
        /// When in practice mode, designates the page the user has selected to start on.
        /// </summary>
        public const string USER_DEFAULT_KEY_PRACTICE_MODE_START_PAGE = "PracticeModeStartPage";
        public const int USER_DEFAULT_INITIAL_PRACTICE_MODE_START_PAGE = 1;

        
        /// <summary>
        /// Ratio to convert width from pixels to logical coordinates.
        /// </summary>
        public static float PixelToLogicalScaleWidth = CCDirector.SharedDirector.WinSize.Width / CCDrawManager.FrameSize.Width;

        /// <summary>
        /// Ratio to convert height from pixels to logical coordinates.
        /// </summary>
        public static float PixelToLogicalScaleHeight = CCDirector.SharedDirector.WinSize.Height / CCDrawManager.FrameSize.Height;

        /// <summary>
        /// Duration in seconds to for our intro.
        /// 
        /// Used by flyby, positioning first set of models, etc.
        /// </summary>
        public const float DURATION_INTRO = 16.0f;

        /// <summary>
        /// Duration in seconds to for our intro to fade in from black.
        /// </summary>
        public const float DURATION_INTRO_FADE_IN = DURATION_INTRO / 2f;

        /// <summary>
        /// Duration in seconds for moving to next line.
        /// 
        /// Used by flyby, positioning next set of models, etc.
        /// </summary>
        public const float DURATION_MOVE_TO_NEXT_LINE = 4.0f;

        /// <summary>
        /// Duration in seconds for moving to next page.
        /// 
        /// Used by flyby, positioning new set of models, etc.
        /// </summary>
        public const float DURATION_MOVE_TO_NEXT_PAGE = 8.0f;

        /// <summary>
        /// Duration in seconds for layer transitions.
        /// 
        /// Used displaying menus, ui layer transition in/out, etc.
        /// </summary>
        public const float DURATION_LAYER_TRANSITION = 0.5f;

        /// <summary>
        /// Duration in seconds for particle display.
        /// 
        /// Used by particle system to limit display of a particle effect to a defined duration.
        /// </summary>
        public const float DURATION_PARTICLE_DURATION = 2.0f;

        /// <summary>
        /// Duration in seconds for obstacle to glow when hit.
        /// </summary>
        public const float DURATION_OBSTACLE_GLOW = 0.25f;

        /// <summary>
        /// Duration in seconds to display hero being killed.
        /// </summary>
        public const float DURATION_HERO_KILL = 2.0f;

        /// <summary>
        /// Duration in seconds for fast animation of an obstacle.
        /// 
        /// Used to animate obstacle's X position, height or angle.
        /// </summary>
        public const float DURATION_ANIMATE_OBSTACLE_FAST = 2.0f;

        /// <summary>
        /// Duration in seconds for medium animation of an obstacle.
        /// 
        /// Used to animate obstacle's X position, height or angle.
        /// </summary>
        public const float DURATION_ANIMATE_OBSTACLE_MEDIUM = 4.0f;

        /// <summary>
        /// Duration in seconds for slow animation of an obstacle.
        /// 
        /// Used to animate obstacle's X position, height or angle.
        /// </summary>
        public const float DURATION_ANIMATE_OBSTACLE_SLOW = 8.0f;

        /// <summary>
        /// How long we display a status message for 
        /// </summary>
        public const float DURATION_STATUS_LABEL = 2.0f;

        /// <summary>
        /// How long we display an image for an upgrade on an upgrades page, before fading to the next image for the upgrade.
        /// </summary>
        public const float DURATION_UPGRADE_IMAGE_DISPLAY = 6.0f;

        /// <summary>
        /// How long we transition an image for an upgrade on an upgrades page, before fading to the next image for the upgrade.
        /// </summary>
        public const float DURATION_UPGRADE_IMAGE_TRANSITION = 2.0f;

    }
}