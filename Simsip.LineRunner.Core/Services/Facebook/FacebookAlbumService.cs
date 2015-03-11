using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Facebook
{
    public class FacebookAlbumService : IFacebookAlbumService
    {
        private const string TRACE_TAG = "FacebookAlbumService";

        private readonly IRestService _restService;
        private readonly IFacebookAlbumRepository _albumRepository;
        private readonly IFacebookPhotoRepository _photoRepository;
        private readonly IFacebookCommentRepository _commentRepository;
        private readonly IFacebookLikeRepository _likeRepository;

        public FacebookAlbumService(IRestService restService, 
                            IFacebookAlbumRepository albumRepository,
                            IFacebookPhotoRepository photoRepository,
                            IFacebookCommentRepository commentRepository,
                            IFacebookLikeRepository likeRepository)
        {
            _restService = restService;
            _albumRepository = albumRepository;
            _photoRepository = photoRepository;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
        }

        #region IAlbumService Implementation

        #region Albums

        public async Task<string> AddAlbumAsync(SimsipUserEntity simsipUser,
                                  FacebookAlbumEntity album)
        {
            var response = string.Empty;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["name"] = album.Name;
                parameters["message"] = album.Description;
                parameters["privacy"] = "{\"value\":\"EVERYONE\"}";
                // upload1.Add("privacy", "{\"value\":\"SELF\"}");

                response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                    simsipUser.FacebookId + "/albums",
                                    RestVerbs.Post,
                                    parameters,
                                    OAuthType.Facebook);

                // The new album's facebook id
                album.ServerId = response;
                _albumRepository.Create(album);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook album: " + ex);
            }

            return response;
        }

        public async Task<IList<FacebookAlbumEntity>> GetAlbumsAsync(string facebookUserId)
        {
            var returnAlbums = new List<FacebookAlbumEntity>();

            try
            {
                var fql =
                    "SELECT aid, object_id, owner, name, description, location, link, cover_object_id, cover_pid, photo_count, type, created, modified, can_upload " +
                    "FROM album " +
                    "WHERE owner = " + facebookUserId + " " +
                    "AND substr(name, 0, 3) = 'sim'&";

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                                        "fql?q=" + fql,
                                        RestVerbs.Get,
                                        OAuthType.Facebook);
                
                returnAlbums = ParseGetAlbumsResponse(response);


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook albums: " + ex);
            }

            return returnAlbums;
        }

        public async Task UpdateAlbumAsync(FacebookAlbumEntity album)
        {
            throw new NotImplementedException();
 
            /*
            try
            {
                // We'll have to punt on this for now.
                // In the future we will have to massively:
                // 1. Create new album
                // 2. Sequentially pull down each photo
                // 3. Sequentially post each photo to new album
                // 4. Delete old album

                throw new NotSupportedException();
            }
            catch (Exception ex)
            {
                // TODO: Log

                throw ex;
            }
            */
        }

        public async Task DeleteAlbumAsync(FacebookAlbumEntity album)
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

        #region Photos

        public async Task<string> AddPhotoAsync(FacebookAlbumEntity album, 
                                  FacebookPhotoEntity photo)
        {
            // TODO: Need to represent raw bytes for RawImage
            throw new NotImplementedException();

            var response = string.Empty;

            try
            {
                var parameters = new Dictionary<string, string>();
                // TODO: Need to represent raw bytes for RawImage
                // parameters["source"] = photo.RawImage;
                parameters["message"] = photo.Name;

                response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    album.ServerId + "/photos",
                    RestVerbs.Post,
                    parameters,
                    OAuthType.Facebook);
                    
                // The new photo's facebook id
                photo.ServerId = response;
                _photoRepository.Create(photo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook photo: " + ex);
            }

            return response;
        }

        public async Task<IList<FacebookPhotoEntity>> GetPhotosAsync(string albumId)
        {
            var returnPhotos = new List<FacebookPhotoEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    albumId + "/photos?",
                    RestVerbs.Get,
                    OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnPhotos = ParseGetPhotosResponse(response, albumId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook photos for album: " + ex);
            }

            return returnPhotos;
        }

        public async Task UpdatePhotoAsync(FacebookPhotoEntity photo)
        {
            throw new NotSupportedException();

            /*
            try
            {
                // We'll have to punt on this for now.
                // In the future we will have to:
                // 1. Pull down old photo
                // 2. Post new photo to album id using old photo for content integrated with select pieces from newPhoto
                // 3. Delete old photo

                throw new NotSupportedException();
            }
            catch (Exception ex)
            {
                // TODO: Log

                throw ex;
            }
            */
        }

        public async Task DeletePhotoAsync(FacebookPhotoEntity photo)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                        photo.ServerId,
                        RestVerbs.Delete,
                        OAuthType.Facebook);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting facebook photo: " + ex);
            }
        }

        #endregion

        #region Comments

        public async Task<string> AddCommentAsync(FacebookAlbumEntity album, 
                                    FacebookCommentEntity comment)
        {
            var response = string.Empty;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["message"] = comment.Message;

                response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    album.ServerId + "/comments",
                    RestVerbs.Post,
                    parameters,
                    OAuthType.Facebook);

                // The new comment's facebook id
                comment.ServerId = response;
                comment.ForeignKey = album.ServerId;    // Just being safe
                _commentRepository.Create(comment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook comment to album: " + ex);
            }

            return response;
        }

        public async Task<IList<FacebookCommentEntity>> GetCommentsAsync(FacebookAlbumEntity album)
        {
            var returnComments = new List<FacebookCommentEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    album.ServerId + "/comments",
                    RestVerbs.Get,
                    OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnComments = ParseGetCommentsResponse(response, album.ServerId);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook comments for album: " + ex);
            }

            return returnComments;
        }

        #endregion

        #region Likes

        public async Task AddLikeAsync(FacebookAlbumEntity album,
                                 FacebookLikeEntity like)
        {
            try
            {
                await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    album.ServerId + "/likes",
                    RestVerbs.Post,
                    string.Empty,
                    OAuthType.Facebook);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook like to album: " + ex);
            }
        }

        public async Task<IList<FacebookLikeEntity>> GetLikesAsync(FacebookAlbumEntity album)
        {
            var returnLikes = new List<FacebookLikeEntity>();

            try
            {
                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                    album.ServerId + "/likes",
                    RestVerbs.Get,
                    OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnLikes = ParseGetLikesResponse(response, album.ServerId);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook likes for album: " + ex);
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

        private List<FacebookAlbumEntity> ParseGetAlbumsResponse(string response)
        {
            var returnAlbums = new List<FacebookAlbumEntity>();

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
                        albumModel.ServerId = parsedAlbum["object_id"].Value<string>();
                        albumModel.FromId = parsedAlbum["owner"].Value<string>();
                        albumModel.Name = parsedAlbum["name"] != null ? parsedAlbum["name"].Value<string>() : string.Empty;
                        albumModel.Description = parsedAlbum["description"] != null ? parsedAlbum["description"].Value<string>() : string.Empty;
                        albumModel.Location = parsedAlbum["location"] != null ? parsedAlbum["location"].Value<string>() : string.Empty;
                        albumModel.Link = parsedAlbum["link"] != null ? parsedAlbum["link"].Value<string>() : string.Empty;
                        albumModel.CoverPhoto = parsedAlbum["cover_object_id"] != null ? parsedAlbum["cover_object_id"].Value<string>() : string.Empty;
                        albumModel.Count = parsedAlbum["photo_count"] != null ? parsedAlbum["photo_count"].Value<long>() : 0;
                        albumModel.Type = parsedAlbum["type"] != null ? parsedAlbum["type"].Value<string>() : string.Empty;
                        albumModel.CreatedTime = parsedAlbum["created"] != null ? parsedAlbum["created"].Value<string>() : string.Empty;
                        albumModel.UpdatedTime = parsedAlbum["modified"] != null ? parsedAlbum["modified"].Value<string>() : string.Empty;
                        albumModel.CanUpload = parsedAlbum["can_upload"] != null ? parsedAlbum["can_upload"].Value<bool>() : true;

                        // Now add in the extended properties we've flattened into this model
                        // Reference: https://developers.facebook.com/docs/reference/api/using-pictures/
                        // TODO: Add in a converter to specify size
                        albumModel.CoverPhotoThumbnailUrl = @"https://graph.facebook.com/" + albumModel.ServerId + @"/picture";
         
                        // TODO: previous implementation, remove after one pass
                        // var coverPhotoResponse = MakeRequestSync(albumModel.CoverPhoto, "GET", null);
                        // var parsedcoverPhoto = JObject.Parse(coverPhotoResponse);
                        // albumModel.CoverPhotoThumbnailUrl = parsedCoverPhoto["picture"].Value<string>() + "?width=280&height=280";
                        
                        // Is this a new album?
                        var albumOnDevice = _albumRepository.GetAlbumByServerId(albumModel.ServerId);
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
                        Debug.WriteLine("Exception parsing an individual facebook album: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook albums: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnAlbums;
        }

        private List<FacebookPhotoEntity> ParseGetPhotosResponse(string response, string albumId)
        {
            var returnPhotos = new List<FacebookPhotoEntity>();

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
                        photoModel.Tags = parsedPhoto["tags"] != null ? parsedPhoto["tags"].Value<string>() : string.Empty;
                        photoModel.Name = parsedPhoto["name"] != null ? parsedPhoto["name"].Value<string>() : string.Empty;
                        photoModel.NameTags = parsedPhoto["name_tags"] != null ? parsedPhoto["name_tags"].Value<string>() : string.Empty;
                        photoModel.Icon = parsedPhoto["icon"] != null ? parsedPhoto["icon"].Value<string>() : string.Empty;
                        photoModel.Picture = parsedPhoto["picture"] != null ? parsedPhoto["picture"].Value<string>() : string.Empty;
                        photoModel.Source = parsedPhoto["source"] != null ? parsedPhoto["source"].Value<string>() : string.Empty;
                        photoModel.Height = parsedPhoto["height"] != null ? parsedPhoto["height"].Value<long>() : 0;
                        photoModel.Width = parsedPhoto["width"] != null ? parsedPhoto["width"].Value<long>() : 0;
                        photoModel.Link = parsedPhoto["link"] != null ? parsedPhoto["link"].Value<string>() : string.Empty;
                        photoModel.CreatedTime = parsedPhoto["create_time"] != null ? parsedPhoto["create_time"].Value<string>() : string.Empty;
                        photoModel.UpdatedTime = parsedPhoto["updated_time"] != null ? parsedPhoto["updated_time"].Value<string>() : string.Empty;
                        
                        // Add in our containing album's id
                        photoModel.AlbumId = albumId;

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

                        // Now add in the extended properties we'v flattened into this model
                        // TODO: We will hold off on getting facebook photo 'connections' here as we are
                        // assuming it will be too expensive. The 'connections' will be accessed
                        // when looking at an individual photo
                        // TODO: Or should we just get first 4 connections - for instance?

                        // Is this a new photo?
                        var photoOnDevice = _photoRepository.GetPhotoByServerId(photoModel.ServerId);
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
                        Debug.WriteLine("Exception parsing an individual facebook photo: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook photos: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnPhotos;
        }

        private List<FacebookCommentEntity> ParseGetCommentsResponse(string response, string albumId)
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

        private List<FacebookLikeEntity> ParseGetLikesResponse(string response, string albumId)
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