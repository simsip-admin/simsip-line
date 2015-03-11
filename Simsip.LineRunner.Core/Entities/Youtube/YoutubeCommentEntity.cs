using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Youtube
{
    /// <summary>
    /// A comment on a youtube video.
    /// 
    /// References:
    /// https://developers.google.com/youtube/2.0/reference#youtube_data_api_tag_entry
    /// https://developers.google.com/youtube/2.0/developers_guide_protocol_comments
    /// </summary>
    public class YoutubeCommentEntity
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
        /// The foreign key for this comment.
        /// 
        /// Currently this will be a video id.
        /// </summary>
        [Indexed]
        public string ForeignKey { get; set; }

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
        /// The <content> tag contains the text of the comment.
        /// 
        /// Occurance: Optional
        /// </summary>
        public string Content { get; set; }

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
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkSelf { get; set; }

        /// <summary>
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkPrev { get; set; }

        /// <summary>
        /// Reference:
        /// https://developers.google.com/youtube/2.0/reference#Navigating_between_feeds
        /// </summary>
        public string LinkNext { get; set; }

        /// <summary>
        /// Reference:
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_comments
        /// </summary>
        public string LinkInReplyTo { get; set; }

        /// <summary>
        /// 
        /// TODO: Need to figure this one out - not in api doc link below.
        /// 
        /// Reference:
        /// https://developers.google.com/youtube/2.0/developers_guide_protocol_comments
        /// </summary>
        public string LinkEdit { get; set; }

        /// <summary>
        /// The <yt:spam> tag indicates, by its presence, that a comment has been flagged
        /// as spam. 
        /// 
        /// Occurence: Optional
        /// 
        /// The absence of this tag in a comment feed entry indicates that the 
        /// corresponding comment has not been flagged as spam. This tag is only 
        /// supported in API version 2.
        /// </summary>
        public bool Spam { get; set; }

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
