using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Facebook
{
    public class FacebookGroupEntity
    {
        #region Data model

        /// <summary>
        /// The id created on the device to identify this group.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The group ID
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The URL for the group's icon
        /// 
        /// String containing a valid URL
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The URL for the group's cover photo
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// The Id for the group's cover photo
        /// </summary>
        public string CoverId { get; set; }

        /// <summary>
        /// The image offset for the group's cover photo
        /// </summary>
        public string CoverImageOffset { get; set; }

        /// <summary>
        /// The profile Id that created this group
        /// </summary>
        [Indexed]
        public string OwnerId { get; set; }

        /// <summary>
        /// The profile name that created this group
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// The name of the group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A brief description of the group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The URL for the group's website
        /// 
        /// String containing a valid URL
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The privacy setting of the group
        /// 
        /// String containing OPEN, CLOSED, or SECRET
        /// </summary>
        public string Privacy { get; set; }

        /// <summary>
        /// The last time the group was updated
        /// 
        /// String containing ISO-8601 date-time
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        #endregion

        #region Connections

        /// <summary>
        /// This group's wall.
        /// 
        /// Array of Post objects.
        /// </summary>
        [Ignore]
        public IList<FacebookPostEntity> Feed { get; set; }

        #endregion

    }
}
