using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class ObstacleEntity
    {
        /// <summary>
        /// The name of the model to represent this obstacle - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ModelName { get; set; }
    }
}
