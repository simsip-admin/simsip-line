using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Version
{
    public class VersionEntity
    {
        #region Data model

        [PrimaryKey]
        public int VersionId { get; set; }

        public int Version { get; set; }

        #endregion
    }
}
