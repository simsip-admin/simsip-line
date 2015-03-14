using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace Simsip.LineRunner.Entities.LineRunner
{
    public class TextureEntity
    {
        /// <summary>
        /// The unique id for this TextureEntity.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int TextureId { get; set; }

        /// <summary>
        /// The name of the model we are designating this texture for.
        /// </summary>
        [Indexed]
        public string ModelName { get; set; }

        /// <summary>
        /// The BasicEffect id corresponding to the _originalEffects collection.
        /// </summary>
        public int TexturePosition { get; set; }

        /// <summary>
        /// The name of the texture to load.
        /// </summary>
        public string TextureName { get; set; }
    }
}
