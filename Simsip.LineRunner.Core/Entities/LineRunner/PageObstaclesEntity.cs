using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class PageObstaclesEntity
    {
        /// <summary>
        /// The page number we are describing.
        /// </summary>
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
        /// The name of the model used to represent this obstacle.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// A string matching the ObstacleType enumeration.
        /// </summary>
        public string ObstacleType { get; set; }

        /// <summary>
        /// The X position in logical coordinates that will place the origin of the
        /// obstacle at.
        /// </summary>
        public float LogicalXScaledTo100 { get; set; }

        /// <summary>
        /// The logical height we will truncate this obstacle to.
        /// </summary>
        public float LogicalHeightScaledTo100 { get; set; }

        /// <summary>
        /// The logical angle we will rotate this obstacle to.
        /// </summary>
        public float LogicalAngle { get; set; }

        /// <summary>
        /// Whether this obstacle should be marked as goal for scoring purposes.
        /// </summary>
        public bool IsGoal { get; set; }
    }
}
