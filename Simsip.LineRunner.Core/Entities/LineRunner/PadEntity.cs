using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class PadEntity
    {
        /// <summary>
        /// The name of the model to load for this page - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ModelName { get; set; }

        /// <summary>
        /// The name of the pad to display to the user for selection purposes.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Number of lines to display on page
        /// </summary>
        public int LineCount { get; set; }

        /// <summary>
        /// X position of page in model coordinates
        /// </summary>
        public float ModelStartX { get; set; }
    
        /// <summary>
        /// Y position of page in model coordinates
        /// </summary>
        public float ModelStartY  { get; set; }             
        
        /// <summary>
        /// Height of page header in model coordinates
        /// </summary>
        public float ModelHeaderMargin { get; set; }

        /// <summary>
        /// Height of page footer in model coordinates
        /// </summary>
        public float ModelFooterMargin { get; set; }
    }
}
