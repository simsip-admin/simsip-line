using System;
using System.Collections.Generic;
using SQLite;
using Newtonsoft.Json;

namespace Simsip.LineRunner.Entities.Simsip
{
    public class SimsipProductEntity
    {
        #region DataModel

        /// <summary>
        /// The id created on the device to identify this product.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify this product.
        /// 
        /// For example, simsip-hub-free or simsip-hub-paid.
        /// </summary>
        [Indexed]
        [JsonProperty(PropertyName = "id")]
        public string ServerId { get; set; }

        /// <summary>
        /// The id representing the product line this product belows to.
        /// 
        /// For example simsip-hub. This helps identify a particular version in
        /// a family of products.
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// The version of this product.
        /// 
        /// The version shall be in maojor.minor. build format (e.g., 1.3.2)
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        ///  A concise description of the product.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// A link to the most up to date description.
        /// </summary>
        [JsonProperty(PropertyName = "link")]
        public string ImageUrl { get; set; }

        #endregion

    }
}





