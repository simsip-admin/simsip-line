/*  
 Copyright © 2009 Project Mercury Team Members (http://mpe.codeplex.com/People/ProjectPeople.aspx)

 This program is licensed under the Microsoft Permissive License (Ms-PL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://mpe.codeplex.com/license.
*/

namespace ProjectMercury.Modifiers
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Simsip.LineRunner.Utils;
    using Engine.Graphics;
    using BEPUphysicsDemos;

    /// <summary>
    /// Defines a Modifier which adjusts the scale of a Particle over its lifetime.
    /// </summary>
#if WINDOWS
    [TypeConverter("ProjectMercury.Design.Modifiers.ScaleModifierTypeConverter, ProjectMercury.Design")]
#endif
    public class Scale3DModifier : Modifier
    {
        /// <summary>
        /// The initial scale of the Particle in pixels.
        /// </summary>
        public float InitialDepth;

        /// <summary>
        /// The camera type we will used to determine the current depth from.
        /// </summary>
        public XNAUtils.CameraType TheCameraType;

        /// <summary>
        /// The camera used to determine depth during updates.
        /// </summary>
        public Camera TheCamera;

        /// <summary>
        /// Returns a deep copy of the Modifier implementation.
        /// </summary>
        /// <returns></returns>
        public override Modifier DeepCopy()
        {
            return new Scale3DModifier
            {
                InitialDepth = this.InitialDepth,
                TheCameraType = this.TheCameraType
            };
        }

        /// <summary>
        /// Processes the particles.
        /// </summary>
        /// <param name="dt">Elapsed time in whole and fractional seconds.</param>
        /// <param name="particle">A pointer to an array of particles.</param>
        /// <param name="count">The number of particles which need to be processed.</param>
        protected internal override unsafe void Process(float dt, Particle* particle, int count)
        {
            float currentDepth = TheCamera.Position.Z;
            float scale = (this.InitialDepth - currentDepth) / this.InitialDepth;

            for (int i = 0; i < count; i++)
            {
                particle->Scale = scale;

                particle++;
            }
        }
    }
}