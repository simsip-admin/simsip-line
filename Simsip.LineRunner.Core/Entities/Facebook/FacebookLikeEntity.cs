using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Facebook
{
    public class FacebookLikeEntity
    {
        #region Json Field Names
        
        /// <summary>
        /// The facebook json field name for the id field.
        /// </summary>
        public static string FIELD_NAME_ID = "id";

        /// <summary>
        /// The facebook json field name for the name field.
        /// </summary>
        public static string FIELD_NAME_NAME = "name";	

        #endregion

        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this like.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The facebook id for this like.
        /// 
        /// This should be unique from facebook.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// The foreign key for this like.
        /// 
        /// Currently this will be one of album or photo id.
        /// </summary>
        [Indexed]
        public string ForeignKey { get; set; }

        /// <summary>
        /// The user name for this like
        /// </summary>
        public string Name { get; set; }

        #endregion

        #endregion
    }
}
