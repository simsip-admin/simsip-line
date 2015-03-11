using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Facebook
{
    public class FacebookVideoService : IFacebookVideoService
    {
        private const string TRACE_TAG = "FacebookVideoService";

        private readonly IRestService _restService;
        private readonly IFacebookVideoRepository _videoRepository;
        private readonly IFacebookCommentRepository _commentRepository;
        private readonly IFacebookLikeRepository _likeRepository;

        public FacebookVideoService(IRestService restService, 
                                    IFacebookVideoRepository videoRepository,
                                    IFacebookCommentRepository commentRepository,
                                    IFacebookLikeRepository likeRepository)
        {
            _restService = restService;
            _videoRepository = videoRepository;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
        }

        #region IFacebookVideoService Implementation

        #region Videos

        public async Task<string> AddVideoAsync(SimsipUserEntity simsipUser,
                                                FacebookVideoEntity album) 
        {
            // References:
            // https://developers.facebook.com/blog/post/493/
            // https://developers.facebook.com/blog/post/515/#video_upload
            
            throw new NotImplementedException();
        }

        public async Task<IList<FacebookVideoEntity>> GetVideosAsync(string facebookUserId)
        {
            var returnVideos = new List<FacebookVideoEntity>();

            try
            {
                var fql =
                    "SELECT album_id, vid, owner, title, description, thumbnail_link, embed_html, src, created_time, updated_time " +
                    "FROM video " +
                    "WHERE owner = " + facebookUserId + " " +
                    "AND substr(title, 0, 3) = 'sim'&";

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                    "fql?q=" + fql,
                    RestVerbs.Get,
                    OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnVideos = ParseGetVideosResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook videos: " + ex);
            }

            return returnVideos;
        }

        public async Task UpdateVideoAsync(FacebookVideoEntity album)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteVideoAsync(FacebookVideoEntity album)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Comments

        public async Task<string> AddCommentAsync(FacebookVideoEntity video, 
                               FacebookCommentEntity comment)
        {
            var response = string.Empty;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["message"] = comment.Message;

                response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                    video.ServerId + "/comments",
                                    RestVerbs.Post,
                                    parameters,
                                    OAuthType.Facebook);

                // The new comment's facebook id
                comment.ServerId = response;
                comment.ForeignKey = video.ServerId;    // Just being safe
                _commentRepository.Create(comment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook comment to video: " + ex);
            }

            return response;
        }

        public async Task<IList<FacebookCommentEntity>> GetCommentsAsync(FacebookVideoEntity video)
        {
            var returnComments = new List<FacebookCommentEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                        video.ServerId + "/comments",
                                        RestVerbs.Get,
                                        OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnComments = ParseGetCommentsResponse(response, video.ServerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook comments for video: " + ex);
            }

            return returnComments;
        }

        #endregion

        #region Likes

        public async Task AddLikeAsync(FacebookVideoEntity video,
                            FacebookLikeEntity like)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                        video.ServerId + "/likes",
                        RestVerbs.Post,
                        string.Empty,
                        OAuthType.Facebook);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook like to video: " + ex);
            }
        }

        public async Task<IList<FacebookLikeEntity>> GetLikesAsync(FacebookVideoEntity video)
        {
            var returnLikes = new List<FacebookLikeEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                        video.ServerId + "/likes",
                                        RestVerbs.Get,
                                        OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnLikes = ParseGetLikesResponse(response, video.ServerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook likes for video: " + ex);
            }

            return returnLikes;
        }

        public async Task DeleteLikeAsync(FacebookLikeEntity like)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                        like.ForeignKey + "/likes",
                        RestVerbs.Delete,
                        OAuthType.Facebook);

                // TODO: How to evaluate response as boolean?

                _likeRepository.Delete(like);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting facebook like: " + ex);
            }
        }

        #endregion

        #endregion

        #region Helper functions

        private List<FacebookVideoEntity> ParseGetVideosResponse(string response)
        {
            var returnVideos = new List<FacebookVideoEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedVideos = parsedResponse["data"];

                int videoCount = parsedVideos.Count();
                for(int i = 0; i < videoCount; i++)
                {
                     // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedVideo = parsedVideos[i];
                        var videoModel = new FacebookVideoEntity();

                        // Get the simplistic values first
                        videoModel.ServerId = parsedVideo["vid"].Value<string>();
                        videoModel.FromId = parsedVideo["owner"].Value<string>();
                        videoModel.Name = parsedVideo["title"].Value<string>();
                        videoModel.Description = parsedVideo["description"].Value<string>();
                        videoModel.Picture = parsedVideo["thumbnail_link"].Value<string>();
                        videoModel.EmbedHtml = parsedVideo["embed_html"].Value<string>();
                        videoModel.Source = parsedVideo["src"].Value<string>();
                        /* TODO
                        videoModel.CreatedTime = parsedVideo["created_time"].Value<DateTime>();
                        videoModel.UpdatedTime = parsedVideo["updated_time"].Value<DateTime>();
                        */
                        
                        // Is this a new album?
                        var videoOnDevice = _videoRepository.GetVideoByServerId(videoModel.ServerId);
                        if (videoOnDevice == null)
                        {
                            _videoRepository.Create(videoModel);
                        }
                        else
                        {
                            _videoRepository.Update(videoModel);
                        }

                        returnVideos.Add(videoModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual facebook video: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook videos: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnVideos;
        }

        private List<FacebookCommentEntity> ParseGetCommentsResponse(string response, string videoId)
        {
            var returnComments = new List<FacebookCommentEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedComments = parsedResponse["data"];

                int commentCount = parsedComments.Count();
                for(int i = 0; i < commentCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedComment = parsedComments[i];
                        var commentModel = new FacebookCommentEntity();

                        // Get the simplistic values first
                        commentModel.ServerId = parsedComment["id"].Value<string>();
                        commentModel.FromId = parsedComment["from"]["id"].Value<string>();
                        commentModel.Message = parsedComment["message"].Value<string>();
                        commentModel.CreatedTime = parsedComment["created_time"].Value<DateTime>();

                        // Add in our containing album's id
                        commentModel.ForeignKey = videoId;

                        // Now add in the extended properties we'v flattened into this model
                        // TODO: We will hold off on getting facebook comment 'connections' here as we are
                        // assuming it will be too expensive. The 'connections' will be accessed
                        // when looking at an individual comment (e.g., likes)
                        // TODO: Or should we just get first 4 connections - for instance?

                        // Is this a new comment?
                        var commentOnDevice = _commentRepository.ReadByServerId(commentModel.ServerId);
                        if (commentOnDevice == null)
                        {
                            _commentRepository.Create(commentModel);
                        }
                        else
                        {
                            _commentRepository.Update(commentModel);
                        }

                        returnComments.Add(commentModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual facebook comment: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook comment: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnComments;
        }

        private List<FacebookLikeEntity> ParseGetLikesResponse(string response, string videoId)
        {
            var returnLikes = new List<FacebookLikeEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedLikes = parsedResponse["data"];

                int likeCount = parsedLikes.Count();
                for (int i = 0; i < likeCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedLike = parsedLikes[i];
                        var likeModel = new FacebookLikeEntity();

                        // Get the simplistic values first
                        likeModel.ServerId = parsedLike["id"].Value<string>();
                        likeModel.Name = parsedLike["name"].Value<string>();

                        // Add in our containing album's id
                        likeModel.ForeignKey = videoId;

                        // Is this a new like?
                        var likeOnDevice = _likeRepository.ReadByServerId(likeModel.ServerId);
                        if (likeOnDevice == null)
                        {
                            _likeRepository.Create(likeModel);
                        }
                        else
                        {
                            _likeRepository.Update(likeModel);
                        }

                        returnLikes.Add(likeModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual facebook like: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook likes: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnLikes;
        }

        #endregion
    }
}