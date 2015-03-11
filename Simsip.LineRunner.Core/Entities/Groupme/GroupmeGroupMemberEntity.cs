using SQLite;


namespace Simsip.LineRunner.Entities.Groupme
{
    public class GroupmeGroupMemberEntity
    {
        /// <summary>
        /// The id created on the device to identify this group member.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id received from the server to identify this group member.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The group id for the group this group member belongs to.
        /// 
        /// This will be populated by the ServerId from the Group model.
        /// </summary>
        [Indexed]
        public string GroupId { get; set; }

        /// <summary>
        /// The user id of this group member.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The nickname of this group member.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Whether this group member is muted or not.
        /// </summary>
        public bool Muted { get; set; }

        /// <summary>
        /// The avatar url for this group member.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
