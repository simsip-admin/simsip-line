using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace Simsip.LineRunner.Entities.LineRunner
{
    public class CharacterEntity
    {
        /// <summary>
        /// The product id that this character belongs to.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// The name of the model to represent this character - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ModelName { get; set; }
    }
}
