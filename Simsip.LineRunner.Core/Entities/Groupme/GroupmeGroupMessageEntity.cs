using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Groupme
{
    public class GroupmeGroupMessageEntity
    {
        /// <summary>
        /// The id created on the device to identify this group message.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify this group message.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The id of the group that owns this group message.
        /// 
        /// This will be populated with the ServerId from the GroupModel.
        /// </summary>
        [Indexed]
        public string GroupId { get; set; }
        
        /// <summary>
        /// A field populated with a GUID on the device at creation time of 
        /// this group message.
        /// 
        /// This helps detect duplicates in certain operations.
        /// </summary>
        public string SourceGuid { get; set; }
        
        /// <summary>
        /// The date and time this group message was created.
        /// </summary>
        [Indexed]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The user id of the user who created this group message.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The name of the user who created this group message.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The avatar url of the user who created this group message.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// The text of the group message.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///  A flag indicating if this group message is a system message.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// A comma separated list of user ids who have favorited this group
        /// message.
        /// </summary>
        public string FavoritedBy { get; set; }

        /// <summary>
        /// The count of favorites for this message.
        /// </summary>
        public int FavoritedCount { get; set; }

        /// <summary>
        /// An optional picture sent with this message.
        /// 
        /// TODO: Remove once full blown attachments are in place
        /// </summary>
        public string PictureUrl { get; set; }

        /// <summary>
        /// A dynamically created collection of attachments for this message.
        /// </summary>
        [Ignore]
        public List<GroupmeMessageAttachmentEntity> Attachments {get; set; }
    }
}
