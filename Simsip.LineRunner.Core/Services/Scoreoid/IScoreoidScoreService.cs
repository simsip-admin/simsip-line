using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public interface IScoreoidScoreService
    {
        /// <summary>
        /// Lets you create a user score.
        /// </summary>
        /// <returns></returns>
        Task<bool> CreateScore(ScoreEntity scoreEntity);

        /// <summary>
        /// Lets you create incremental leaderboards.
        /// </summary>
        /// <returns></returns>
        Task IncrementScore();

        /// <summary>
        /// Lets you pull all your game scores.
        /// </summary>
        /// <returns></returns>
        Task<IList<ScoreEntity>> GetScores(PlayerEntity playerEntity);

        /// <summary>
        /// Lets you get all your games best scores.
        /// </summary>
        /// <returns></returns>
        Task<IList<ScoreEntity>> GetBestScores();

        /// <summary>
        /// Lets you count all your game scores.
        /// </summary>
        /// <returns></returns>
        Task<int> CountScores();

        /// <summary>
        /// Lets you count all your game best scores.
        /// </summary>
        /// <returns></returns>
        Task<int> CountBestScores();

        /// <summary>
        /// Lets you get all your game average scores.
        /// </summary>
        /// <returns></returns>
        Task<int> GetAverageScore();
    }
}
