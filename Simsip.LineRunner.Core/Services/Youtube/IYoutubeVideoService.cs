using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Services.Youtube
{
    interface IYoutubeVideoService
    {
        #region Comments

        // Reference:
        // https://developers.google.com/youtube/2.0/developers_guide_protocol_comments

        Task AddComment(SimsipUserEntity user,
                        YoutubeVideoEntity video, 
                        YoutubeCommentEntity comment);

        Task<IList<YoutubeCommentEntity>> GetComments(YoutubeVideoEntity video);

        Task AddCommentInReplyTo(SimsipUserEntity user,
                                 YoutubeVideoEntity video,
                                 YoutubeCommentEntity originalComment,
                                 YoutubeCommentEntity inReplyToComment);

        Task DeleteComment(SimsipUserEntity user,
                           YoutubeCommentEntity comment);

        #endregion

        #region Likes
        
        // Reference:
        // https://developers.google.com/youtube/2.0/developers_guide_protocol_ratings

        Task AddRating(SimsipUserEntity user,
                       YoutubeVideoEntity video, 
                       bool like);

        #endregion

        #region Video Responses
        
        // Reference
        // https://developers.google.com/youtube/2.0/developers_guide_protocol_video_responses

        Task<IList<YoutubeVideoEntity>>  GetVideoResponses(YoutubeVideoEntity video);

        #endregion
    }
}
