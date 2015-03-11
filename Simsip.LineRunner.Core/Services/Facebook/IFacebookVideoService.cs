using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    public interface IFacebookVideoService
    {
        #region Videos

        Task<string> AddVideoAsync(SimsipUserEntity simsipUser, 
                                   FacebookVideoEntity album);

        Task<IList<FacebookVideoEntity>> GetVideosAsync(string userId);

        Task UpdateVideoAsync(FacebookVideoEntity album);

        Task DeleteVideoAsync(FacebookVideoEntity album);

        #endregion

        #region Comments

        Task<string> AddCommentAsync(FacebookVideoEntity video, 
                                     FacebookCommentEntity comment);

        Task<IList<FacebookCommentEntity>> GetCommentsAsync(FacebookVideoEntity video);

        // Currently not in API
        // Task UpdateComment(FacebookCommentEntity comment);
        // Task DeleteComment(FacebookCommentEntity comment);

        #endregion

        #region Likes

        Task AddLikeAsync(FacebookVideoEntity video, 
                          FacebookLikeEntity like);

        Task<IList<FacebookLikeEntity>> GetLikesAsync(FacebookVideoEntity video);

        Task DeleteLikeAsync(FacebookLikeEntity like);
        
        // Currently not in API
        // Task UpdateLike(LikeModel like);

        #endregion
    }
}
