using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class ResourcePackEntity
    {
        /// <summary>
        /// The name of the resource pack to load - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ResourcePackName { get; set; }

        /// <summary>
        /// A user friendly name for the resource pack.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The individual(s) to give credit for the source resource pack.
        /// </summary>
        public string Credit { get; set; }

        /// <summary>
        /// The url containting the copyright that allows us to use the source resource pack.
        /// </summary>
        public string CopyrightUrl { get; set; }


    }
}
