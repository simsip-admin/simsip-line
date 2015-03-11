using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Facebook
{
    /// <summary>
    /// An individual photo as represented in the Graph API.
    /// </summary>
    public class FacebookPhotoEntity
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
        /// The facebook json field name for the tags field.
        /// </summary>
        public static string FIELD_NAME_TAGS = "tags";

        /// <summary>
        /// The facebook json field name for the name field.
        /// </summary>
        public static string FIELD_NAME_NAME = "name";

        /// <summary>
        /// The facebook json field name for the name tags field.
        /// </summary>
        public static string FIELD_NAME_NAME_TAGS = "name_tags";

        /// <summary>
        /// The facebook json field name for the icon field.
        /// </summary>
        public static string FIELD_NAME_ICON = "icon";

        /// <summary>
        /// The facebook json field name for the picture field.
        /// </summary>
        public static string FIELD_NAME_PICTURE = "picture";

        /// <summary>
        /// The facebook json field name for the source field.
        /// </summary>
        public static string FIELD_NAME_SOURCE = "source";

        /// <summary>
        /// The facebook json field name for the height field.
        /// </summary>
        public static string FIELD_NAME_HEIGHT = "height";

        /// <summary>
        /// The facebook json field name for the width field.
        /// </summary>
        public static string FIELD_NAME_WIDTH = "width";

        /// <summary>
        /// The facebook json field name for the images field.
        /// </summary>
        public static string FIELD_NAME_IMAGES = "images";

        /// <summary>
        /// The facebook json field name for the linke field.
        /// </summary>
        public static string FIELD_NAME_LINK = "link";

        /// <summary>
        /// The facebook json field name for the place field.
        /// </summary>
        public static string FIELD_NAME_PLACE = "place";
        
        /// <summary>
        /// The facebook json field name for the created time field.
        /// </summary>
        public static string FIELD_NAME_CREATED_TIME = "created_time";

        /// <summary>
        /// The facebook json field name for the updated time field.
        /// </summary>
        public static string FIELD_NAME_UPDATED_TIME = "updated_time";
        
        /// <summary>
        /// The facebook json field name for the position field.
        /// </summary>
        public static string FIELD_NAME_POSITION = "position";

        #endregion

        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this photo.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The photo ID
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The album ID that contains this photo
        /// </summary>
        [Indexed]
        public string AlbumId { get; set; }

        /// <summary>
        /// The profile (user or page) id that posted this photo
        /// </summary>
        [Indexed]
        public string FromId { get; set; }

        /// <summary>
        /// The profile (user or page) name that posted this photo
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// The tagged users and their positions in this photo
        /// 
        /// Array of objects, the x and y coordinates are percentages 
        /// from the left and top edges of the photo, respectively
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// The user provided caption given to this photo - do not 
        /// include advertising in this field
        /// </summary>
        public string Name	{ get; set; }

        /// <summary>
        /// An array containing an array of objects mentioned in the name field 
        /// which contain the id, name, and type of each object as well as the 
        /// offset and length which can be used to match it up with its corresponding 
        /// string in the name field
        /// </summary>
        public string NameTags { get; set; }

        /// <summary>
        /// The icon that Facebook displays when photos are published to the Feed
        /// 
        /// String representing a valid URL
        /// </summary>
        public string Icon { get; set; }
	
        /// <summary>
        /// The thumbnail-sized source of the photo
        /// 
        /// String representing a valid URL
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// The source image of the photo - currently this can have a maximum width or height of 720px, 
        /// increasing to 960px on 1st March 2012
        /// 
        /// String representing a valid URL
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The height of the photo in pixels
        /// </summary>
        public long Height { get; set; }

        /// <summary>
        /// The width of the photo in pixels
        /// </summary>
        public long Width { get; set; }
	
        /// <summary>
        /// The 4 different stored representations of the photo
        /// 
        /// Array of objects, containing height, width, and source fields
        /// </summary>
        [Ignore]
        public IList<PhotoImage> Images { get; set; }
	
        /// <summary>
        /// A link to the photo on Facebook
        /// 
        /// String representing a valid URL
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Location associated with a Photo, if any
        /// 
        /// Object containing id and name of Page associated with this location, 
        /// and a location field containing geographic information such as latitude, 
        /// longitude, country, and other fields (fields will vary based on geography 
        /// and availability of information)
        /// </summary>
        [Ignore]
        public PhotoPlace Place { get; set; }

        /// <summary>
        /// The time the photo was initially published
        ///
        /// Converted from string containing ISO-8601 date-time
        /// </summary>
        public string CreatedTime	{ get; set; }

        /// <summary>
        /// The last time the photo or its caption was updated
        /// 
        /// Converted from string containing ISO-8601 date-time
        /// </summary>
        public string UpdatedTime	{ get; set; }

        /// <summary>
        /// The position of this photo in the album
        /// </summary>
        public long Position	{ get; set; }

        #endregion

        #region Connections

        [Ignore]
        public IList<FacebookCommentEntity> Comments { get; set; }

        [Ignore]
        public IList<FacebookLikeEntity> Likes { get; set; }

        // TODO: Get other connections

        #endregion

        #region Augmented Fields

        /// <summary>
        ///  Optional field to contain a raw representation of the image.
        ///  
        /// Helpful when you need to work with the image directly - for instance
        /// when adding an image directly to an album.
        /// </summary>
        // TODO
        // public Bitmap RawImage { get; set; }

        #endregion

        #endregion
    }

    /// <summary>
    /// The 4 different stored representations of the photo
    /// </summary>
    public class PhotoImage
    {
        /// <summary>
        /// The height of the photo in pixels
        /// </summary>
        public long Height { get; set; }

        /// <summary>
        /// The width of the photo in pixels
        /// </summary>
        public long Width { get; set; }
        
        /// <summary>
        /// The source image of the photo
        /// 
        /// String representing a valid URL
        /// </summary>
        public string Source { get; set; }
    }

    /// <summary>
    /// Location associated with a Photo, if any
    /// 
    /// Object containing id and name of Page associated with 
    /// this location, and a location field containing geographic information 
    /// such as latitude, longitude, country, and other fields (fields will vary
    /// based on geography and availability of information)
    /// </summary>
    public class PhotoPlace
    {
        // TODO
    }
}
