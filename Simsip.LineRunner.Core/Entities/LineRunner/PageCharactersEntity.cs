using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace Simsip.LineRunner.Entities.LineRunner
{
    public class PageCharactersEntity
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
        /// The character number on a particular page we are describing.
        /// </summary>
        public int CharacterNumber { get; set; }

        /// <summary>
        /// The name of the model used to represent this character.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// A string matching the CharacterType enumeration.
        /// </summary>
        public string CharacterType { get; set; }

        /// <summary>
        /// The X position in logical coordinates that will place the origin of the
        /// character at.
        /// </summary>
        public int LogicalX { get; set; }

        /// <summary>
        /// The Y position in logical coordinates that will place the origin of the
        /// character at.
        /// </summary>
        public int LogicalY { get; set; }
    }
}
