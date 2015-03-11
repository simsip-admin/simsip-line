using System;
using System.Collections;
using System.Collections.Generic;
using Spherical;
using Spherical.Quickhull;
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

  File:      Chull.cs
  Summary:   Computes the convex hull of a collection of points
	         on the surface of the sphere.
  Date:	     August 10, 2005
 
  
======================================================================= */
namespace Spherical.Shape
{	
	/// <summary>
	/// 
	/// Computes the convex hull of a collection of points
	/// on the surface of the sphere. Uses QuickHull3D 
	/// </summary>
	public sealed class Chull {
        /// <summary>
        /// Various error conditions for unsuccesful convex hull creation
        /// </summary>
        public enum Cherror {
            /// <summary>
            /// operation was succesful
            /// </summary>
            Ok,
            /// <summary>
            /// points were coplanar, convex hull undefined
            /// </summary>
            Coplanar,
            /// <summary>
            /// points can not be confined to any hemisphere, convex hull undefined
            /// </summary>
            BiggerThanHemisphere,
            /// <summary>
            /// The required number of points were not present
            /// </summary>
            NotEnoughPoints,
            /// <summary>
            /// Other error, not defined
            /// </summary>
            Unkown
        }
        /// <summary>
        /// Create a convex spherical polygon from the spherical convex hull of the given points 
        /// </summary>
        /// <param name="xs">list of x coords</param>
        /// <param name="ys">list of y coords</param>
        /// <param name="zs">list of z coords</param>
        /// <param name="err">error code</param>
        /// <returns>a new Convex object</returns>
        public static Convex Make(List<double> xs, List<double> ys, List<double> zs, out Cherror err) {
            Convex con = PointsToConvex(xs, ys, zs, out err);
            return con;
        }
        /// <summary>
        /// Do not allow default constructor
        /// </summary>
		private Chull(){
		}
        public static Convex ToConvex(IList<Cartesian> points, out Cherror err) {
            Cartesian origin = new Cartesian(0.0, 0.0, 0.0, false);
            Quickhull.QuickHull3D qh = new QuickHull3D();
            Cartesian planeNormal;
            Convex con = null;
            //
            // Ingest points into input
            //
            if(points.Count < 3) {
                err = Cherror.NotEnoughPoints;
                return con; // Should still be null
            }
            err = Cherror.Ok; // benefit of doubt;
            //
            // Add the origin too
            //
            points.Add(new Cartesian(0.0, 0.0, 0.0, false));
            try {
                qh.build(points);
                // See if the origin is in it
                //
                //
                foreach (Quickhull.Face face in qh.GetFaceList()) {
                    //
                    //
                    double D = face.distanceToPlane(origin);
                    if (D >= -Spherical.Htm.Trixel.DblTolerance) {

                        planeNormal = face.getNormal();
                        if (con == null) {
                            con = new Convex();
                        }
                        con.Add(new Halfspace(-planeNormal.X, -planeNormal.Y, -planeNormal.Z, true, 0.0));
                    }
                }
                if (con == null) {
                    err = Cherror.BiggerThanHemisphere;
                }
            } catch (Quickhull.QuickHull3D.CoplanarException) {
                err = Cherror.Coplanar;
                return null;
            } catch (Exception) {
                err = Cherror.Unkown;
                throw;
            }
            return con;
        }
        /// <summary>
        /// Internal function for which Make(...) serves as a public wrapper
        /// </summary>
        /// <param name="xs">list of x coorsd</param>
        /// <param name="ys">list of y coords</param>
        /// <param name="zs">list of z coords</param>
        /// <param name="err">error code</param>
        /// <returns></returns>
        internal static Convex PointsToConvex(List<double> xs, List<double> ys, List<double> zs, out Cherror err) {
            Cartesian origin = new Cartesian(0.0, 0.0, 0.0, false);
            Quickhull.QuickHull3D qh = new QuickHull3D();
            Cartesian planeNormal;
            Convex con = null;
            //
            // Ingest points into input
            //
            if(xs.Count < 3) {
                err = Cherror.NotEnoughPoints;
                return con; // Should still be null
            }
            err = Cherror.Ok; // benefit of doubt;
            //
            // Add the origin too
            //
            xs.Add(0.0);
            ys.Add(0.0);
            zs.Add(0.0);
            try {
                qh.build(xs, ys, zs);
                // See if the origin is in it
                //
                //
                foreach (Quickhull.Face face in qh.GetFaceList()) {
                    //
                    //
                    double D = face.distanceToPlane(origin);
                    if (D >= -Spherical.Htm.Trixel.DblTolerance) {

                        planeNormal = face.getNormal();
                        if (con == null) {
                            con = new Convex();
                        }
                        con.Add(new Halfspace(-planeNormal.X, -planeNormal.Y, -planeNormal.Z, true, 0.0));
                    }
                }
                if (con == null) {
                    err = Cherror.BiggerThanHemisphere;
                }
            } catch (Quickhull.QuickHull3D.CoplanarException) {
                err = Cherror.Coplanar;
                return null;
            } catch (Exception) {
                err = Cherror.Unkown;
                throw;
            }
            return con;
        }
	}
}


