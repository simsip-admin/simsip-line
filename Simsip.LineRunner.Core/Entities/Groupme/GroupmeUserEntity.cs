using System;
using SQLite;


namespace Simsip.LineRunner.Entities.Groupme
{
    public class GroupmeUserEntity
    {
        /// <summary>
        /// The id created on the device to identify this user.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify this user.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// The phone number for this user.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The image url for this user.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The name for this user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The date and time this user was created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time this user was updated at.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The email for this user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// A flag indicating if this user is enabled for SMS delivery of messages.
        /// </summary>
        public bool Sms { get; set; }
    }
}
