using System;
using System.Collections.Generic;
using System.Text;
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

  File:      Wedge.cs
  Summary:   Implements a wedge shaped area described by two directed
             semilines, one starting, and one terminating at thier common point
  Date:	     Jan 16, 2007

======================================================= */
namespace Spherical.Htm {
    /// <summary>
    /// A wedge is an open area defined by a point and and incoming and outgoing semiline
    /// 
    /// It is always used in a relative context, so only the angle from a "northing" direction
    /// int the positive sense is stored for the two semilines.
    /// 
    /// Wedges are used for capturing the topological relationships between arcs that
    /// contain a vertex of a trixel, and the neighborhood of the trixel's vertex.
    /// 
    /// 
    /// </summary>
    internal class Wedge {
        public static readonly double Epsilon = 3.0 * Spherical.Constant.DoublePrecision;
        double lo;
        double hi;
        /// <summary>
        /// Universal wedge, contains everything
        /// </summary>
        internal Wedge() {
            this.lo = 0.0;
            this.hi = 2.0 * Math.PI;
        }
        /// <summary>
        /// outgoing arc (low) and incoming arc (high)
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        internal Wedge(double outangle, double inangle) {
            this.lo = outangle;
            if(inangle < outangle) {
                inangle += 2.0 * Math.PI;
            }
            this.hi = inangle;
        }
        /// <summary>
        /// This wedge can either completely or partially overlap the other wedge, or 
        /// it's relationship is undefined
        /// 
        /// </summary>
        /// <param name="other">other widget to test</param>
        /// <returns>One of Partial, Inner or Undefined</returns>
        internal Markup Compare(Wedge other) {
            double mylo, myhi, otherlo, otherhi;
            mylo = this.lo;
            myhi = this.hi;
            otherlo = other.lo;
            otherhi = other.hi;

            // find the lowest low, subtract from all
            double bias = mylo < otherlo ? mylo : otherlo;

            mylo -= bias;
            myhi -= bias;
            otherlo -= bias;
            otherhi -= bias;

            if(myhi > 2.0 * Math.PI) {
                myhi -= 2.0 * Math.PI;
                mylo -= 2.0 * Math.PI;
            }
            if(mylo > otherlo + Epsilon && mylo < otherhi - Epsilon)
                return Markup.Partial;
            if(myhi > otherlo + Epsilon && myhi < otherhi - Epsilon)
                return Markup.Partial;

            if(mylo <= otherlo + Epsilon && myhi >= otherhi - Epsilon)
                return Markup.Inner;
            return Markup.Undefined;
        }
    }
}
