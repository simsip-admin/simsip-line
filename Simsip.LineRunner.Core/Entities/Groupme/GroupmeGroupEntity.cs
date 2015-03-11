using System;
using SQLite;


namespace Simsip.LineRunner.Entities.Groupme
{
    public enum GroupType
    {
        Private
    }

    public class GroupmeGroupEntity
    {
        /// <summary>
        /// The id created on the device to identify this group.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify this group.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// The name of this group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of this group.
        /// </summary>
        public GroupType Type { get; set; }

        /// <summary>
        /// The description of this group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The image url of this group.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The user id of the creator of this group.
        /// </summary>
        public string CreatorUserId { get; set; }

        /// <summary>
        /// The date and time this group was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time this group was updated.
        /// </summary>
        [Indexed]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The share url of this group.
        /// </summary>
        public string ShareUrl { get; set; }

        /// <summary>
        /// The message count for this group.
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// The last message id for this group.
        /// </summary>
        public string LastMessageId { get; set; }

        /// <summary>
        /// The date and time the last messge for this group was created at.
        /// </summary>
        public DateTime LastMessageCreatedAt { get; set; }

        /// <summary>
        /// The message preview nickname for this group.
        /// </summary>
        public string MessagePreviewNickname { get; set; }

        /// <summary>
        /// The message preview text for this group.
        /// </summary>
        public string MessagePreviewText { get; set; }

        /// <summary>
        /// The message preview url for this group.
        /// </summary>
        public string MessagePreviewImageUrl { get; set; }

        /// <summary>
        /// A dynamically maintained value indicating if the group has unread messages or not.
        /// </summary>
        public bool HasUnreadMessages { get; set; }

        /// <summary>
        /// True if the group is pinned to Start, false if not.
        /// </summary>
        public bool IsPinned { get; set; }
    }
}
