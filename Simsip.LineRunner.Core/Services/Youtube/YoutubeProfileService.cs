using System;
using System.Diagnostics;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Entities.Youtube;
using Simsip.LineRunner.Services.OAuth;
using Simsip.LineRunner.Services.Rest;
using Simsip.LineRunner.Data.Youtube;
using Simsip.LineRunner.Entities.OAuth;

namespace Simsip.LineRunner.Services.Youtube
{
    public class YoutubeProfileService : IYoutubeProfileService
    {
        private const string TRACE_TAG = "YoutubeProfileService";

        private readonly IRestService _restService;
        private readonly IYoutubeProfileRepository _profileRepository;
        private readonly IYoutubeVideoRepository _videoRepository;

        public YoutubeProfileService(IRestService restService, 
                                     IYoutubeProfileRepository profileRepository,
                                     IYoutubeVideoRepository videoRepository)
        {
            _restService = restService;
            _profileRepository = profileRepository;
            _videoRepository = videoRepository;
        }

        #region IProfileService implementation

        /// <summary>
        /// Retrieve a youtube user's profile.
        /// 
        /// References:
        /// https://developers.google.com/youtube/2.0/reference#User_profile_entry
        /// https://developers.google.com/youtube/2.0/reference#youtube_data_api_tag_entry
        /// </summary>
        /// <param name="simsipUser">A SimsipUser model representing the user you want to get the
        /// youtube profile for.</param>
        /// <returns>A youtube Profile model.</returns>
        /// 
        public async Task<YoutubeProfileEntity> GetProfile()
        {
            YoutubeProfileEntity returnUser = null;

            try
            {
                var response = await _restService.MakeRequestAsync(YoutubeServiceUrls.ProfileUrl,
                    RestVerbs.Get,
                    OAuthType.Youtube);

                // TODO: Analyze http status code here before parsing

                returnUser = ParseGetUserResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting youtube user: " + ex);
            }

            return returnUser;
        }

        public async Task<YoutubeProfileEntity> GetProfileByUserId()
        {
            throw new NotImplementedException();
        }


        public async Task<IList<YoutubeVideoEntity>> GetVideos(YoutubeProfileEntity profile)
        {
            // TODO: Duplicate implementation with VideoService.GetVideoResponses()

            var returnVideos = new List<YoutubeVideoEntity>();

            try
            {
                // So far, it looks like we can just use the link w/o need for access token/key
                // to get the raw atom feed
                var response = await _restService.MakeRequestAsync(profile.UploadedFeedLinkHref,
                    RestVerbs.Get,
                    OAuthType.None);

                // TODO: Analyze http status code here before parsing

                returnVideos = ParseGetVideosResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting youtube videos: " + ex);
            }

            return returnVideos;
        }

        #endregion

        #region Helper methods

        private YoutubeProfileEntity ParseGetUserResponse(string response)
        {
            var returnProfile = new YoutubeProfileEntity();

            try
            {
                // Parse the raw atom feed
                var byteArray = Encoding.UTF8.GetBytes(response);
                var stream = new MemoryStream(byteArray);
                var profile = XElement.Load(stream);

                // We'll need these namespaces for parsing
                XNamespace def = "http://www.w3.org/2005/Atom";
                XNamespace yt = "http://gdata.youtube.com/schemas/2007";
                XNamespace gd = "http://schemas.google.com/g/2005";
                XNamespace media = "http://search.yahoo.com/mrss/";

                // Carefully extract out the values, note the "?" annotations indicating optional elements
                XElement id = profile.Element(def + "id");
                XElement published = profile.Element(def + "published");                // ?
                XElement updated = profile.Element(def + "updated");
                XElement title = profile.Element(def + "title");
                XElement firstName = profile.Element(yt + "firstName");                 // ?
                XElement lastName = profile.Element(yt + "lastName");                   // ?
                XElement userId = profile.Element(yt + "userId");

                XAttribute userNameDisplay = null;
                XElement userName = profile.Element(yt + "username");                   // ?
                if (userName != null)
                {
                    userNameDisplay = userName.Attribute("display");                    // ?
                }
                else
                {
                    // TODO: Log 
                }

                XElement channelId = profile.Element(yt + "channelId");

                XElement authorName = null;
                XElement authorUri = null;
                XElement authorUserId = null;
                XElement author = profile.Element(def + "author");
                if (author != null)
                {
                    authorName = author.Element(def + "name");
                    authorUri = author.Element(def + "uri");
                    authorUserId = author.Element(yt + "userId");
                }
                else
                {
                    // TODO: Log 
                }

                XAttribute viewCount = null;
                XAttribute videoWatchCount = null;
                XAttribute subscriberCount = null;
                XAttribute lastWebAccess = null;
                XAttribute totalUploadViews = null;
                XElement statistics = profile.Element(yt + "statistics");
                if (statistics != null)
                {
                    viewCount = statistics.Attribute("viewCount");
                    videoWatchCount = statistics.Attribute("videoWatchCount");
                    subscriberCount = statistics.Attribute("subscriberCount");
                    lastWebAccess = statistics.Attribute("lastWebAccess");
                    totalUploadViews = statistics.Attribute("totalUploadViews");
                }
                else
                {
                    // TODO: Log
                }

                XAttribute uploadedFeedLinkRel = null;
                XAttribute uploadedFeedLinkHref = null;
                XAttribute uploadedFeedLinkCountHint = null;
                string relUploads = "http://gdata.youtube.com/schemas/2007#user.uploads";
                XElement feedLink = (from link in profile.Elements(gd + "feedLink")
                                     where relUploads == (string)link.Attribute("rel")
                                     select link).FirstOrDefault();
                if (feedLink != null)
                {
                    uploadedFeedLinkRel = feedLink.Attribute("rel");
                    uploadedFeedLinkHref = feedLink.Attribute("href");
                    uploadedFeedLinkCountHint = feedLink.Attribute("countHint");
                }
                else
                {
                    // TODO: Log
                }

                XAttribute thumbnail = null;
                XElement mediaThumbnail = profile.Element(media + "thumbnail");
                if (mediaThumbnail != null)
                {
                    thumbnail = mediaThumbnail.Attribute("url");
                }
                else
                {
                    // TODO: Log
                }

                // Now, carefully populate our data model
                returnProfile.ServerId = (string)id;
                returnProfile.Published = published != null ? (string)published : null;
                returnProfile.Updated = updated != null ? (string)updated : null;
                returnProfile.Title = title != null ? (string)title : null;
                returnProfile.FirstName = firstName != null ? (string)firstName : null;
                returnProfile.LastName = lastName != null ? (string)lastName : null;
                returnProfile.UserId = userId != null ? (string)userId : null;
                returnProfile.UserName = userName != null ? (string)userName : null;

                returnProfile.UserNameDisplay = userNameDisplay != null ? (string)userNameDisplay : null;
                returnProfile.ChannelId = channelId != null ? (string)channelId : null;
                returnProfile.AuthorName = authorName != null ? (string)authorName : null;
                returnProfile.AuthorUri = authorUri != null ? (string)authorUri : null;
                returnProfile.AuthorUserId = authorUserId != null ? (string)authorUserId : null;
                returnProfile.ViewCount = viewCount != null ? (long)viewCount : 0;
                returnProfile.VideoWatchCount = videoWatchCount != null ? (long)videoWatchCount : 0;
                returnProfile.SubscriberCount = subscriberCount != null ? (long)subscriberCount : 0;
                returnProfile.LastWebAccess = lastWebAccess != null ? (string)lastWebAccess : null;
                returnProfile.TotalUploadViews = totalUploadViews != null ? (long)totalUploadViews : 0;
                returnProfile.UploadedFeedLinkRel = uploadedFeedLinkRel != null ? (string)uploadedFeedLinkRel : null;
                returnProfile.UploadedFeedLinkHref = uploadedFeedLinkHref != null ? (string)uploadedFeedLinkHref : null;
                returnProfile.UploadedFeedLinkCountHint = uploadedFeedLinkCountHint != null ? (long)uploadedFeedLinkCountHint : 0;
                returnProfile.Thumbnail = thumbnail != null ? (string)thumbnail : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception attempting to get youtube profile: " + ex);

                returnProfile = null;
            }

            return returnProfile;
        }

        private List<YoutubeVideoEntity> ParseGetVideosResponse(string response)
        {
            var returnVideos = new List<YoutubeVideoEntity>();

            try
            {
                // Parse the raw atom feed
                var byteArray = Encoding.UTF8.GetBytes(response);
                var stream = new MemoryStream(byteArray);
                var videos = XElement.Load(stream);

                // We'll need these namespaces for parsing
                XNamespace def = "http://www.w3.org/2005/Atom";
                XNamespace openSearch = "http://a9.com/-/spec/opensearch/1.1/";
                XNamespace yt = "http://gdata.youtube.com/schemas/2007";
                XNamespace gd = "http://schemas.google.com/g/2005";
                XNamespace media = "http://search.yahoo.com/mrss/";

                // Get the open search results first (applies to all entries)
                XElement totalResults = videos.Element(openSearch + "totalResults");
                XElement startIndex = videos.Element(openSearch + "startIndex");
                XElement itemsPerPage = videos.Element(openSearch + "itemsPerPage");

                // Grab the collection of video entries
                IEnumerable<XElement> entries = from entry in videos.Elements(def + "entry")
                                                select entry;

                // Don't let one bad entry spoil it for the rest
                try
                {
                    foreach (XElement entry in entries)
                    {
                        // Carefully extract out the values, note the "?" annotations indicating optional elements
                        XElement id = entry.Element(def + "id");
                        XElement published = entry.Element(def + "published");   // ?
                        XElement updated = entry.Element(def + "updated");
                        XElement title = entry.Element(def + "title");
                        
                        XAttribute contentSrc  = null;
                        XAttribute contentType = null;
                        var content = entry.Element(def + "content");       // ?
                        if (content != null)
                        {
                            contentSrc = content.Attribute("src");          // ?
                            contentType = content.Attribute("type");        // ?
                        }
                        else
                        {
                            // TODO: Log 
                        }

                        XElement authorName = null;
                        XElement authorUri = null;
                        XElement authorUserId = null;
                        var author = entry.Element(def + "author");       // ?
                        if (author != null)
                        {
                            authorName = author.Element(def + "name");      // ?
                            authorUri = author.Element(def + "uri");        // ?
                            authorUserId = author.Element(yt + "userId");   // ?
                        }
                        else
                        {
                            // TODO: Log 
                        }

                        XAttribute likes = null;
                        XAttribute dislikes = null;
                        var rating = entry.Element(yt + "rating");                // ?
                        if (rating != null)
                        {
                            likes = rating.Attribute("numLikes");           // ?
                            dislikes = rating.Attribute("numDislikes");     // ?
                        }

                        XAttribute viewCount = null;
                        XAttribute favoriteCount = null;
                        XElement statistics = entry.Element(yt + "statistics");
                        if (statistics != null)
                        {
                            viewCount = statistics.Attribute("viewCount");
                            favoriteCount = statistics.Attribute("favoriteCount");
                        }
                        else
                        {
                            // TODO: Log 
                        }

                        XElement duration = null;
                        XAttribute durationSeconds = null;
                        XElement uploaded = null;
                        XElement videoId = null;
                        XAttribute thumbnailUrl = null;
                        var mediaGroup = entry.Element(media + "group");    // ?
                        if (mediaGroup != null)
                        {
                            duration = mediaGroup.Element(yt + "duration"); // ?
                            if (duration != null)
                            {
                                durationSeconds = duration.Attribute("seconds");
                            }
                            uploaded = mediaGroup.Element(yt + "uploaded"); // ?
                            videoId = mediaGroup.Element(yt + "videoid");   // ?
                            var hqdefault = "hqdefault";
                            var mediaThumbnail = (from thumbnail in mediaGroup.Elements(media + "thumbnail")
                                                  where hqdefault == (string)thumbnail.Attribute(yt + "name")
                                                  select thumbnail).FirstOrDefault();
                            if (mediaThumbnail != null)
                            {
                                thumbnailUrl = mediaThumbnail.Attribute("url");
                            }
                            else
                            {
                                // TODO: Log 
                            }
                        }
                        else
                        {
                            // TODO: Log 
                        }

                        // TODO: Move outside of loop
                        string relSelf = "self";
                        string relPrev = "prev";
                        string relNext = "next";
                        string relVideoResponses = "http://gdata.youtube.com/schemas/2007#video.responses";
                        string relRatings = "http://gdata.youtube.com/schemas/2007#video.ratings";
                        XAttribute linkSelf = (from link in entry.Elements(def + "link")
                                               where relSelf == (string)link.Attribute("rel")
                                               select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkPrev = (from link in entry.Elements(def + "link")
                                               where relPrev == (string)link.Attribute("rel")
                                               select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkNext = (from link in entry.Elements(def + "link")
                                               where relNext == (string)link.Attribute("rel")
                                               select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkVideoResponses = (from link in entry.Elements(def + "link")
                                                         where relVideoResponses == (string)link.Attribute("rel")
                                                         select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkRatings = (from link in entry.Elements(def + "link")
                                                  where relRatings == (string)link.Attribute("rel")
                                                  select link.Attribute("href")).FirstOrDefault();

                        XElement commentsFeedLink = null;
                        XAttribute commentsLink = null;
                        XAttribute commentsCountHint = null;
                        XElement comments = entry.Element(gd + "comments");
                        if (comments != null)
                        {
                            commentsFeedLink = comments.Element(gd + "feedLink");
                            if (commentsFeedLink != null)
                            {
                                commentsLink = commentsFeedLink.Attribute("href");
                                commentsCountHint = commentsFeedLink.Attribute("countHint");
                            }
                            else
                            {
                                // TODO: Log 
                            }
                        }
                        else
                        {
                            // TODO: Log 
                        }


                        // Now, carefully populate our data model
                        var videoModel = new YoutubeVideoEntity();

                        videoModel.ServerId = (string)id;
                        videoModel.Published = published != null ? (string)published : null;
                        videoModel.Updated = updated != null ? (string)updated : null;
                        videoModel.Title = title != null ? (string)title : null;
                        
                        videoModel.ContentSrc = contentSrc != null ? (string)contentSrc : null;
                        videoModel.ContentType = contentType != null ? (string)contentType : null;
                        
                        videoModel.AuthorName = authorName != null ? (string)authorName : null;
                        videoModel.AuthorUri = authorUri != null ? (string)authorUri : null;
                        videoModel.AuthorUserId = authorUserId != null ? (string)authorUserId : null;

                        videoModel.Likes = likes != null ? (long)likes : 0;
                        videoModel.Dislikes = dislikes != null ? (long)dislikes : 0;

                        videoModel.ViewCount = viewCount != null ? (long)viewCount : 0;
                        videoModel.FavoriteCount = favoriteCount != null ? (long)favoriteCount : 0;

                        videoModel.Duration = durationSeconds != null ? (long)durationSeconds : 0;
                        videoModel.Uploaded = uploaded != null ? (string)uploaded : null;
                        videoModel.VideoId = videoId != null ? (string)videoId : null;
                        videoModel.Thumbnail = thumbnailUrl != null ? (string)thumbnailUrl : null;

                        videoModel.LinkSelf = linkSelf != null ? (string)linkSelf : null;
                        videoModel.LinkPrev = linkPrev != null ? (string)linkPrev : null;
                        videoModel.LinkNext = linkNext != null ? (string)linkNext : null;
                        videoModel.LinkVideoResponses = linkVideoResponses != null ? (string)linkVideoResponses : null;
                        videoModel.LinkRatings = linkRatings != null ? (string)linkRatings : null;

                        videoModel.LinkComments = commentsLink != null ? (string)commentsLink : null;
                        videoModel.CommentsCountHint = commentsCountHint != null ? (long)commentsCountHint : 0;

                        videoModel.TotalResults = totalResults != null ? (long)totalResults : 0;
                        videoModel.StartIndex = startIndex != null ? (long)startIndex : 0;
                        videoModel.ItemsPerPage = itemsPerPage != null ? (long)itemsPerPage : 0;

                        // And finally add the model to the collection we will return
                        returnVideos.Add(videoModel);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception parsing an individual youtube video: " + ex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing youtube videos: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnVideos;
        }

        #endregion

    }
}
