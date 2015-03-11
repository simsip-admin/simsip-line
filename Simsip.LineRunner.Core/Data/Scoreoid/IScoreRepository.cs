using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public interface IScoreRepository
    {
        void Create(ScoreEntity score);

        List<ScoreEntity> GetTopScores(int count);

        ScoreEntity GetScore(int scoreId);
        ScoreEntity GetScoreByScore(int score);
        int GetHighScore();

        void Update(ScoreEntity score);
        void Delete(ScoreEntity score);
        int Count { get; }
    }
}
