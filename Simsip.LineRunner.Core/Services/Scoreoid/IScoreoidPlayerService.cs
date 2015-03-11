using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public interface IScoreoidPlayerService
    {
        /// <summary>
        /// Lets you create a player with a number of optional values.
        /// </summary>
        /// <returns></returns>
        Task<bool> CreatePlayer(PlayerEntity playerEntity);

        /// <summary>
        /// Check if player exists and returns the player information. Post parameters work as query conditions. 
        /// 
        /// This method can be used for login by adding username and password parameters.
        /// </summary>
        /// <returns></returns>
        Task<PlayerEntity> GetPlayer(PlayerEntity playerEntity);

        /// <summary>
        /// Lets you edit your player information.
        /// </summary>
        /// <returns></returns>
        Task EditPlayer();

        /// <summary>
        /// Deletes a player and all the players scores.
        /// </summary>
        /// <returns></returns>
        Task DeletePlayer();

        /// <summary>
        /// Lets you pull a specific field from your player info.
        /// </summary>
        /// <returns></returns>
        Task<string> GetPlayerField();

        /// <summary>
        /// Lets you update your player field’s.
        /// </summary>
        /// <returns></returns>
        Task UpdatePlayerField();

        /// <summary>
        /// Lets you pull all the scores for a player.
        /// </summary>
        /// <returns></returns>
        Task<IList<ScoreEntity>> GetPlayerScores();

        /// <summary>
        /// Returns the player’s rank based on the parmeters set.
        /// </summary>
        /// <returns></returns>
        Task<string> GetPlayerRank();
        
        /// <summary>
        /// Lets you count all your players.
        /// </summary>
        /// <returns></returns>
        Task<int> CountPlayers();
    }
}
