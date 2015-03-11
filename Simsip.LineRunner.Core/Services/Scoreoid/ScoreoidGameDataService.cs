using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using Simsip.LineRunner.Entities.Scoreoid;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Scoreoid
{
    /// <summary>
    /// Stubbed out service implementation.
    /// </summary>
    public class ScoreoidGameDataService : IScoreoidGameDataService
    {

        private readonly IRestService _restService;

        public ScoreoidGameDataService()
        {
            _restService = new RestService();
        }

        #region IScoreoidGameDataService Implementation

        public async Task<GameDataEntity> GetGameDataAsync(string key)
        {
            throw new NotImplementedException();
        }

        public async Task SetGameDataAsync(GameDataEntity gameData)
        {
            throw new NotImplementedException();
        }

        public async Task UnsetGameDataAsync(GameDataEntity gameData)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper functions

        private GameEntity ParseGetGameResponse(string response)
        {
            /* TODO
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

            return returnAlbums;
            */

            return null;
        }

        #endregion
    }
}