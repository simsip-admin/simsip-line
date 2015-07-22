using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class RandomObstaclesEntity
    {
        /// <summary>
        /// The product id that this entries belong to.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// The name of the random set to represent these obstacles - also the primary key.
        /// 
        /// IMPORTANT: The naming represents both a count and version:
        ///            "Random"<count of goals><version of this set of count of goals>
        /// Examples: 
        /// Random0101 - Specifies the set representing 1 goal, first version of this category
        /// Random0403 - Specifies the set representing 4 goasls, third version of this category
        /// 
        /// Also note that in PageObstaclesRepository, this may be specified as Random<count>x
        /// (e.g., Random04x). In this case, we are specifing choose a random set from the count
        /// 4 category.
        /// </summary>
        [Indexed]
        public string RandomObstaclesSet { get; set; }

        /// <summary>
        /// The obstacle number in a particular set we are describing.
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
        /// A height range we can apply to each obstacle to add additional randomization.
        /// 
        /// IMPORTANT: Needs to be int as the Random class we use in the code only has 
        /// a signature for generating a range between to ints - so to keep it simple
        /// will stick with ints here.
        /// 
        /// Example: 10
        /// Pick a value between -10 to 10 to add to the logical height of each obstacle.
        /// </summary>
        public int HeightRange { get; set; }

        /// <summary>
        /// If non-zero, an additional percentage scaling we will apply to the obstacle.
        /// </summary>
        public float LogicalScaleScaledTo100 { get; set; }

        /// <summary>
        /// The logical angle we will rotate this obstacle to.
        /// </summary>
        public float LogicalAngle { get; set; }

        /// <summary>
        /// A string containing coded values that direct how the display particle effect
        /// should be created for this obstacle.
        /// Currently:
        /// emppty - do not display the display particle effect defined for this obstacle
        /// y(es) - display the display particle effect defined for this obstacle
        /// r(andom) - choose from a family of particle effects to display for this obstacle
        /// m(ultiple) - display multiple display particle effects defined for this obstacle
        /// specific enum - used to override the default display particle effect, display the particle effect specified by the enum
        /// </summary>
        public string DisplayParticle { get; set; }

        /// <summary>
        /// Whether this obstacle should be marked as goal for scoring purposes.
        /// </summary>
        public bool IsGoal { get; set; }

    }
}
