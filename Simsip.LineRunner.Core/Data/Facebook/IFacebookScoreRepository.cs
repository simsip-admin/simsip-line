using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookScoreRepository
    {
        void Create(FacebookScoreEntity score);
        List<FacebookScoreEntity> GetPlayersScores(int skip, int pageSize);
        FacebookScoreEntity GetTopScoreForPlayer();
        List<FacebookScoreEntity> GetTopScoresForPlayer(int count);
        FacebookScoreEntity GetScoreForUserAndAppId(string userId, string appId);
        void Update(FacebookScoreEntity album);
        void Delete(FacebookScoreEntity album);
        int Count { get; }
    }
}
