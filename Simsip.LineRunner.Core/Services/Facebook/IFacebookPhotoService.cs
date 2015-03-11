using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    public interface IFacebookPhotoService
    {
        #region Photos

        Task<FacebookPhotoEntity> GetPhotoAsync(FacebookPhotoEntity photo);
        
        #endregion

        #region Comments

        Task<string> AddCommentAsync(FacebookPhotoEntity photo, 
                                     FacebookCommentEntity comment);

        Task<IList<FacebookCommentEntity>> GetCommentsAsync(FacebookPhotoEntity photo);

        // Currently not in API
        // Task UpdateComment(SimsipUser simsipUser, string albumId, string oldCommentId, string newMessage);
        // Task DeleteComment(SimsipUser simsipUser, string commentId);

        #endregion

        #region Likes

        Task AddLikeAsync(FacebookPhotoEntity photo); 

        Task<IList<FacebookLikeEntity>> GetLikesAsync(FacebookPhotoEntity photoId);

        Task DeleteLikeAsync(FacebookLikeEntity photoId);
        
        // Currently not in API
        // Task UpdateLike(SimsipUser simsipUser, string albumId, string likeId);

        #endregion
    }
}
