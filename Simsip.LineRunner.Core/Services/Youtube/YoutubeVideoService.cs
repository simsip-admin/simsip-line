using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Entities.Youtube;
using Simsip.LineRunner.Services.Rest;
using Simsip.LineRunner.Data.Youtube;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Services.OAuth;


namespace Simsip.LineRunner.Services.Youtube
{
    public class YoutubeVideoService : IYoutubeVideoService
    {
        private const string TRACE_TAG = "YoutubeVideoService";

        private readonly IRestService _restService;
        private readonly IYoutubeVideoRepository _videoRepository;

        public YoutubeVideoService(IRestService restService, 
                                   IYoutubeVideoRepository videoRepository)
        {
            _restService = restService;
            _videoRepository = videoRepository;
        }

        #region IVideoService Implementation

        #region Comments

        /// <summary>
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_comments
        /// </summary>
        /// <param name="simsipUser"></param>
        /// <param name="video"></param>
        public async Task AddComment(SimsipUserEntity user,
                               YoutubeVideoEntity video, 
                               YoutubeCommentEntity comment)
        {
            try
            {
                // Request setup
                using (var client = new HttpClient())
                {
                    var url = video.LinkComments +
                              "&access_token=" + // TODO: App.YoutubeAccessToken +
                              "&key=" + OAuthCredentials.YOUTUBE_APP_ID;

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.YoutubeAccessToken);
                    client.DefaultRequestHeaders.Add("GData-Version", "2");
                    client.DefaultRequestHeaders.Add("X-GData-Key", "key=" + OAuthCredentials.YOUTUBE_APP_ID);

                    // Payload setup
                    // Example of payload:
                    // <?xml version="1.0" encoding="UTF-8"?>
                    //        <entry xmlns="http://www.w3.org/2005/Atom"
                    //               xmlns:yt="http://gdata.youtube.com/schemas/2007">
                    //              <content>This is a crazy video.</content>
                    //         </entry>
                    string defaultNamespaceString = "http://www.w3.org/2005/Atom";
                    string youtubeNamespaceString = "http://gdata.youtube.com/schemas/2007";
                    XNamespace def = defaultNamespaceString;
                    XNamespace yt = youtubeNamespaceString;
                    XElement payload = new XElement(def + "entry",
                                        new XAttribute("xmlns", defaultNamespaceString),
                                        new XAttribute(XNamespace.Xmlns + "yt", youtubeNamespaceString),
                                            new XElement("content", comment.Content)
                                     );
                    var body = payload.ToString();

                    var content = new StringContent(body, Encoding.UTF8, "application/atom+xml");
                    
                    await _restService.MakeRequestAsync(client,
                            url,
                            RestVerbs.Post,
                            content);

                    // TODO: How to parse out comment id so we can save locally?
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding youtube comment to video: " + ex);
            }
        }

        /// <summary>
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_comments
        /// </summary>
        /// <param name="simsipUser"></param>
        /// <param name="video"></param>
        /// <returns></returns>
        public async Task<IList<YoutubeCommentEntity>> GetComments(YoutubeVideoEntity video)
        {
            var returnComments = new List<YoutubeCommentEntity>();

            try
            {
                // We'll assume that we don't need and access token or key here
                var response = await _restService.MakeRequestAsync(video.LinkComments,
                                        RestVerbs.Get,
                                        OAuthType.None);

                // TODO: Analyze http status code here before parsing

                returnComments = ParseGetCommentsResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting youtube comments for a video: " + ex);
            }

            return returnComments;
        }

        public async Task AddCommentInReplyTo(SimsipUserEntity user, 
                                              YoutubeVideoEntity video, 
                                              YoutubeCommentEntity originalComment, 
                                              YoutubeCommentEntity inReplyToComment)
        {
            try
            {
                // Request setup
                using (var client = new HttpClient())
                {
                    var url = video.LinkComments +
                              "&access_token=" + // TODO App.YoutubeAccessToken +
                              "&key=" + OAuthCredentials.YOUTUBE_APP_ID;

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.YoutubeAccessToken);
                    client.DefaultRequestHeaders.Add("GData-Version", "2");
                    client.DefaultRequestHeaders.Add("X-GData-Key", "key=" + OAuthCredentials.YOUTUBE_APP_ID);

                    // Payload setup
                    // Example of payload:
                    // <?xml version="1.0" encoding="UTF-8"?>
                    //        <entry xmlns="http://www.w3.org/2005/Atom"
                    //               xmlns:yt="http://gdata.youtube.com/schemas/2007">
                    //              <content>This is a crazy video.</content>
                    //         </entry>
                    string defaultNamespaceString = "http://www.w3.org/2005/Atom";
                    string youtubeNamespaceString = "http://gdata.youtube.com/schemas/2007";
                    XNamespace def = defaultNamespaceString;
                    XNamespace yt = youtubeNamespaceString;
                    XElement payload = new XElement(def + "entry",
                                        new XAttribute("xmlns", defaultNamespaceString),
                                        new XAttribute(XNamespace.Xmlns + "yt", youtubeNamespaceString),
                                            new XElement("link"),
                                                new XAttribute("rel", "http://gdata.youtube.com/schemas/2007#in-reply-to"),
                                                new XAttribute("type", "application/atom+xml"),
                                                new XAttribute("href", originalComment.LinkSelf),
                                            new XElement("content", inReplyToComment.Content)
                                     );
                    string body = payload.ToString();

                    var content = new StringContent(body, Encoding.UTF8, "application/atom+xml");

                    await _restService.MakeRequestAsync(client,
                            url,
                            RestVerbs.Post,
                            content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding youtube in reply to comment to video: " + ex);
            }
        }

        /// <summary>
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_comments
        /// </summary>
        /// <param name="simsipUser"></param>
        /// <param name="video"></param>
        /// <param name="comment"></param>
        public async Task DeleteComment(SimsipUserEntity user,
                                  YoutubeCommentEntity comment)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Request setup
                    // TODO: We still don't know how to extract the edit link
                    var url = comment.LinkEdit +
                              "&access_token=" + // TODO App.YoutubeAccessToken +
                              "&key=" + OAuthCredentials.YOUTUBE_APP_ID;

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.YoutubeAccessToken);
                    client.DefaultRequestHeaders.Add("GData-Version", "2");
                    client.DefaultRequestHeaders.Add("X-GData-Key",  "key=" + OAuthCredentials.YOUTUBE_APP_ID);

                    await _restService.MakeRequestAsync(client,
                            url,
                            RestVerbs.Delete,
                            null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding youtube comment to video: " + ex);
            }
        }

        #endregion

        #region Likes

        /// <summary>
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_ratings
        /// </summary>
        /// <param name="simsipUser"></param>
        /// <param name="video"></param>
        public async Task AddRating(SimsipUserEntity user,
                              YoutubeVideoEntity video, 
                              bool like)
        {
            try
            {
                // Request setup
                using (var client = new HttpClient())
                {
                    var url = video.LinkRatings +
                                  "&access_token=" + // TODO App.YoutubeAccessToken +
                                  "&key=" + OAuthCredentials.YOUTUBE_APP_ID;

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.YoutubeAccessToken);
                    client.DefaultRequestHeaders.Add("GData-Version", "2");
                    client.DefaultRequestHeaders.Add("X-GData-Key", "key=" + OAuthCredentials.YOUTUBE_APP_ID);
                    
                    // Payload setup
                    // Example of payload:
                    // <?xml version="1.0" encoding="UTF-8"?>
                    //     <entry xmlns="http://www.w3.org/2005/Atom"
                    //            xmlns:yt="http://gdata.youtube.com/schemas/2007">
                    //         <yt:rating value="like"/>
                    // </entry>                
                    string defaultNamespaceString = "http://www.w3.org/2005/Atom";
                    string youtubeNamespaceString = "http://gdata.youtube.com/schemas/2007";
                    XNamespace def = defaultNamespaceString;
                    XNamespace yt = youtubeNamespaceString;
                    XElement rating = null;
                    if (like)
                    {
                        rating = new XElement(yt + "rating", new XAttribute("value", "like"));
                    }
                    else
                    {
                        rating = new XElement(yt + "rating", new XAttribute("value", "dislike"));
                    }
                    XElement payload = new XElement(def + "entry",
                                        new XAttribute("xmlns", defaultNamespaceString),
                                        new XAttribute(XNamespace.Xmlns + "yt", youtubeNamespaceString),
                                            rating
                                     );
                    var body = payload.ToString();

                    var content = new StringContent(body, Encoding.UTF8, "application/atom+xml");

                    await _restService.MakeRequestAsync(client,
                            url,
                            RestVerbs.Post,
                            content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding youtube like to video: " + ex);
            }
        }

        #endregion

        #region Video Responses


        /// <summary>
        /// 
        /// Reference
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_video_responses
        /// </summary>
        /// <param name="simsipUser"></param>
        /// <param name="video"></param>
        /// <returns></returns>
        public async Task<IList<YoutubeVideoEntity>> GetVideoResponses(YoutubeVideoEntity video)
        {
            var returnVideos = new List<YoutubeVideoEntity>();

            try
            {
                // So far, it looks like we can just use the link w/o need for access token/key
                // to get the raw atom feed
                var response = await _restService.MakeRequestAsync(video.LinkVideoResponses,
                                        RestVerbs.Get,
                                        OAuthType.None);

                // TODO: Analyze http status code here before parsing

                returnVideos = ParseGetVideoResponsesResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting youtube video responses: " + ex);
            }

            return returnVideos;
        }

        #endregion

        #endregion

        #region Helper methods

        private List<YoutubeCommentEntity> ParseGetCommentsResponse(string response)
        {
            var returnComments = new List<YoutubeCommentEntity>();

            try
            {
                // Parse the raw atom feed
                var byteArray = Encoding.UTF8.GetBytes(response);
                var stream = new MemoryStream(byteArray);
                var comments = XElement.Load(stream);

                // We'll need these namespaces for parsing
                XNamespace def = "http://www.w3.org/2005/Atom";
                XNamespace openSearch = "http://a9.com/-/spec/opensearch/1.1/";
                XNamespace yt = "http://gdata.youtube.com/schemas/2007";
                XNamespace gd = "http://schemas.google.com/g/2005";
                XNamespace media = "http://search.yahoo.com/mrss/";

                // Get the open search results first (applies to all entries)
                XElement totalResults = comments.Element(openSearch + "totalResults");
                XElement startIndex = comments.Element(openSearch + "startIndex");
                XElement itemsPerPage = comments.Element(openSearch + "itemsPerPage");

                // Grab the collection of video entries
                IEnumerable<XElement> entries = from entry in comments.Elements(def + "entry")
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

                        XElement content = entry.Element(def + "content");       // ?

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
                            // TODO: Log via Joe's standard to understand better
                        }

                        string relSelf = "self";
                        string relPrev = "prev";
                        string relNext = "next";
                        string relInReplyTo = "http://gdata.youtube.com/schemas/2007#in-reply-to";
                        XAttribute linkSelf = (from link in entry.Elements(def + "link")
                                               where relSelf == (string)link.Attribute("rel")
                                               select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkPrev = (from link in entry.Elements(def + "link")
                                               where relPrev == (string)link.Attribute("rel")
                                               select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkNext = (from link in entry.Elements(def + "link")
                                               where relNext == (string)link.Attribute("rel")
                                               select link.Attribute("href")).FirstOrDefault();
                        XAttribute linkInReplyTo = (from link in entry.Elements(def + "link")
                                                    where relInReplyTo == (string)link.Attribute("rel")
                                                    select link.Attribute("href")).FirstOrDefault();

                        XElement spam = entry.Element(yt + "spam");

                        // Now, carefully populate our data model
                        var commentModel = new YoutubeCommentEntity();

                        commentModel.ServerId = (string)id;
                        commentModel.Published = published != null ? (string)published : null;
                        commentModel.Updated = updated != null ? (string)updated : null;
                        commentModel.Title = title != null ? (string)title : null;

                        commentModel.Content = content != null ? (string)content : null;

                        commentModel.AuthorName = authorName != null ? (string)authorName : null;
                        commentModel.AuthorUri = authorUri != null ? (string)authorUri : null;
                        commentModel.AuthorUserId = authorUserId != null ? (string)authorUserId : null;

                        commentModel.LinkSelf = linkSelf != null ? (string)linkSelf : null;
                        commentModel.LinkPrev = linkPrev != null ? (string)linkPrev : null;
                        commentModel.LinkNext = linkNext != null ? (string)linkNext : null;
                        commentModel.LinkInReplyTo = linkInReplyTo != null ? (string)linkInReplyTo : null;

                        commentModel.Spam = spam != null ? (bool)spam : false;

                        // And finally add the model to the collection we will return
                        returnComments.Add(commentModel);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception parsing an individual youtube comment: " + ex, new object[] { });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing youtube comment: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnComments;
        }

        private List<YoutubeVideoEntity> ParseGetVideoResponsesResponse(string response)
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

                        XAttribute contentSrc = null;
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
                        XElement uploaded = null;
                        XElement videoId = null;
                        XAttribute thumbnailUrl = null;
                        var mediaGroup = entry.Element(media + "group");    // ?
                        if (mediaGroup != null)
                        {
                            duration = mediaGroup.Element(yt + "duration"); // ?
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

                        videoModel.Duration = duration != null ? (long)duration : 0;
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
                    Debug.WriteLine("Exception parsing an individual youtube video response: " + ex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing youtube video responses: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnVideos;
        }

        #endregion
    }
}
