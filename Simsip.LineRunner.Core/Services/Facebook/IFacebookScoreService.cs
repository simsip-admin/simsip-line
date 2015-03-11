using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    public interface IFacebookScoreService
    {
        Task<bool> AddScoreAsync(FacebookScoreEntity score);

        Task<IList<FacebookScoreEntity>> GetScoresForPlayerAsync();

        // TODO
        Task<IList<FacebookScoreEntity>> GetScoresForAllPlayersAsync();

        Task<bool> UpdateScoreAsync(SimsipUserEntity simsipUser, FacebookScoreEntity score);
    }
}
