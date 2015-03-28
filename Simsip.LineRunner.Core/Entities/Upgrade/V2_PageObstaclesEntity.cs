using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.Upgrade
{
    public class V2_PageObstaclesEntity
    {
        /// <summary>
        /// The page number we are describing.
        /// </summary>
        [Indexed]
        public int PageNumber { get; set; }

        /// <summary>
        /// The line number on a particular page we are describing.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The obstacle number on a particular page we are describing.
        /// </summary>
        public int ObstacleNumber { get; set; }

        /// <summary>
        /// An additional random height range which can be added to height.
        /// </summary>
        public int HeightRange { get; set; }
    }
}
