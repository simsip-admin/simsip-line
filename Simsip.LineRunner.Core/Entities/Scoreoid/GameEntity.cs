using SQLite;
using System;


namespace Simsip.LineRunner.Entities.Scoreoid
{
    /// <summary>
    /// This holds scoreboard information about the game.
    /// 
    /// Reference:
    /// http://wiki.scoreoid.net/api/game/getgame/
    /// user_id => Your user id [String]
    /// name => Your game name [String]
    /// short_description => Your game's short description [String]
    /// description => Your game's full description [String]
    /// game_type => Your game type [String]
    /// version => Current version of your game [Integer] 
    /// levels => Number of game levels [Integer] 
    /// platform => Current platform you game is on [String]
    /// play_url => Your game's play/download URL [String]
    /// website_url => Your game's website URL [String]
    /// players_count => The number of players [Integer]
    /// scores_count => The number of scores [Integer]
    /// status => Current game status [JSON Bool (0/1), XML Bool (true/false)] 
    /// created => Date your game was created [Date String, format: YYYY-MM-DD hh:mm:ss] 
    /// updated => Last date your game was updated [Date String, format: YYYY-MM-DD hh:mm:ss] 
    /// </summary>
    public class GameEntity
    {
        /// <summary>
        ///  Your Game ID
        /// </summary>
        [PrimaryKey]
        public string GameId { get; set; }

        /// <summary>
        /// Your user id [String]
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Your game name [String]
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Your game's short description [String]
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Your game's full description [String]
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Your game type [String]
        /// </summary>
        public string GameType { get; set; }

        /// <summary>
        /// Current version of your game [Integer] 
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Number of game levels [Integer] 
        /// </summary>
        public int Levels { get; set; }

        /// <summary>
        /// Current platform you game is on [String]
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Your game's play/download URL [String]
        /// </summary>
        public string PlayUrl { get; set; }

        /// <summary>
        /// Your game's website URL [String]
        /// </summary>
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// The number of players [Integer]
        /// </summary>
        public int PlayersCount { get; set; }

        /// <summary>
        /// The number of scores [Integer]
        /// </summary>
        public int ScoresCount { get; set; }

        /// <summary>
        /// Current game status [JSON Bool (0/1), XML Bool (true/false)] 
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Date your game was created [Date String, format: YYYY-MM-DD hh:mm:ss] 
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Last date your game was updated [Date String, format: YYYY-MM-DD hh:mm:ss] 
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}