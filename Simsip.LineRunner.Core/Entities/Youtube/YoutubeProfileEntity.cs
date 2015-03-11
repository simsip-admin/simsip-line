using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Youtube
{
    /// <summary>
    /// A youtube user profile. 
    /// 
    /// Note, unless you are a youtube partner, a user profile has a 1:1
    /// mapping to the user's channel. A typical youtube user is allowed only 1 channel and you get
    /// the reference to that channel via the youtube user's profile.
    /// 
    /// If you have qualified to be a youtube partner, then you can have more than one channel.
    /// 
    /// https://developers.google.com/youtube/2.0/reference#youtube_data_api_tag_entry
    /// </summary>
    public class YoutubeProfileEntity
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
        ///  Occurance: Mandatory
        /// </summary>
        public string Updated { get; set; }

        /// <summary>
        /// The <title> tag provides a human-readable title for a feed or an entry in a feed.
        /// 
        ///  Occurance: Mandatory
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The <yt:firstName> tag specifies the user's first name. 
        /// 
        ///  Occurance: Optional
        ///  
        /// This tag appears in a user profile entry.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The <yt:lastName> tag specifies the user's last name. 
        /// 
        /// Occurance: Optional
        /// 
        /// This tag appears in a user profile entry.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The <yt:userId> tag contains a value that uniquely identifies a YouTube user regardless of account 
        /// characteristics such as whether the user's account has a YouTube channel or is connected to a 
        /// Google+ profile. 
        /// 
        /// Occurance: Unknown
        ///  
        /// If you need to manually generate a feed URL that is not for the default user, you can use this 
        /// value as the username in the feed URL.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The <yt:username> tag specifies either a YouTube username or a globally unique YouTube user ID that 
        /// identifies the user account. (See the <yt:userId> tag definition for more details about that value.) Regardless, the value will be a valid value to use in a feed URL. However, we recommend that you use the value of this tag's display attribute as a meaningful value to display when identifying a user.
        /// 
        /// Occurance: Optional
        ///  
        /// - In a subscriptions feed, the <yt:username> tag identifies the user whose actions are being subscribed to 
        /// (for a user activity subscription), or the owner of the YouTube channel associated with the subscription.
        /// - In a request to add a subscription, the <yt:username> tag identifies the user whose activity or channel is being subscribed to.
        /// - In a profile entry, the <yt:username> tag identifies the user associated with the profile.
        /// - In a user contact entry, the <yt:username> tag identifies the contact of the logged-in user.
        /// - In an activity feed entry, the <yt:username> tag identifies the user associated with an action. For example, it might 
        /// identify the user who owns a channel that was subscribed to.
        /// - In a request to upgrade an unlinked Google Account so that it is associated with a YouTube username (and channel), 
        /// the <yt:username> tag identifies the YouTube username that the account holder wants to have.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The display attribute contains a meaningful, but non-unique, value for a user that is suitable to display as a username. 
        /// 
        /// Occurance: Optional
        ///  
        /// For users who have accounts that are connected to Google+, the attribute value will be the user's full public display name. 
        /// For users who have full YouTube accounts (as opposed to unlinked Google accounts) that are not connected to Google+, the 
        /// attribute value will be set to the user's YouTube account name.
        /// </summary>
        public string UserNameDisplay { get; set; }

        /// <summary>
        /// The <yt:channelId> tag contains a value that uniquely identifies a YouTube channel.
        /// 
        /// Occurance: Unknown
        /// </summary>
        public string ChannelId { get; set; }

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
        /// When the viewCount attribute refers to a user profile, the attribute specifies the number of times 
        /// that the user's profile has been viewed.
        /// 
        /// Occurance: Optional
        ///  
        /// The <yt:statistics> tag provides statistics about a video or a user. The <yt:statistics> tag 
        /// is not provided in a video entry if the value of the viewCount attribute is 0. This tag appears 
        /// in video, playlist and user profile entries.
        /// </summary>
        public long ViewCount { get; set; }

        /// <summary>
        /// The videoWatchCount attribute specifies the number of videos that a user has watched on 
        /// YouTube. 
        /// 
        /// Occurance: Optional
        /// 
        /// The videoWatchCount attribute is only specified when the <yt:statistics> tag appears within 
        /// a user profile entry.
        /// 
        /// The <yt:statistics> tag provides statistics about a video or a user. The <yt:statistics> tag 
        /// is not provided in a video entry if the value of the viewCount attribute is 0. This tag appears 
        /// in video, playlist and user profile entries.
        /// </summary>
        public long VideoWatchCount { get; set; }

        /// <summary>
        /// The subscriberCount attribute specifies the number of YouTube users who have subscribed to a 
        /// particular user's YouTube channel. 
        /// 
        /// Occurance: Optional
        /// 
        /// The subscriberCount attribute is only specified when the <yt:statistics> tag appears within a 
        /// user profile entry.
        /// 
        /// The <yt:statistics> tag provides statistics about a video or a user. The <yt:statistics> tag 
        /// is not provided in a video entry if the value of the viewCount attribute is 0. This tag appears 
        /// in video, playlist and user profile entries.
        /// </summary>
        public long SubscriberCount { get; set; }

        /// <summary>
        /// The lastWebAccess attribute indicates the most recent time that a particular user used 
        /// YouTube.
        /// 
        /// Occurance: Optional
        /// 
        /// The <yt:statistics> tag provides statistics about a video or a user. The <yt:statistics> tag 
        /// is not provided in a video entry if the value of the viewCount attribute is 0. This tag appears 
        /// in video, playlist and user profile entries.
        /// </summary>
        public string LastWebAccess { get; set; }

        /// <summary>
        /// The totalUploadViews attribute specifies the total number of views for all of a channel's 
        /// videos. 
        /// 
        /// Occurance: Optional
        /// 
        /// This attribute is only specified when the <yt:statistics> tag appears within a user profile 
        /// entry.
        /// </summary>
        public long TotalUploadViews { get; set; }

        /// <summary>
        /// The rel attribute contains the name of the related feed linked to in the href attribute.
        /// 
        /// Occurance: Optional
        ///  
        /// The <gd:feedLink> tag identifies a logically nested feed. For example, a comments feed is 
        /// logically nested beneath a video entry. Similarly, the feed for a single playlist is 
        /// logically nested within a playlist entry in a user's playlists feed.
        /// </summary>
        public string UploadedFeedLinkRel { get; set; }

        /// <summary>
        /// The href attribute contains the absolute URL of the related feed.
        /// 
        /// Occurance: Optional
        /// 
        /// The <gd:feedLink> tag identifies a logically nested feed. For example, a comments feed is 
        /// logically nested beneath a video entry. Similarly, the feed for a single playlist is 
        /// logically nested within a playlist entry in a user's playlists feed.
        /// </summary>
        public string UploadedFeedLinkHref { get; set; }

        /// <summary>
        /// The gd:countHint specifies the number of entries in the related feed. 
        /// 
        /// Occurance: Optional
        ///  
        /// For example, if the <gd:feedLink> tag specifies a link to the comments feed for 
        /// a video, the gd:countHint attribute indicates how many comments exist for the video.
        /// 
        /// The <gd:feedLink> tag identifies a logically nested feed. For example, a comments feed is 
        /// logically nested beneath a video entry. Similarly, the feed for a single playlist is 
        /// logically nested within a playlist entry in a user's playlists feed.
        /// </summary>
        public long UploadedFeedLinkCountHint { get; set; }

        /// <summary>
        /// The url attribute specifies the URL for the thumbnail image.
        /// 
        /// Occurance: Optional
        ///  
        /// The <media:thumbnail> tag specifies the location of an image that can be used to represent a user.
        /// </summary>
        public string Thumbnail { get; set; }

        #endregion

        #endregion
    }
}
