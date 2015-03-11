using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class ModelEntity
    {
        /// <summary>
        /// The name of the model to represent this object - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ModelName {get; set;}

        /// <summary>
        /// Allows one model to be aliased to different representations.
        /// 
        /// Example: ModelName Pad2 is an alias for ModelAlias Pad11 (the actual model)
        /// </summary>
        public string ModelAlias { get; set; }


        /// <summary>
        /// The X width in model coordinates.
        /// </summary>
        public float ModelWidth { get; set; }

        /// <summary>
        /// The Y height in model coordinates.
        /// </summary>
        public float ModelHeight { get; set; }

        /// <summary>
        /// The Z depth in model coordinates.
        /// </summary>
        public float ModelDepth { get; set; }
    }
}
