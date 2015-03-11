using SQLite;


namespace Simsip.LineRunner.Entities.Groupme
{
    public class MessageAttachmentType
    {
        public const string Background = "background";
        public const string Emoji = "emoji";
        public const string Image = "image";
        public const string Location = "location";
        public const string Sticker = "sticker";
        public const string Video = "video";
    }

    public class GroupmeMessageAttachmentEntity
    {
        /// <summary>
        /// The id created on the device to identify this message attachment.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id received from the server to identify this message attachment.
        /// 
        /// This should be unique.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// The id of the message that owns this message attachment.
        /// 
        /// This will be populated by the ServerId from the GroupMessageModel or the
        /// DirectMessageModel.
        /// </summary>
        [Indexed]
        public string MessageId { get; set; }

        /// <summary>
        /// The type of the message attachment.
        /// </summary>
        [Indexed]
        public string Type { get; set; }

        /// <summary>
        /// If needed, the order an attachment for a particular type should be displayed in.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// If the attachment type has a name associated with it, this will hold its value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If the attachment type has a description associated with it, this will hold its value.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If the attachment type has a url assocated with it, this will hold its value.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// If the attachment type is "location", this will be the latitude of the
        /// location.
        /// </summary>
        public string LocationLat { get; set; }

        /// <summary>
        /// If the attachment type is "location", this will be the longitude of the
        /// location.
        /// </summary>
        public string LocationLng { get; set; }

        /// <summary>
        /// If the attachment type is "emoji", this will be the placeholder used
        /// in the text of the message to represent an emoji should be inserted
        /// here.
        /// </summary>
        public string EmojiPlaceholder { get; set; }

        /// <summary>
        /// If the attachment type is "emoji", this will be a mapping of emoji
        /// pack ids and and an offset into that emoji pack id.
        /// 
        /// The entries will be comma seperated and within each entry the "|"
        /// character will separate the emoji pack id from the offset.
        /// 
        /// Example:
        /// "1|15,1|9,2|18,1|14"
        /// </summary>
        public string EmojiCharmap { get; set; }
    }
}
