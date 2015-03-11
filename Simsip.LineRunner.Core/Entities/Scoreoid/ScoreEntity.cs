using SQLite;
using System;


namespace Simsip.LineRunner.Entities.Scoreoid
{
    /// <summary>
    /// This holds scoreboard information about a score.
    /// 
    /// Reference:
    /// http://wiki.scoreoid.net/api/score/getscores/
    /// username => The players username [String]
    /// email => The players email address [String]
    /// first_name => The players first name [String]
    /// last_name => The players last name [String]
    /// platform => The players platform [String]
    /// score => The score [Integer] 
    /// difficulty => The difficulty settings [Integer]  
    /// created => The date the player was created calculated by Scoreoid [YYYY-MM-DD hh:mm:ss]
    /// data => The scores custom data
    /// </summary>
    public class ScoreEntity
    {
        /// <summary>
        /// Unique identifier for score.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ScoreId { get; set; }
        
        /// <summary>
        /// The players username [String]
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The players email address [String]
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The players first name [String]
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The players last name [String]
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The players platform [String]
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// The score [Integer] 
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// The difficulty settings [Integer]  
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        /// The date the player was created calculated by Scoreoid [YYYY-MM-DD hh:mm:ss]
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The scores custom data
        /// </summary>
        public string Data { get; set; }
    }
}

