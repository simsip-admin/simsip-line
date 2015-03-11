using System;
using Spherical;
#region Copyright Gyorgy Fekete
/* 
 * Copyright Alex Szalay, Gyorgy Fekete, Jim Gray 2007
 * The Spherical Geometry and Hierarchical Triangular Mesh libraries and source code are
 * provided as-is with no warranties. The user is hereby granted a non-exclusive license to use,
 * modify and extend these libraries. We ask that the authors (A. Szalay, T. Budavari, G. Fekete
 * at The Johns Hopkins University,  and Jim Gray, Microsoft Research be credited in any publication
 * or derived product that incorporates the whole or any part of our system
 */
#endregion

/*=====================================================================

  File:      Circle.cs
  Summary:   Implements a circular region of interest on the celestial sphere.
  Date:	     August 10, 2005
 
======================================================= */

namespace Spherical.Shape
{
	/// <summary>
	/// Compute a Convex consisting of a single Halfspace based on 
    /// the parameters of the given circle
	/// </summary>
	public class Circle 
	{
        /// <summary>
        /// Make a Convex from the parameters of the given circle
        /// </summary>
        /// <param name="ra">Right Ascensionof center</param>
        /// <param name="dec">Declinationof center</param>
        /// <param name="radius">radius in minutes of arc</param>
        /// <returns>a new Convex object</returns>
        public static Convex Make(double ra, double dec, double radius)
        {
            double x, y, z;
            Cartesian.Radec2Xyz(ra, dec, out x, out y, out z);
            return Make(x, y, z, radius);
        }
        /// <summary>
        /// Make a Convex from the parameters of the given circle 
        /// </summary>
        /// <param name="x">X coord of center on unit sphere</param>
        /// <param name="y">Y coord of center on unit sphere</param>
        /// <param name="z">Z coord of center on unit sphere</param>
        /// <param name="radius">radius in arc minutes</param>
        /// <returns>a new Convex object</returns>
        public static Convex Make(double x, double y, double z, double radius)
        {
            Convex con = new Convex();
            double arg = Math.PI * radius / 10800.0;
            double d;
            d = Math.Cos(arg);
            con.Add(new Halfspace(new Cartesian (x, y, z, false), d));
            return con;
        }
	}
}
