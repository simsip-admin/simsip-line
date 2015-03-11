using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
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

  File:      SmarStuff.cs
  Summary:   Various Smart helper objects, each of which are small enough
             so they can all be lumped into the same file
  Date:	     Jan 16, 2007

======================================================= */
namespace Spherical.Htm {
    /// <summary>
    /// Smart arcs encapsulate Arcs, and add HtmID tags
    /// to their endpoints
    /// 
    /// </summary>
    class SmartArc {
        public Arc Arc;
        internal Int64 Hid1;
        internal Int64 Hid2;
        internal SmartArc(Arc a) {
            this.Arc = a;
            this.Hid1 = Trixel.CartesianToHid20(a.Point1);
            this.Hid2 = Trixel.CartesianToHid20(a.Point2);// nan input is ok
        }
    }

    /// <summary>
    /// Sortable roots are intesections of arcs with other arcs,
    /// Usually a patch element and the edge of a trixel.
    /// </summary>
    /// Sortable roots are intervals, because aligned patch arcs and trixel edges
    /// have not one or two , but an interval of common points. When the intersection is 
    /// normal, then the Lower == Upper
    public class SortableRoot {
        internal double Lower;
        internal double Upper;
        /// <summary>
        /// 
        /// </summary>
        public Arc ParentArc;
        public Topo topo;
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="arc"></param>
        /// <param name="top"></param>
        public SortableRoot(double low, double high, Arc arc, Topo top) {
            this.Lower = low;
            this.Upper = high;
            this.ParentArc = arc;
            this.topo = top;
            // this.incoming = false; // COMPILER AUTOMATICALLY MAKES IT FALSE  CA1805 
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="arc"></param>
        public SortableRoot(double angle, Arc arc)
            : this(angle, angle, arc, Topo.Intersect) {
        }
        /// <summary>
        /// True if Sortable Root is or conatins 0
        /// </summary>
        /// <returns></returns>
        public bool isZero() {
            return (Lower <= Trixel.DblTolerance);
        }
        /// <summary>
        /// True if Sortable Root is or conatins 1
        /// </summary>
        /// <returns></returns>
        public bool isOne() {
            return (Upper >= 1.0 - Trixel.DblTolerance);
        }
        /// <summary>
        /// Display as text
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() {
            char letter;
            char inc;

            if (this.topo == Topo.Intersect) {
                letter = 'X';
            } else if (topo == Topo.Same) {
                letter = 'S';
            } else if (topo == Topo.Inverse) {
                letter = 'I';
            } else {
                letter = '?';
            }
            inc = ' ';
            if (this.Upper - this.Lower < Trixel.DblTolerance) {
                return String.Format("{0}{2}:({1})", letter, this.Lower, inc);
            }
            return String.Format("{0}{3}:(_{1}  _{2})", letter, this.Lower, this.Upper, inc);
        }

    }
    /// <summary>
    /// An angle quantity augmented with state information
    /// </summary>
    class PositionAngle {
        internal enum Direction {
            Begin,
            End,
            Undefined
        }
        internal double Angle;
        internal PositionAngle.Direction State;
        /// <summary>
        /// Constructor. Specifies both quantities
        /// </summary>
        /// <param name="angle">in radians</param>
        /// <param name="state">Begin, End or Undefined</param>
        internal PositionAngle(double angle, Direction state) {
            if (angle < 0.0) {
                this.Angle = angle + 2.0 * Math.PI;
            } else {
                this.Angle = angle;
            }
            this.State = state;
        }
        /// <summary>
        /// Comparator for sorting
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CompareTo(PositionAngle a, PositionAngle b) {
            return a.Angle.CompareTo(b.Angle);
        }
    }
}
