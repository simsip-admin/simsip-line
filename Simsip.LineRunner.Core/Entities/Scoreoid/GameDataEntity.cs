using SQLite;
using System;


namespace Simsip.LineRunner.Entities.Scoreoid
{
    /// <summary>
    /// This holds scoreboard information about game data.
    /// 
    /// Reference:
    /// http://wiki.scoreoid.net/api/game/getgamedata/
    /// Returns the value associated with the specified key for the specified game, 
    /// if key was not found returns the default value provided (if provided) 
    /// if no default value was provided returns nil. 
    /// </summary>
    public class GameDataEntity
    {
        /// <summary>
        /// The key for a game data entry.
        /// </summary>
        [PrimaryKey]
        public string Key { get; set; }
        
        /// <summary>
        /// The value for a game data entry.
        /// </summary>
        public string Value { get; set; }
    }
}

