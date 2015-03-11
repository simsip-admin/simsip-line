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
    public class FacebookSubscriptionService : IFacebookSubscriptionService
    {
        private const string TRACE_TAG = "FacebookSubscriptionService";

        private const string FqlAlbumsJsonName = "fqlAlbums";
        private const string FqlVideosJsonName = "fqlVideos";

        private readonly IRestService _restService;
        private readonly IFacebookAlbumRepository _albumRepository;
        private readonly IFacebookVideoRepository _videoRepository;

        public FacebookSubscriptionService(IRestService restService, 
                                    IFacebookAlbumRepository albumRepository,
                                    IFacebookVideoRepository videoRepository)
        {
            _restService = restService;
            _albumRepository = albumRepository;
            _videoRepository = videoRepository;
        }

        #region IFacebookSubscriptionService Implementation

        #region Subscriptions

        public async Task<string> AddSubscriptionAsync(SimsipUserEntity simsipUser,
                             FacebookSubscriptionEntity subscription)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<FacebookSubscriptionEntity>> GetSubscriptionsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<FacebookHybridEntity>> GetSubscriptionHybridsAsync(IList<FacebookSubscriptionEntity> subscriptions)
        {
            var returnSubscriptions = new List<FacebookHybridEntity>();

            try
            {
                // Create SELECT substring for subscribed_id
                var subscribedIdSelect = string.Empty;
                foreach(var subscription in subscriptions)
                {
                    subscribedIdSelect += subscription.SubscribedId + ",";
                }
                if (subscribedIdSelect.EndsWith(","))
                {
                    subscribedIdSelect = subscribedIdSelect.Substring(0, subscribedIdSelect.Length - 1);
                }

                var fqlAlbums =
                    "SELECT aid, object_id, owner, name, description, location, link, cover_object_id, cover_pid, photo_count,  type, created, modified, can_upload " +
                    "FROM album " +
                    "WHERE owner IN (" + subscribedIdSelect + ") " +
                    "AND substr(name, 0, 3) = 'sim'";

                var fqlVideos =
                    "SELECT album_id, vid, owner, title, description, thumbnail_link, embed_html, src, created_time, updated_time " +
                    "FROM video " +
                    "WHERE owner IN (" + subscribedIdSelect + ") " +
                    "AND substr(title, 0, 3) = 'sim'";

                var body = new JObject();
                body.Add(FqlAlbumsJsonName, fqlAlbums);
                body.Add(FqlVideosJsonName, fqlVideos);
                
                // IMPORTANT: Need to use Uri.EscapeDataString to encode all special characters including '#'
                var fqlSubscriptions = Uri.EscapeDataString(body.ToString());

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                                        "fql?q=" + fqlSubscriptions + "&",
                                        RestVerbs.Get,
                                        OAuthType.Facebook);

                returnSubscriptions = ParseGetSubscriptionHybridsResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook subcription hybrids: " + ex);
            }

            return returnSubscriptions;
        }

        public async Task UpdateSubscriptionAsync(FacebookSubscriptionEntity album)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteSubscriptionAsync(FacebookSubscriptionEntity album)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Helper functions

        private List<FacebookHybridEntity> ParseGetSubscriptionHybridsResponse(string response)
        {
            // IMPORTANT: See comment at end of function for example on how the json looks

            var returnHybrids = new List<FacebookHybridEntity>();
            var responseHybrids = new List<FacebookHybridEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedData = parsedResponse["data"];
                int dataCount = parsedData.Count();
                for (int i = 0; i < dataCount; i++)
                {
                    var parsedDatum = parsedData[i];
                    var name = parsedDatum["name"].Value<string>(); 
                    if (name == FqlAlbumsJsonName)
                    {
                        ParseGetAlbumsResponse(parsedDatum["fql_result_set"], responseHybrids);
                    }
                    else if (name == FqlVideosJsonName)
                    {
                        ParseGetVideosResponse(parsedDatum["fql_result_set"], responseHybrids);
                    }
                }

                returnHybrids = responseHybrids.OrderByDescending(x => x.UpdatedTime)
                                               .ToList<FacebookHybridEntity>();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook hybrids: " + ex);

                throw ex;
            }

            return returnHybrids;

/* Json reference for this request:
{
  "data": [
    {
      "name": "fqlAlbums",
      "fql_result_set": [
        {
          "aid": "100005107653734_1073741827",
          "object_id": 208367002676887,
          "owner": 100005107653734,
          "name": "simmc night-wmc2",
          "description": "",
          "location": "",
          "link": "https://www.facebook.com/album.php?fbid=208367002676887&id=100005107653734&aid=1073741827",
          "cover_object_id": "208367012676886",
          "cover_pid": "100005107653734_1073741870",
          "photo_count": 13,
          "type": "normal",
          "created": 1383423895,
          "modified": 1383423916,
          "can_upload": false
        }
      ]
    },
    {
      "name": "fqlVideos",
      "fql_result_set": [
        {
          "album_id": null,
          "vid": 208371439343110,
          "owner": 100005107653734,
          "title": "simmc halloween1-wmc2",
          "description": "",
          "thumbnail_link": "https://fbcdn-vthumb-a.akamaihd.net/hvthumb-ak-ash3/t15/1095248_208374552676132_208371439343110_56618_528_t.jpg",
          "embed_html": "<iframe src=\"https://www.facebook.com/video/embed?video_id=208371439343110\" width=\"1280\" height=\"720\" frameborder=\"0\"></iframe>",
          "src": "https://fbcdn-video-a.akamaihd.net/hvideo-ak-prn2/v/t42/1244541_208374499342804_1047670232_n.mp4?oh=44fcdd1c3d7028dc3ee25bfbe63114d2&oe=52EB5789&__gda__=1391130515_54ff13cb248660072489191f5c1cd660",
          "created_time": 1383425877,
          "updated_time": 1391001739
        }
      ]
    }
  ]
}
*/
        }

        // TODO: Should this be centralized from AlbumService?
        private void ParseGetAlbumsResponse(JToken parsedAlbums, IList<FacebookHybridEntity> returnHybrids)
        {
            try
            {
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

                        var hybridModel = new FacebookHybridEntity() 
                        {
                            ServerId = albumModel.ServerId,
                            Type = FacebookHybridEntity.AlbumType,
                            FromId = albumModel.FromId,
                            Name = albumModel.Name,
                            Description = albumModel.Description,
                            CoverPhotoThumbnailUrl = albumModel.CoverPhotoThumbnailUrl,
                            Count = albumModel.Count,
                            UpdatedTime = albumModel.UpdatedTime
                        };
                        
                        returnHybrids.Add(hybridModel);
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
        }

        // TODO: Should this be centralized with facebook video service?
        private void ParseGetVideosResponse(JToken parsedVideos, IList<FacebookHybridEntity> returnHybrids)
        {
            try
            {
                int videoCount = parsedVideos.Count();
                for (int i = 0; i < videoCount; i++)
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
                        videoModel.CreatedTime = parsedVideo["created_time"].Value<string>();
                        videoModel.UpdatedTime = parsedVideo["updated_time"].Value<string>();

                        // Is this a new video?
                        var videoOnDevice = _videoRepository.GetVideoByServerId(videoModel.ServerId);
                        if (videoOnDevice == null)
                        {
                            _videoRepository.Create(videoModel);
                        }
                        else
                        {
                            _videoRepository.Update(videoModel);
                        }

                        var hybridModel = new FacebookHybridEntity()
                        {
                            ServerId = videoModel.ServerId,
                            Type = FacebookHybridEntity.VideoType,
                            FromId = videoModel.FromId,
                            Name = videoModel.Name,
                            Description = videoModel.Description,
                            CoverPhotoThumbnailUrl = videoModel.Picture,
                            Source = videoModel.Source,
                            UpdatedTime = videoModel.UpdatedTime
                        };

                        returnHybrids.Add(hybridModel);

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
        }

        #endregion
    }
}