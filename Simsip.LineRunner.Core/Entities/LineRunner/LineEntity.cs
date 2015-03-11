using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace Simsip.LineRunner.Entities.LineRunner
{
    public class LineEntity
    {
        /// <summary>
        /// The name of the model to represent this line - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ModelName { get; set; }

        /// <summary>
        /// The name of the pad to display to the user for selection purposes.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
