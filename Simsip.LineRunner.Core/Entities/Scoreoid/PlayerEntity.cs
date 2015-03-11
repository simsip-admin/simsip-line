using SQLite;
using System;


namespace Simsip.LineRunner.Entities.Scoreoid
{
    /// <summary>
    /// This holds scoreboard information about the player.
    /// 
    /// Reference:
    /// http://wiki.scoreoid.net/api/player/getplayer/
    /// username => The players username [String]
    /// password => The players password [String] 
    /// unique_id => The players unique ID [Integer] 
    /// first_name => The players first name [String]  
    /// last_name => The players last name [String] 
    /// email => The players email [String] 
    /// created => The date the player was created calculated by Scoreoid [YYYY-MM-DD hh:mm:ss]
    /// updated => The last time the player was updated calculated by Scoreoid [YYYY-MM-DD hh:mm:ss]
    /// bonus => The players bonus [Integer] 
    /// achievements => The players achievements [String, comma-separated] 
    /// best_score => The players best score calculated by Scoreoid [Integer]
    /// gold => The players gold [Integer] 
    /// money => The players money [Integer] 
    /// kills => The players kills [Integer] 
    /// lives => The players lives [Integer] 
    /// time_played => The time the player played [Integer] 
    /// unlocked_levels => The players unlocked levels [Integer] 
    /// unlocked_items => The players unlocked items [String, comma-separated] 
    /// inventory => The players inventory [String, comma-separated] 
    /// last_level => The players last level [Integer] 
    /// current_level => The players current level [Integer] 
    /// current_time => The players current time [Integer] 
    /// current_bonus => The players current bonus [Integer] 
    /// current_kills => The players current kills [Integer] 
    /// current_achievements => The players current achievements [String, comma-separated] 
    /// current_gold => The players current gold [Integer] 
    /// current_unlocked_levels => The players current unlocked levels [Integer] 
    /// current_unlocked_items => The players current unlocked items [String, comma-separated] 
    /// current_lifes => The players current lifes [Integer] 
    /// xp => The players XP [Integer] 
    /// energy => The players energy [Integer] 
    /// boost => The players energy [Integer] 
    /// latitude => The players GPS latitude [Integer] 
    /// longitude => The players GPS longitude [Integer] 
    /// game_state => The players game state [String]
    /// platform => The players platform needs to match the string value that was used when creating the player [String]
    /// rank => The players current rank [Integer] 
    /// </summary>
    public class PlayerEntity
    {
        /// <summary>
        /// The players unique ID [Integer] 
        /// </summary>
        [PrimaryKey]
        public int UniqueId { get; set; }

        /// <summary>
        /// The players username [String]
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The players password [String] 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The players first name [String]  
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The players last name [String] 
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The players email [String] 
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The date the player was created calculated by Scoreoid [YYYY-MM-DD hh:mm:ss]
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the player was updated calculated by Scoreoid [YYYY-MM-DD hh:mm:ss]
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// The players bonus [Integer] 
        /// </summary>
        public int Bonus { get; set; }

        /// <summary>
        /// The players achievements [String, comma-separated] 
        /// </summary>
        public string Achievements { get; set; }

        /// <summary>
        /// The players best score calculated by Scoreoid [Integer]
        /// </summary>
        public int BestScore { get; set; }

        /// <summary>
        /// The players gold [Integer] 
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// The players money [Integer] 
        /// </summary>
        public int Money { get; set; }

        /// <summary>
        /// The players kills [Integer] 
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// The players lives [Integer] 
        /// </summary>
        public int Lives { get; set; }

        /// <summary>
        /// The time the player played [Integer] 
        /// </summary>
        public int TimePlayed { get; set; }

        /// <summary>
        /// The players unlocked levels [Integer] 
        /// </summary>
        public string UnlockedLevels { get; set; }

        /// <summary>
        /// The players unlocked items [String, comma-separated] 
        /// </summary>
        public string UnlockedItems { get; set; }

        /// <summary>
        /// The players inventory [String, comma-separated] 
        /// </summary>
        public string Inventory { get; set; }

        /// <summary>
        /// The players last level [Integer] 
        /// </summary>
        public string LastLevel { get; set; }

        /// <summary>
        /// The players current level [Integer] 
        /// </summary>
        public string CurrentLevel { get; set; }

        /// <summary>
        /// The players current time [Integer] 
        /// </summary>
        public int CurrentTime { get; set; }

        /// <summary>
        /// The players current bonus [Integer] 
        /// </summary>
        public int CurrentBonus { get; set; }

        /// <summary>
        /// The players current kills [Integer] 
        /// </summary>
        public int CurrentKills { get; set; }

        /// <summary>
        /// The players current achievements [String, comma-separated] 
        /// </summary>
        public string CurrentAchievements { get; set; }

        /// <summary>
        /// The players current gold [Integer] 
        /// </summary>
        public int CurrentGold { get; set; }

        /// <summary>
        /// The players current unlocked levels [Integer] 
        /// </summary>
        public int CurrentUnlockedLevels { get; set; }

        /// <summary>
        /// The players current unlocked items [String, comma-separated] 
        /// </summary>
        public string CurrentUnlockedItems { get; set; }

        /// <summary>
        /// The players current lifes [Integer] 
        /// </summary>
        public int CurrentLives { get; set; }

        /// <summary>
        /// The players XP [Integer] 
        /// </summary>
        public int Xp { get; set; }

        /// <summary>
        /// The players energy [Integer] 
        /// </summary>
        public int Energy { get; set; }

        /// <summary>
        /// The players energy [Integer] 
        /// </summary>
        public int Boost { get; set; }

        /// <summary>
        /// The players GPS latitude [Integer] 
        /// </summary>
        public int Latitude { get; set; }

        /// <summary>
        /// The players GPS longitude [Integer] 
        /// </summary>
        public int Longitude { get; set; }

        /// <summary>
        /// The players game state [String]
        /// </summary>
        public string GameState { get; set; }

        /// <summary>
        /// The players platform needs to match the string value that was used when creating the player [String]
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// The players current rank [Integer] 
        /// </summary>
        public int Rank { get; set; }

    }
}
