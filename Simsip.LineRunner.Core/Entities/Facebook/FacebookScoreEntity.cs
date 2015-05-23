using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    /// <summary>
    /// Querying without the read_stream permission will return only the public view 
    /// of the data (i.e. data that can be see when the user is logged out).
    /// </summary>
    public class FacebookScoreEntity
    {
        #region Data model

        /// <summary>
        /// The id created on the device to identify this score
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ScoreId { get; set; }

        /// <summary>
        /// Id of the player associated with the score
        /// </summary>
        [Indexed]
        public string PlayerId { get; set; }

        /// <summary>
        /// Name of the player associated with the score
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Numeric score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Elapsed time for score
        /// </summary>
        public TimeSpan ScoreTime { get; set; }

        /// <summary>
        /// App id associated with the score
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// App name associated with the score
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Type of the data which is score
        /// </summary>
        public string Type { get; set; }

        #endregion
    }
}
