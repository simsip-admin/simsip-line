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
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Facebook
{
    /// <summary>
    /// Reference: https://developers.facebook.com/docs/opengraph/using-actions/
    /// </summary>
    public class FacebookActionService : IFacebookActionService
    {
        private const string TRACE_TAG = "ActionService";

        private readonly IRestService _restService;
        private readonly IFacebookActionRepository _actionRepository;

        public FacebookActionService(IRestService restService, 
                                     IFacebookActionRepository actionRepository)
        {
            _restService = restService;
            _actionRepository = actionRepository;
        }

        #region IActionService Implementation

        #region Actions

        public async Task<string> AddAction(FacebookUserEntity user,
                                            FacebookActionEntity action)
        {
            var response = string.Empty;

            try
            {
                var parameters = BuildAddActionParameters(action);
                    
                response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    user.ServerId + "/" + 
                    action.ActionType + "?" +
                    parameters,
                    RestVerbs.Post,
                    string.Empty,
                    OAuthType.Facebook);

                // The new action's facebook id
                action.ServerId = response;
                _actionRepository.Create(action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook action: " + ex);
            }

            return response;
        }

        public async Task<IList<FacebookActionEntity>> GetActions(FacebookUserEntity user)
        {
            var returnActions = new List<FacebookActionEntity>();
            
            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                        user.ServerId + "/actions",
                                        RestVerbs.Get,
                                        OAuthType.Facebook);

                returnActions = ParseGetActionsResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook actions: " + ex);
            }

            return returnActions;
        }

        public async Task UpdateAction(FacebookActionEntity action)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAction(FacebookActionEntity album)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                        album.ServerId,
                        RestVerbs.Delete,
                        OAuthType.Facebook);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting facebook album: " + ex);
            }
        }

        #endregion

        #endregion

        #region Helper functions

        private string BuildAddActionParameters(FacebookActionEntity action)
        {
            var parameters = string.Empty;
            parameters += action.ObjectType + "=" + Uri.EscapeUriString(action.ObjectValue) + "&";
            if (action.StartTime != null)
            {
                // TODO: Date conversion
                parameters += "start_time=" + Uri.EscapeUriString(action.StartTime.ToString()) + "&";
            }
            if (action.EndTime != null)
            {
                // TODO: Date conversion
                parameters += "end_time=" + Uri.EscapeUriString(action.EndTime.ToString()) + "&";
            }
            if (action.ExpiresIn != 0)
            {
                parameters += "expires_in=" + Uri.EscapeUriString(action.ExpiresIn.ToString()) + "&";
            }
            if (action.Notify != true)
            {
                parameters += "notify=" + Uri.EscapeUriString(action.Notify.ToString()) + "&";
            }
            if (!string.IsNullOrEmpty(action.Privacy))
            {
                // TODO: How to parametize the json representation for privacy?
                parameters += "privacy=" + Uri.EscapeUriString(action.Privacy) + "&";
            }
            if (!string.IsNullOrEmpty(action.Image))
            {
                parameters += "image=" + Uri.EscapeUriString(action.Image) + "&";
            }
            if (!string.IsNullOrEmpty(action.Ref))
            {
                parameters += "ref=" + Uri.EscapeUriString(action.Ref) + "&";
            }
            if (action.Scrape != false)
            {
                parameters += "scrape=" + Uri.EscapeUriString(action.Scrape.ToString()) + "&";
            }
            if (action.NoFeedStory != false)
            {
                parameters += "no_feed_story=" + Uri.EscapeUriString(action.NoFeedStory.ToString()) + "&";
            }
            if (!string.IsNullOrEmpty(action.Message))
            {
                parameters += "message=" + Uri.EscapeUriString(action.Message) + "&";
            }
            if (action.ExplicitlyShared != false)
            {
                parameters += "fb:explicitly_shared=" + Uri.EscapeUriString(action.ExplicitlyShared.ToString()) + "&";
            }
            if (!string.IsNullOrEmpty(action.Place))
            {
                // TODO: How to parametize the json representation for place?
                parameters += "place=" + Uri.EscapeUriString(action.Place) + "&";
            }
            if (!string.IsNullOrEmpty(action.Tags))
            {
                // TODO: How to interpret comma separated list ids?
                parameters += "tags=" + Uri.EscapeUriString(action.Tags) + "&";
            }
            
            // Remove last "&"
            parameters = parameters.Substring(0, parameters.Length - 1);

            return parameters;
        }

        private List<FacebookActionEntity> ParseGetActionsResponse(string response)
        {
            
            var returnActions = new List<FacebookActionEntity>();

            /* TODO
            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedAlbums = parsedResponse["data"];

                int albumCount = parsedAlbums.Count();
                for(int i = 0; i < albumCount; i++)
                {
                     // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedAlbum = parsedAlbums[i];
                        var albumModel = new FacebookAlbumEntity();

                        // Get the simplistic values first
                        albumModel.ServerId = parsedAlbum["id"].Value<string>();
                        albumModel.FromId = parsedAlbum["from"]["id"].Value<string>();
                        albumModel.FromName = parsedAlbum["from"]["name"].Value<string>();
                        albumModel.Name = parsedAlbum["name"].Value<string>();
                        albumModel.Description = parsedAlbum["description"].Value<string>();
                        albumModel.Location = parsedAlbum["location"].Value<string>();
                        albumModel.Link = parsedAlbum["link"].Value<string>();
                        albumModel.CoverPhoto = parsedAlbum["cover_photo"].Value<string>();
                        albumModel.Privacy = parsedAlbum["privacy"].Value<string>();
                        albumModel.Count = parsedAlbum["count"].Value<long>();
                        albumModel.Type = parsedAlbum["type"].Value<string>();
                        albumModel.CreatedTime = parsedAlbum["created_time"].Value<string>();
                        albumModel.UpdatedTime = parsedAlbum["updated_time"].Value<string>();
                        albumModel.CanUpload = parsedAlbum["can_upload"].Value<bool>();

                        // Now add in the extended properties we've flattened into this model
                        // Reference: https://developers.facebook.com/docs/reference/api/using-pictures/
                        // TODO: Add in a converter to specify size
                        albumModel.CoverPhotoThumbnailUrl = albumModel.Link + "/picture";
         
                        // TODO: previous implementation, remove after one pass
                        // var coverPhotoResponse = MakeRequestSync(albumModel.CoverPhoto, "GET", null);
                        // var parsedcoverPhoto = JObject.Parse(coverPhotoResponse);
                        // albumModel.CoverPhotoThumbnailUrl = parsedCoverPhoto["picture"].Value<string>() + "?width=280&height=280";
                        
                        // Is this a new album?
                        var albumOnDevice = _albumRepository.ReadByServerId(albumModel.ServerId);
                        if (albumOnDevice == null)
                        {
                            _albumRepository.Create(albumModel);
                        }
                        else
                        {
                            _albumRepository.Update(albumModel);
                        }

                        returnAlbums.Add(albumModel);
                    }
                    catch (Exception ex)
                    {
                        Mvx.TaggedError(TRACE_TAG, "Exception parsing an individual facebook album: " + ex, new object[] {});
                    }
                }
            }
            catch (Exception ex)
            {
                Mvx.TaggedError(TRACE_TAG, "Exception parsing facebook albums: " + ex, new object[] {});

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }
            */

            return returnActions;
        }

        private List<FacebookPhotoEntity> ParseGetPhotosResponse(string response, string albumId)
        {
            var returnPhotos = new List<FacebookPhotoEntity>();

            /* TODO
            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedPhotos = parsedResponse["data"];

                int photoCount = parsedPhotos.Count();
                for(int i = 0; i < photoCount; i++)
                {
                     // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedPhoto = parsedPhotos[i];
                        var photoModel = new FacebookPhotoEntity();

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
                        
                        // Add in our containing album's id
                        photoModel.AlbumId = albumId;

                        // TODO: We'll punt on this for now
                        // photoModel.Images = new List<PhotoImage>();
                        // foreach (dynamic image in photo.images)
                        // {
                        //     var photoImageModel = new PhotoImage();
                        //     photoImageModel.Height = image.height;
                        //     photoImageModel.Width = image.width;
                        //     photoImageModel.Source = image.source;
                        //     photoModel.Images.Add(photoImageModel);
                        // }
                                                
                        // This is nullable
                        // photoModel.Position = photo.position;

                        // Now add in the extended properties we'v flattened into this model
                        // TODO: We will hold off on getting facebook photo 'connections' here as we are
                        // assuming it will be too expensive. The 'connections' will be accessed
                        // when looking at an individual photo
                        // TODO: Or should we just get first 4 connections - for instance?

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

                        returnPhotos.Add(photoModel);
                    }
                    catch (Exception ex)
                    {
                        Mvx.TaggedError(TRACE_TAG, "Exception parsing an individual facebook photo: " + ex, new object[] {});
                    }
                }
            }
            catch (Exception ex)
            {
                Mvx.TaggedError(TRACE_TAG, "Exception parsing facebook photos: " + ex, new object[] {});

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }
            */

            return returnPhotos;
        }

        private List<FacebookCommentEntity> ParseGetCommentsResponse(string response, string albumId)
        {
            var returnComments = new List<FacebookCommentEntity>();

            /* TODO
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
                        commentModel.ForeignKey = albumId;

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
                        Mvx.TaggedError(TRACE_TAG, "Exception parsing an individual facebook comment: " + ex, new object[] {});
                    }
                }
            }
            catch (Exception ex)
            {
                Mvx.TaggedError(TRACE_TAG, "Exception parsing facebook comment: " + ex, new object[] {});

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }
            */

            return returnComments;
        }

        private List<FacebookLikeEntity> ParseGetLikesResponse(string response, string albumId)
        {
            var returnLikes = new List<FacebookLikeEntity>();

            /* TODO
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
                        likeModel.ForeignKey = albumId;

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
                        Mvx.TaggedError(TRACE_TAG, "Exception parsing an individual facebook like: " + ex, new object[] { });
                    }
                }
            }
            catch (Exception ex)
            {
                Mvx.TaggedError(TRACE_TAG, "Exception parsing facebook likes: " + ex, new object[] { });

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }
            */

            return returnLikes;
        }

        #endregion
    }
}