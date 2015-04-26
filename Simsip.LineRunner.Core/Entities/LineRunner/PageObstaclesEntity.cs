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
        /// A +/- delta amount we can apply to the X position of an obstacle 
        /// to allow it to range over a set of values.
        /// </summary>
        public float XRange { get; set; }

        /// <summary>
        /// The logical height we will truncate this obstacle to.
        /// </summary>
        public float LogicalHeightScaledTo100 { get; set; }

        /// <summary>
        /// A +/- delta amount we can apply to The height of an obstacle
        /// to allow it to range over a set of values.
        /// </summary>
        public float HeightRange { get; set; }

        /// <summary>
        /// If non-zero, an additional percentage scaling we will apply to the obstacle.
        /// </summary>
        public float LogicalScaleScaledTo100 { get; set; }

        /// <summary>
        /// The logical angle we will rotate this obstacle to.
        /// </summary>
        public float LogicalAngle { get; set; }

        /// <summary>
        /// A +/- delta amount we can apply to The angle of an obstacle
        /// to allow it to range over a set of values.
        /// </summary>
        public float AngleRange { get; set; }

        /// <summary>
        /// Whether this obstacle should be marked as goal for scoring purposes.
        /// </summary>
        public bool IsGoal { get; set; }
    }
}
