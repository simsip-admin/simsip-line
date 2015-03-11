using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    public class FacebookSubscriptionEntity
    {
        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this subscription.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify this subscription.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The user ID of the subscribing user in a particular subscription link
        /// </summary>
        [Indexed]
        public string SubscriberId { get; set; }

        /// <summary>
        /// The user ID of the subscribed-to user in a particular subscription link
        /// </summary>
        [Indexed]
        public string SubscribedId { get; set; }

        #endregion

        #endregion
    }
}
