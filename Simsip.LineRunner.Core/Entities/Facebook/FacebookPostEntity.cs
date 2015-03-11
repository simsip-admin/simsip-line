using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    /// <summary>
    /// Querying without the read_stream permission will return only the public view 
    /// of the data (i.e. data that can be see when the user is logged out).
    /// </summary>
    public class FacebookPostEntity
    {
        #region Data model

        /// <summary>
        /// The id created on the device to identify this photo.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The post ID
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// Id of the user who posted the message
        /// </summary>
        [Indexed]
        public string FromId { get; set; }

        /// <summary>
        /// Name of the user who posted the message
        /// </summary>
        public string FromName { get; set; }

        // TODO
        // to field: Profiles mentioned or targeted in this post
        // Contains in data an array of objects, each with the name and Facebook id of the user
        // - see facebook reference

        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; set; }

        // TODO
        // message_tags field: Objects tagged in the message (Users, Pages, etc)
        // - see facebook reference

        /// <summary>
        /// If available, a link to the picture included with this post
        /// 
        /// String containing the URL
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// The link attached to this post
        /// 
        /// String containing the URL
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The name of the link
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The caption of the link (appears beneath the link name)
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// A description of the link (appears beneath the link caption)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A URL to a Flash movie or video file to be embedded within the post
        /// 
        /// string containing the URL
        /// </summary>
        public string Source { get; set; }

        // TODO
        // properties field: A list of properties for an uploaded video, for example, the length of the video
        // - see facebook reference

        /// <summary>
        /// A link to an icon representing the type of this post
        /// </summary>
        public string Icon { get; set; }

        // TODO
        // actions field: A list of available actions on the post (including commenting, liking, and an optional app-specified action)
        // - see facebook reference

        // TODO
        // privacy field: The privacy settings of the Post
        // - see facebook reference
        // also: https://developers.facebook.com/docs/reference/api/privacy-parameter/

        /// <summary>
        /// A string indicating the type for this post (including link, photo, video)
        /// 
        /// Note: Fql's stream table has a type column which is an int to describe the stream item:
        /// https://developers.facebook.com/docs/reference/fql/stream
        /// </summary>
        public string Type { get; set; }

        // TODO: host of other fields
        // - see facebook reference

        /// <summary>
        /// The Facebook object id for an uploaded photo or video
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// The time of the last comment on this post
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// Type of post
        /// 
        /// One of mobile_status_update, created_note, added_photos, added_video, 
        /// shared_story, created_group, created_event, wall_post, app_created_story, 
        /// published_story, tagged_in_photo, approved_friend
        /// </summary>
        public string StatusType { get; set; }

        #endregion

        #region Connections

        // TODO: See facebook reference for what we want here

        #endregion

    }
}
