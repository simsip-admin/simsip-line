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

  File:      Rectangle.cs 
  Summary:   Implements a rectangle, a region delimited
             by two lines of equal Declination, and
             two meridians
  Date:	     August 16, 2005

 
======================================================= */

namespace Spherical.Shape
{
	/// <summary>
	/// Implements a simple region
	/// </summary>
	public class Rectangle  {
        // no instances needed, Microsoft.Performance CA1053
        private Rectangle() {
        }
        public static Convex Make(double ra1, double dec1, double ra2, double dec2)
        {
            //
            // Create four halfspaces. Two great circles for RA and
            // two small circles for DEC

            double dlo, dhi; // offset from center, parameter for constraint
            double declo, dechi;
            double costh, sinth; // sine and cosine of theta (RA) for rotation of vector
            double x, y, z;

            //
            // Halfspaces belonging to declo and dechi are circles parallel
            // to the xy plane, their normal is (0, 0, +/-1)
            //
            // declo halfpsacet is pointing up (0, 0, 1)
            // dechi is pointing down (0, 0, -1)
            if (dec1 > dec2)
            {
                declo = dec2;
                dechi = dec1;
            }
            else
            {
                declo = dec1;
                dechi = dec2;
            }
            dlo = Math.Sin(declo * Constant.Degree2Radian);
            dhi = -Math.Sin(dechi * Constant.Degree2Radian); // Yes, MINUS!
            Convex con = new Convex(4);
            con.Add(new Halfspace(new Cartesian (0.0, 0.0, 1.0, false), dlo)); // Halfspace #1
            con.Add(new Halfspace(new Cartesian (0.0, 0.0, -1.0, false),dhi)); // Halfspace #1

            costh = Math.Cos(ra1 * Constant.Degree2Radian);
            sinth = Math.Sin(ra1 * Constant.Degree2Radian);
            x = -sinth;
            y = costh;
            z = 0.0;
            con.Add(new Halfspace(new Cartesian (x, y, z, false), 0.0));// Halfspace #3

            costh = Math.Cos(ra2 * Constant.Degree2Radian);
            sinth = Math.Sin(ra2 * Constant.Degree2Radian);
            x = sinth;
            y = -costh;
            z = 0.0;

            con.Add(new Halfspace(new Cartesian (x, y, z, false), 0.0));// Halfspace #4
            return con;
        }
	}
}
