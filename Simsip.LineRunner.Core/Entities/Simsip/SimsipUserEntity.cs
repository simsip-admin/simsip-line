using System;
using System.Collections.Generic;
using SQLite;

namespace Simsip.LineRunner.Entities.Simsip
{
    public class SimsipUserEntity
    {
        #region DataModel

        /// <summary>
        /// The id created on the device to identify this user.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The id created on the server to identify user.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        public string FacebookAccessToken { get; set; }

        public string FacebookRefreshToken { get; set; }

        public string FacebookId { get; set; }

        public string YoutubeAccessToken { get; set; }

        public string YoutubeRefreshToken { get; set; }

        public string YoutubeId { get; set; }

        public string GroupmeAccessToken { get; set; }

        public string GroupmeRefreshToken { get; set; }

        public string GroupmeId { get; set; }

        #endregion

    }
}





