using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Facebook
{
    public class FacebookUserEntity
    {
        #region Factory
        
        /// <summary>
        /// A factory method to centralize creating a facebook user model.
        /// 
        /// Pass in the values that you have available. The factory method will
        /// make sure required fields have been included and return null if a required field
        /// is missing.
        /// </summary>
        /// <returns>A newly created facebook User model or null if a required field was
        /// missing or an error occurred.</returns>
        public FacebookAlbumEntity CreateUser()
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
        /// The facebook json field name for the name field.
        /// </summary>
        public static string FIELD_NAME_NAME = "name";	
        
        /// <summary>
        /// The facebook json field name for the username field.
        /// </summary>
        public static string FIELD_NAME_USERNAME = "username";
        
        /// <summary>
        /// The facebook json field name for the last_name field.
        /// </summary>
        public static string FIELD_NAME_LAST_NAME = "last_name";
        
        /// <summary>
        /// The facebook json field name for the first_name field.
        /// </summary>
        public static string FIELD_NAME_FIRST_NAME = "first_name";	
        
        /// <summary>
        /// The facebook json field name for the middle_name field.
        /// </summary>
        public static string FIELD_NAME_MIDDLE_NAME = "middle_name";
        
        /// <summary>
        /// The facebook json field name for the gender field.
        /// </summary>
        public static string FIELD_NAME_GENDER = "gender";
        
        /// <summary>
        /// The facebook json field name for the email field.
        /// </summary>
        public static string FIELD_NAME_EMAIL = "email";
        
        /// <summary>
        /// The facebook json field name for the website field.
        /// </summary>
        public static string FIELD_NAME_WEBSITE = "website";
        
        /// <summary>
        /// The facebook json field name for the picture field.
        /// </summary>
        public static string FIELD_NAME_PICTURE = "picture";

        /// <summary>
        /// The facebook json field name for the is_silhouette field.
        /// </summary>
        public static string FIELD_NAME_IS_SILHOUETTE = "is_silhouette";

        #endregion

        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this user.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The user's Facebook ID
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// The user's full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user's Facebook username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The user's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The user's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The user's middle name
        /// </summary>
        public string MiddleName	{ get; set; }

        /// <summary>
        /// The user's gender: female or male
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The proxied or contact email address granted by the user.
        /// 
        /// String containing a valid RFC822 email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The URL of the user's personal website.
        /// 
        /// String containing a valid URL
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// The URL of the user's profile pic 
        /// (only returned if you explicitly specify a 'fields=picture' param).
        /// 
        /// String; If the "October 2012 Breaking Changes" migration setting is 
        /// enabled for your app, this field will be an object with the url and 
        /// is_silhouette fields; is_silhouette is true if the user has not 
        /// uploaded a profile picture.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// See description for Picture
        /// </summary>
        public bool IsSilhouette { get; set; }

        #endregion

        #region Connections
        
        // None at this time. Current thinking is we will get connections in an isolated
        // manner based on facebook id we determine from logged in SimsipUser. We may 
        // revisit this.

        #endregion

        #endregion
    }
}
