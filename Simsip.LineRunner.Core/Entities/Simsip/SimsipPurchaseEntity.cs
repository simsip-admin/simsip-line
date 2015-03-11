using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Simsip
{
    public class SimsipPurchaseEntity
    {
        #region DataModel

        /// <summary>
        /// The id created on the device to identify this purchase.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify this purchase.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The simsip id of the user who purchased.
        /// </summary>
        public string SimsipUserId { get; set; }

        /// <summary>
        /// The product id of the product purchased.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// The date of the purchase.
        /// </summary>
        public string CreatedDate { get; set; }

        #endregion

    }
}





