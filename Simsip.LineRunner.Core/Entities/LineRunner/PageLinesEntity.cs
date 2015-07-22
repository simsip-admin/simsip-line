using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class PageLinesEntity
    {
        /// <summary>
        /// The product id that this entries belong to.
        /// </summary>
        public string ProductId { get; set; }
        
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
        /// The name of the model used to represent this line.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// A string matching the LineType enumeration.
        /// </summary>
        public string LineType { get; set; }
    }
}
