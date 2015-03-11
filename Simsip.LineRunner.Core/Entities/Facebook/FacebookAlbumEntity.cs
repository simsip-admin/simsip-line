using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Facebook
{
    public class FacebookAlbumEntity
    {
        #region Factory
        
        /// <summary>
        /// A factory method to centralize creating a facebook album model.
        /// 
        /// Pass in the values that you have available. The factory method will
        /// make sure required fields have been included and return null if a required field
        /// is missing.
        /// </summary>
        /// <returns>A newly created facebook Album model or null if a required field was
        /// missing or an error occurred.</returns>
        public FacebookAlbumEntity CreateAlbum()
        {
            return null;
        }

        #endregion

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
        /// The facebook json field name for the name field.
        /// </summary>
        public static string FIELD_NAME_NAME = "name";	
        
        /// <summary>
        /// The facebook json field name for the description field.
        /// </summary>
        public static string FIELD_NAME_DESCRIPTION = "description";
        
        /// <summary>
        /// The facebook json field name for the location field.
        /// </summary>
        public static string FIELD_NAME_LOCATION = "location";
        
        /// <summary>
        /// The facebook json field name for the link field.
        /// </summary>
        public static string FIELD_NAME_LINK = "link";
        
        /// <summary>
        /// The facebook json field name for the cover photo field.
        /// </summary>
        public static string FIELD_NAME_COVER_PHOTO = "cover_photo";
        
        /// <summary>
        /// The facebook json field name for the privacy field.
        /// </summary>
        public static string FIELD_NAME_PRIVACY = "privacy";
        
        /// <summary>
        /// The facebook json field name for the count field.
        /// </summary>
        public static string FIELD_NAME_COUNT = "count";
        
        /// <summary>
        /// The facebook json field name for the type field.
        /// </summary>
        public static string FIELD_NAME_TYPE = "type";
        
        /// <summary>
        /// The facebook json field name for the created time field.
        /// </summary>
        public static string FIELD_NAME_CREATED_TIME = "created_time";	
        
        /// <summary>
        /// The facebook json field name for the updated time field.
        /// </summary>
        public static string FIELD_NAME_UPDATED_TIME = "updated_time";
        
        /// <summary>
        /// The facebook json field name for the can upload field.
        /// </summary>
        public static string FIELD_NAME_CAN_UPLOAD = "can_upload";

        #endregion

        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this album.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The album id.
        /// 
        /// This should be unique from facebook.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The profile id that created this album
        /// </summary>
        [Indexed]
        public string FromId { get; set; }

        /// <summary>
        /// The profile name that created this album
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// The title of the album
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the album
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The location of the album
        /// </summary>
        public string Location	{ get; set; }

        /// <summary>
        /// A link to this album on Facebook.
        /// 
        /// A string containing a valid URL
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The album cover photo ID
        /// </summary>
        public string CoverPhoto { get; set; }

        /// <summary>
        /// Temporary
        /// </summary>
        public string CoverPhotoThumbnailUrl { get; set; }

        /// <summary>
        /// The privacy settings for the album
        /// </summary>
        public string Privacy { get; set; }

        /// <summary>
        /// The number of photos in this album
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The type of the album: profile, mobile, wall, normal or album
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The time the photo album was initially created
        /// 
        /// A string containing ISO-8601 date-time
        /// </summary>
        public string CreatedTime { get; set; }

        /// <summary>
        /// The last time the photo album was updated
        /// 
        /// A string containing ISO-8601 date-time
        /// </summary>
        public string UpdatedTime	{ get; set; }

        /// <summary>
        /// Determines whether the UID can upload to the album and returns 
        /// true if the user owns the album, the album is not full, and the 
        /// app can add photos to the album.
        /// </summary>
        public bool CanUpload { get; set; }

        /// <summary>
        /// Used to store the time for the last time the album was viewed.
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// A value computed upon an an album refresh to determine if we have any
        /// unread photos in the album. 
        /// 
        /// Uses LastSeen to determine this.
        /// </summary>
        public bool IsUnseen { get; set; }

        #endregion

        #region Connections
        
        /// <summary>
        /// The photos contained in this album
        /// </summary>
        [Ignore]
        public IList<FacebookPhotoEntity> Photos { get; set; }

        /// <summary>
        /// The likes made on this album
        /// </summary>
        [Ignore]
        IList<FacebookLikeEntity> Likes { get; set; }

        /// <summary>
        /// The comments made on this album
        /// </summary>
        [Ignore]
        IList<FacebookCommentEntity> Comments { get; set; }

        /// <summary>
        /// The album's cover photo, the first picture uploaded to an album becomes 
        /// the cover photo for the album.
        /// </summary>
        FacebookPhotoEntity Picture { get; set; }

        #endregion

        #endregion
    }
}
