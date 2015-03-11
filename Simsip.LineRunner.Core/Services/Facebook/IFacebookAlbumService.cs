using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    public interface IFacebookAlbumService
    {
        #region Albums

        Task<string> AddAlbumAsync(SimsipUserEntity simsipUser, 
                                   FacebookAlbumEntity album);

        Task<IList<FacebookAlbumEntity>> GetAlbumsAsync(string facebookId);

        Task UpdateAlbumAsync(FacebookAlbumEntity album);

        Task DeleteAlbumAsync(FacebookAlbumEntity album);

        #endregion

        #region Photos

        Task<string> AddPhotoAsync(FacebookAlbumEntity album, 
                                   FacebookPhotoEntity photo);

        Task<IList<FacebookPhotoEntity>> GetPhotosAsync(string albumId); 

        Task UpdatePhotoAsync(FacebookPhotoEntity photo); 

        Task DeletePhotoAsync(FacebookPhotoEntity photo); 
        
        #endregion

        #region Comments

        Task<string> AddCommentAsync(FacebookAlbumEntity album, 
                                     FacebookCommentEntity comment);

        Task<IList<FacebookCommentEntity>> GetCommentsAsync(FacebookAlbumEntity album); 

        // Currently not in API
        // Task UpdateComment(FacebookCommentEntity comment);
        // Task DeleteComment(FacebookCommentEntity comment);

        #endregion

        #region Likes

        Task AddLikeAsync(FacebookAlbumEntity album, 
                          FacebookLikeEntity like);

        Task<IList<FacebookLikeEntity>> GetLikesAsync(FacebookAlbumEntity album);

        Task DeleteLikeAsync(FacebookLikeEntity like);
        
        // Currently not in API
        // Task UpdateLike(LikeModel like);

        #endregion
    }
}
