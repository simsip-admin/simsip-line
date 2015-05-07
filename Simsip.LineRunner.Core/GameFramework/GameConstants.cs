using Cocos2D;


namespace Simsip.LineRunner.GameFramework
{
    public class GameConstants
    {
        // Folders
        public const string FOLDER_CONFIG = "config";
        public const string FOLDER_DATA = "data";
        public const string FOLDER_LOGS = "logs";
        public const string FOLDER_RESOURCE_PACKS = "resourcepacks";
        public const string FOLDER_SAVES = "saves";
        public const string FOLDER_SHADER_PACKS = "shaderpacks";

        // Font settings
        public const string FONT_FAMILY_NORMAL = "Schwarzwald Regular";
        public const float FONT_SIZE_SMALL = 18f;
        public const float FONT_SIZE_NORMAL = 26f;
        public const float FONT_SIZE_LARGE = 36f;
        
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
        public const float DURATION_MOVE_TO_NEXT_PAGE = 4.0f;

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
    }
}