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
        /// The product id that this obstacle belongs to.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// The name of the model to represent this obstacle - also the primary key.
        /// </summary>
        [PrimaryKey]
        public string ModelName { get; set; }

        /// <summary>
        /// If set, represents a family of textures the obstacle can choose
        /// from to texture itself with.
        /// </summary>
        public string TextureFamily { get; set; }

        /// <summary>
        /// If set, maps to an enumeration for a particle effect type for the obstacle to use when hit.
        /// </summary>
        public string HitParticleType { get; set; }

        /// <summary>
        /// If set, maps to an enumeration for a particle effect type for the obstacle to use when displayed if 
        /// the PageObstaclesEntity.DisplayParticle dictates so.
        /// </summary>
        public string DisplayParticleType { get; set; }

        /// <summary>
        /// The X offset in model coordinates to center the display particle effect at.
        /// </summary>
        public float DisplayParticleModelX { get; set; }

        /// <summary>
        /// The Y offset in model coordinates to center the display particle effect at.
        /// </summary>
        public float DisplayParticleModelY { get; set; }

        /// <summary>
        /// If set, maps to an enumeration for a spritesheet type for the obstacle to use when displayed.
        /// </summary>
        public string DecalType { get; set; }

        /// <summary>
        /// The X offset in model coordinates for the origin of an optional spritesheet to play on the model.
        /// </summary>
        public float DecalOriginX { get; set; }

        /// <summary>
        /// The Y offset in model coordinates for the origin of an optional spritesheet to play on the model.
        /// </summary>
        public float DecalOriginY { get; set; }

        /// <summary>
        /// The Z offset in model coordinates for the origin of an optional spritesheet to play on the model.
        /// </summary>
        public float DecalOriginZ { get; set; }

        /// <summary>
        /// The width in model coordinates for an optional spritesheet to play on the model.
        /// </summary>
        public float DecalWidth { get; set; }

        /// <summary>
        /// The height in model coordinates for an optional spritesheet to play on the model.
        /// </summary>
        public float DecalHeight { get; set; }
    }
}
