using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    /// <summary>
    /// A comment on a Graph API object
    /// </summary>
    public class FacebookCommentEntity
    {
        #region Json Field Names

        /// <summary>
        /// The facebook json field name for the id field.
        /// </summary>
        public static string FIELD_NAME_ID = "id";

        /// <summary>
        /// The facebook json field name for the from field.
        /// </summary>
        public static string FIELD_NAME_FROM = "from";

        /// <summary>
        /// The facebook json field name for the from id field.
        /// </summary>
        public static string FIELD_NAME_FROM_ID = "id";

        /// <summary>
        /// The facebook json field name for the from name field.
        /// </summary>
        public static string FIELD_NAME_FROM_NAME = "name";

        /// <summary>
        /// The facebook json field name for the message field.
        /// </summary>
        public static string FIELD_NAME_MESSAGE = "message";

        /// <summary>
        /// The facebook json field name for the created time field.
        /// </summary>
        public static string FIELD_NAME_CREATED_TIME = "created_time";

        /// <summary>
        /// The facebook json field name for the like count field.
        /// </summary>
        public static string FIELD_NAME_LIKE_COUNT = "like_count";

        /// <summary>
        /// The facebook json field name for the user likes field.
        /// </summary>
        public static string FIELD_NAME_USER_LIKES = "user_likes";

        /// <summary>
        /// The facebook json field name for the type field.
        /// </summary>
        public static string FIELD_NAME_TYPE = "type";

        #endregion

        #region Data Model
        
        #region Fields

        /// <summary>
        /// The id created on the device to identify this comment.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The Facebook ID of the comment
        /// 
        /// This should be unique from facebook
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The foreign key for this comment.
        /// 
        /// Currently this will be one of album or photo id.
        /// </summary>
        [Indexed]
        public string ForeignKey { get; set; }

        /// <summary>
        /// The user id that created the comment
        /// </summary>
        [Indexed]
        public string FromId { get; set; }

        /// <summary>
        /// The user name that created the comment
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// The comment text
        /// </summary>
        public string Message	{ get; set; }

        /// <summary>
        /// The timedate the comment was created
        /// 
        /// Parsed from a string containing ISO-8601 date-time
        /// </summary>
        public DateTime CreatedTime	{ get; set; }

        /// <summary>
        /// The number of times this comment was liked
        /// </summary>
        public int LikeCount { get; set; }

        /// <summary>
        /// This field is returned only if the authenticated user likes this comment
        /// 
        /// Always true
        /// </summary>
        public bool UserLikes { get; set; }
	
        /// <summary>
        /// The type of this object; always returns comment
        /// </summary>
        public string Type	{ get; set; }

        #endregion

        #region Connections

        /// <summary>
        /// All of the likes on this comment.
        /// 
        /// Array of objects containing id and name fields of the user 
        /// who liked the comment.
        /// </summary>
        [Ignore]
        IList<FacebookLikeEntity> Likes { get; set; }

        #endregion

        #endregion

    }
}
