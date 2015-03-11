using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Youtube
{
    /// <summary>
    /// A youtube video.
    /// 
    /// Reference:
    /// https://developers.google.com/youtube/2.0/reference#youtube_data_api_tag_entry
    /// </summary>
    public class YoutubeVideoEntity
    {
        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this group message.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }
        
        /// <summary>
        /// The <id> tag specifies a URN that uniquely and permanently identifies a feed or feed entry.
        /// 
        /// Occurance: Mandatory
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The <published> tag specifies the time that a feed entry was created. 
        /// 
        /// Occurance: Optional
        ///  
        /// Times are specified in UTC.
        /// </summary>
        public string Published { get; set; }

        /// <summary>
        /// The <updated> tag specifies the time that a feed or feed entry was last updated.
        /// 
        /// Occurance: Mandatory
        /// </summary>
        public string Updated { get; set; }

        /// <summary>
        /// The <title> tag provides a human-readable title for a feed or an entry in a feed.
        /// 
        /// Occurance: Mandatory
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The src attribute specifies the feed URL that you would use to retrieve the live video entry
        /// associated with an event. 
        /// 
        /// Occurance: Optional
        /// 
        /// This attribute will only be present in an API response if you did not set the inline parameter
        /// value to true in the API request.
        /// 
        /// In a live video entry, the <content> tag contains either a feed URL for the live video entry
        /// associated with an event or it encapsulates the XML for the live video entry. In time, 
        /// the <content> tag could contain a feed URL for a playlist entry or encapsulate the XML for a 
        /// playlist entry (rather than a live video entry).
        /// - If your API request set the inline parameter to false or did not include the inline parameter, then the <content> tag for an event entry will specify the feed URL for the live video entry associated with the event. In this case, the tag's type and src attributes will both appear in the response.
        /// - If your API request set the inline parameter to true, then the <content> tag for an event entry will encapsulate an <entry> tag describing the live video entry associated with the event. In this case, the tag's type attribute will be set, and its src attribute will not be set.
        /// </summary>
        public string ContentSrc { get; set; }

        /// <summary>
        /// The type attribute specifies the format of the content that is either encapsulated within 
        /// the <content> tag or accessible at the URL specified in the src attribute value.
        /// 
        /// Occurance: Optional
        /// 
        /// In a live video entry, the <content> tag contains either a feed URL for the live video entry
        /// associated with an event or it encapsulates the XML for the live video entry. In time, 
        /// the <content> tag could contain a feed URL for a playlist entry or encapsulate the XML for a 
        /// playlist entry (rather than a live video entry).
        /// - If your API request set the inline parameter to false or did not include the inline parameter, then the <content> tag for an event entry will specify the feed URL for the live video entry associated with the event. In this case, the tag's type and src attributes will both appear in the response.
        /// - If your API request set the inline parameter to true, then the <content> tag for an event entry will encapsulate an <entry> tag describing the live video entry associated with the event. In this case, the tag's type attribute will be set, and its src attribute will not be set.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The <name> tag contains the author's YouTube username.
        /// 
        /// Occurance: Optional
        ///  
        /// The <author> tag usually contains information about the YouTube user who owns a piece of video content.
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// The <uri> tag contains a URL related to the author.
        /// 
        /// Occurance: Optional
        ///  
        /// The <author> tag usually contains information about the YouTube user who owns a piece of video content.
        /// </summary>
        public string AuthorUri { get; set; }

        /// <summary>
        /// The <yt:userId> tag contains a value that uniquely identifies a YouTube user regardless of account 
        /// characteristics such as whether the user's account has a YouTube channel or is connected to a Google+ profile. 
        /// 
        /// Occurance: Optional
        ///  
        /// If you need to manually generate a feed URL that is not for the default user, you can use this value as the 
        /// username in the feed URL.
        /// 
        /// The <author> tag usually contains information about the YouTube user who owns a piece of video content.
        /// </summary>
        public string AuthorUserId { get; set; }

        /// <summary>
        /// The url attribute specifies the URL for the thumbnail image.
        /// 
        /// Occurance: Optional
        ///  
        /// The <media:thumbnail> tag specifies the location of an image that can be used to represent a 
        /// video or playlist in a list of results.
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// The seconds attribute indicates the duration, in seconds, of a video or of all of the 
        /// videos in a playlist.
        /// 
        /// Occurance: Optional
        ///  
        /// The <yt:duration> tag specifies the duration, in seconds, of either a video or of all of
        /// the videos in a playlist.
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// The <yt:uploaded> tag specifies the time that a playlist entry was originally uploaded to 
        /// YouTube.
        /// 
        /// Occurance: Optional
        /// </summary>
        public string Uploaded { get; set; }

        /// <summary>
        /// The <yt:videoid> tag specifies a unique ID that YouTube uses to identify a video. 
        /// 
        /// Occurance: Optional
        ///  
        /// In an activity feed entry, the <yt:videoid> tag identifies the video that was rated, shared, 
        /// commented on, uploaded, or marked as a favorite.
        /// </summary>
        public string VideoId { get; set; }

        /// <summary>
        /// The numLikes attribute specifies the number of raters who indicated that they liked a video.
        /// 
        /// Occurance: Optional
        ///  
        /// The <yt:rating> tag specifies the rating that a user is assigning to a video (in a request 
        /// to add a rating) or, in an API response, the total number of raters who liked and disliked
        /// a video.
        /// </summary>
        public long Likes { get; set; }

        /// <summary>
        /// The numDislikes attribute specifies the number of raters who indicated that they disliked a video.
        /// 
        /// Occurance: Optional
        ///  
        /// The <yt:rating> tag specifies the rating that a user is assigning to a video (in a request 
        /// to add a rating) or, in an API response, the total number of raters who liked and disliked
        /// a video.
        /// </summary>
        public long Dislikes { get; set; }

        /// <summary>
        /// When the viewCount attribute refers to a video entry, the attribute specifies the number of
        /// times that the video has been viewed. 
        /// 
        /// Occurance: Optional
        ///  
        /// The <yt:statistics> tag provides statistics about a video or a user. The <yt:statistics> tag 
        /// is not provided in a video entry if the value of the viewCount attribute is 0. This tag appears 
        /// in video, playlist and user profile entries.
        /// </summary>
        public long ViewCount { get; set; }

        /// <summary>
        /// The favoriteCount attribute specifies the number of YouTube users who have added a video to 
        /// their list of favorite videos.
        /// 
        /// Occurance: Optional
        /// 
        /// The favoriteCount attribute is only specified when the <yt:statistics> tag appears within a 
        /// video entry.
        /// 
        /// The <yt:statistics> tag provides statistics about a video or a user. The <yt:statistics> tag 
        /// is not provided in a video entry if the value of the viewCount attribute is 0. This tag appears 
        /// in video, playlist and user profile entries.
        /// </summary>
        public long FavoriteCount { get; set; }

        /// <summary>
        /// For navigation purposes, a link that represents this video.
        /// 
        /// Occurance: Optional
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkSelf { get; set; }

        /// <summary>
        /// For navigation purposes, a link that represents a possible previous page of videos
        /// in a video collection response.
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkPrev { get; set; }

        /// <summary>
        /// For navigation purposes, a link that represents a possible next page of videos
        /// in a video collection response.
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkNext { get; set; }

        /// <summary>
        /// A link that represents the collection of video responses made on this video.
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkVideoResponses { get; set; }

        /// <summary>
        /// A link that represents the collection of comments made on this video.
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkComments { get; set; }

        /// <summary>
        /// An indication of the number of comments for this video.
        /// </summary>
        public long CommentsCountHint { get; set; }

        /// <summary>
        /// A link that represents the ratings for this video.
        /// 
        /// Use this to add likes or dislikes to a video.
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkRatings { get; set; }

        /// <summary>
        /// The <openSearch:totalResults> tag identifies the number of items in the result set for 
        /// the feed. 
        /// 
        /// Please note that the tag value is an approximation and may not represent an exact value. 
        /// In addition, the maximum value for this tag is 1,000,000. You should not use this value 
        /// to create pagination links. Use the <link rel="next"> and <link rel="prev"> tags to 
        /// determine whether to show pagination links.
        /// 
        /// If the feed contains a <link> tag for which the rel attribute value is next, then there 
        /// is another page of results. Otherwise, the current page is the last page in the result set. 
        /// Similarly, if the feed contains a <link> tag for which the rel attribute value is prev, 
        /// then there is a previous page of results. Otherwise, the current page is the first page 
        /// in the result set.
        /// </summary>
        public long TotalResults { get; set; }

        /// <summary>
        /// The <openSearch:startIndex> tag identifies the one-based index of the 
        /// first item returned in the feed.
        /// 
        /// Occurance: Mandatory
        /// </summary>
        public long StartIndex { get; set; }

        /// <summary>
        /// The <openSearch:itemsPerPage> tag indicates how many entries are in an API response.
        /// 
        /// Occurence: Mandatory
        /// </summary>
        public long ItemsPerPage { get; set; }

        #endregion

        #endregion
    }
}
