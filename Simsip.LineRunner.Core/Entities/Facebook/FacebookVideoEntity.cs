using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    public class FacebookVideoEntity
    {
        #region Data model

        /// <summary>
        /// The id created on the device to identify this photo.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The video ID
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The profile (user or page) id that created this video
        /// </summary>
        [Indexed]
        public string FromId { get; set; }

        /// <summary>
        /// The profile (user or page) name that created this video
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// The users who are tagged in this video.
        /// 
        /// Array of objects containing id and name fields
        /// </summary>
        [Ignore]
        public IList<FacebookVideoTag> Tags { get; set; }

        /// <summary>
        /// The video title or caption.
        /// </summary>
        public string Name { get; set; }	

        /// <summary>
        /// The description of the video.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The URL for the thumbnail picture for the video.
        /// 
        /// An HTTP 302 to the URL of the album's cover picture.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// The html element that may be embedded in an Web page to play the video.
        /// 
        /// String containing a valid URL
        /// </summary>
        public string EmbedHtml	{ get; set; } 

        /// <summary>
        /// The icon that Facebook displays when video are published to the Feed.
        /// 
        /// String containing a valid URL
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// A URL to the raw, playable video file.
        /// 
        /// String containing a valid URL.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The time the video was initially published.
        /// 
        /// String containing ISO-8601 date-time.
        /// </summary>
        public string CreatedTime { get; set; }

        /// <summary>
        /// The last time the video or its caption were updated.
        /// 
        /// String containing ISO-8601 date-time
        /// </summary>
        public string UpdatedTime	{ get; set; }

        /// <summary>
        /// Used to store the time for the last time the video was seen.
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// A value computed upon a video refresh to determine if we have seen
        /// it or not. 
        /// 
        /// Uses LastSeen to determine this.
        /// </summary>
        public bool IsUnseen { get; set; }
        
        #endregion

        #region Connections

        /// <summary>
        /// All of the likes on this video.
        /// 
        /// Array of objects containing id and name fields.
        /// </summary>
        [Ignore]
        public IList<FacebookLikeEntity> Likes { get; set; }

        /// <summary>
        /// All of the comments on this video
        /// 
        /// Array of objects containing id, from, message, created_time, and likes fields.
        /// </summary>
        [Ignore]
        public IList<FacebookCommentEntity> Comments	{ get; set; }

        #endregion

    }

    public class FacebookVideoTag
    {
        /// <summary>
        /// The facebook user id who was tagged.
        /// </summary>
        public string Id;

        /// <summary>
        /// The name of the facebook user who was tagged.
        /// </summary>
        public string Name;
    }
}
