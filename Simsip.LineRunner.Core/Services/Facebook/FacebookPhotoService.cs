using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;

namespace Simsip.LineRunner.Services.Facebook
{
    public class FacebookPhotoService : IFacebookPhotoService
    {
        private const string TRACE_TAG = "PhotoService";

        private readonly IRestService _restService;
        private readonly IFacebookPhotoRepository _photoRepository;
        private readonly IFacebookCommentRepository _commentRepository;
        private readonly IFacebookLikeRepository _likeRepository;
        
        public FacebookPhotoService(IRestService restService, 
                            IFacebookPhotoRepository photoRepository,
                            IFacebookCommentRepository commentRepository,
                            IFacebookLikeRepository likeRepository)
        {
            _restService = restService;
            _photoRepository = photoRepository;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
        }

        #region IPhotoService implementation

        #region Photos

        public async Task<FacebookPhotoEntity> GetPhotoAsync(FacebookPhotoEntity photo)
        {
            FacebookPhotoEntity returnPhoto = null;

            try
            {

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                    photo.ServerId,
                    RestVerbs.Get,
                    OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnPhoto = ParseGetPhotoResponse(response);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook photos: " + ex);
            }

            return returnPhoto;
        }

        #endregion

        #region Comments

        public async Task<string> AddCommentAsync(FacebookPhotoEntity photo, 
                                            FacebookCommentEntity comment)
        {
            var response = string.Empty;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["message"] = comment.Message;

                response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                    photo.ServerId + "/comments",
                    RestVerbs.Post,
                    parameters,
                    OAuthType.Facebook);

                // The new comment's facebook id
                comment.ServerId = response;
                comment.ForeignKey = photo.ServerId;    // Just being safe
                _commentRepository.Create(comment);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook comment to photo: " + ex);
            }

            return response;
        }

        public async Task<IList<FacebookCommentEntity>> GetCommentsAsync(FacebookPhotoEntity photo)
        {
            var returnComments = new List<FacebookCommentEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    photo.ServerId + "/comments",
                    RestVerbs.Get,
                    OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnComments = ParseGetCommentsResponse(response, photo.ServerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook comments for photo: " + ex);
            }

            return returnComments;
        }

        #endregion

        #region Likes

        public async Task AddLikeAsync(FacebookPhotoEntity photo)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                        photo.ServerId + "/likes",
                        RestVerbs.Post,
                        string.Empty,
                        OAuthType.Facebook);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook like to photo: " + ex);
            }
        }

        public async Task<IList<FacebookLikeEntity>> GetLikesAsync(FacebookPhotoEntity photo)
        {
            var returnLikes = new List<FacebookLikeEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                        photo.ServerId + "/likes",
                                        RestVerbs.Get,
                                        OAuthType.Facebook);
                            
                // TODO: Analyze http status code here before parsing

                returnLikes = ParseGetLikesResponse(response, photo.ServerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook likes for photo: " + ex);
            }

            return returnLikes;
        }

        public async Task DeleteLikeAsync(FacebookLikeEntity like)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                        like.ServerId,
                        RestVerbs.Delete,
                        OAuthType.Facebook);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting facebook like: " + ex);
            }
        }

        #endregion

        #endregion

        #region Helper methods

        private FacebookPhotoEntity ParseGetPhotoResponse(string response)
        {
            var photoModel = new FacebookPhotoEntity();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedPhoto = parsedResponse["data"];

                // TODO: Can we get album id too, for instance, user has moved
                // photo from one album to another

                // Get the simplistic values first
                photoModel.ServerId = parsedPhoto["id"].Value<string>();
                photoModel.FromId = parsedPhoto["from"]["id"].Value<string>();
                photoModel.FromName = parsedPhoto["from"]["name"].Value<string>();
                photoModel.Tags = parsedPhoto["tags"].Value<string>();
                photoModel.Name = parsedPhoto["name"].Value<string>();
                photoModel.NameTags = parsedPhoto["name_tags"].Value<string>();
                photoModel.Icon = parsedPhoto["icon"].Value<string>();
                photoModel.Picture = parsedPhoto["picture"].Value<string>();
                photoModel.Source = parsedPhoto["source"].Value<string>();
                photoModel.Height = parsedPhoto["height"].Value<long>();
                photoModel.Width = parsedPhoto["width"].Value<long>();
                photoModel.Link = parsedPhoto["link"].Value<string>();
                // photoModel.Place = photo.place;
                photoModel.CreatedTime = parsedPhoto["create_time"].Value<string>();
                photoModel.UpdatedTime = parsedPhoto["updated_time"].Value<string>();

                /* TODO: We'll punt on this for now
                photoModel.Images = new List<PhotoImage>();
                foreach (dynamic image in photo.images)
                {
                    var photoImageModel = new PhotoImage();
                    photoImageModel.Height = image.height;
                    photoImageModel.Width = image.width;
                    photoImageModel.Source = image.source;
                    photoModel.Images.Add(photoImageModel);
                }
                */

                // This is nullable
                // photoModel.Position = photo.position;

                // Now add in the extended properties we've flattened into this model
                // TODO: We will hold off on getting facebook photo 'connections' here as we are
                // assuming it will be too expensive. The 'connections' will be accessed
                // when looking at an individual photo
                // TODO: Or should we just get first 4 connections - for instance?

                // TODO: How can we do this without the album id?
                /*
                // Is this a new photo?
                var photoOnDevice = _photoRepository.ReadByServerId(photoModel.ServerId);
                if (photoOnDevice == null)
                {
                    _photoRepository.Create(photoModel);
                }
                else
                {
                    _photoRepository.Update(photoModel);
                }
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing an individual facebook photo: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return photoModel;
        }

        private List<FacebookCommentEntity> ParseGetCommentsResponse(string response, string photoId)
        {
            var returnComments = new List<FacebookCommentEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedComments = parsedResponse["data"];

                int commentCount = parsedComments.Count();
                for (int i = 0; i < commentCount; i++)
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

                        // Add in our containing photo's id
                        commentModel.ForeignKey = photoId;

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
                Debug.WriteLine("Exception parsing facebook photos: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnComments;
        }

        private List<FacebookLikeEntity> ParseGetLikesResponse(string response, string photoId)
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

                        // Add in our containing photo's id
                        likeModel.ForeignKey = photoId;

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