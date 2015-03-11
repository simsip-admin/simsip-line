using SQLite;
using System;


namespace Simsip.LineRunner.Entities.Scoreoid
{
    /// <summary>
    /// This holds scoreboard information about player data.
    /// 
    /// Reference:
    /// http://wiki.scoreoid.net/api/player/getplayerdata/
    /// Returns the value associated with the specified key for the specified game, if key was not found
    /// returns the default value provided (if provided) if no default value was provided returns nil. 
    /// </summary>
    public class PlayerDataEntity
    {
        /// <summary>
        /// The key for a player data entry.
        /// </summary>
        [PrimaryKey]
        public string Key { get; set; }

        /// <summary>
        /// The value for a player data entry.
        /// </summary>
        public string Value { get; set; }
    }
}

