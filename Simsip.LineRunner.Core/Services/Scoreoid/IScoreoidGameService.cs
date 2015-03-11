using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public interface IScoreoidGameService
    {
        /// <summary>
        /// Pulls all your game information.
        /// </summary>
        /// <returns></returns>
        Task<GameEntity> GetGameAsync();

        /// <summary>
        /// Lets you pull a specific field from your game info.
        /// </summary>
        /// <returns></returns>
        Task<string> GetGameFieldAsync();

        /// <summary>
        /// Lets you get all the players for a specif game.
        /// </summary>
        /// <returns></returns>
        Task<IList<PlayerEntity>> GetPlayersAsync();
    
        /// <summary>
        /// Gets the top value for the following game field’s best_score, bonus, gold, money, kills, lifes, time_played and unlocked_levels.
        /// </summary>
        /// <returns></returns>
        Task<int> GetGameTopAsync();

        /// <summary>
        /// Gets the average for the following game field’s best_score, bonus, gold, money, kills, lifes, time_played and unlocked_levels.
        /// </summary>
        /// <returns></returns>
        Task<int> GetGameAverageAsync();

        /// <summary>
        /// Gets the lowest vaule for the following game field’s bonus, gold, money, kills, lifes, time_played and unlocked_levels.
        /// </summary>
        /// <returns></returns>
        Task<int> GetGameLowestAsync();
    }
}
