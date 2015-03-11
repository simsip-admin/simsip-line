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

  File:      SmartVertex.cs
  Summary:   A smart vertex has 
  Date:	     Jan 16, 2007

======================================================= */
namespace Spherical.Htm {
    
    /// <summary>
    /// Smartvertex is aware of its context represented by a list of Wedge
    /// objects derived from the arcs in the outline.
    /// </summary>
    public class SmartVertex {
        static readonly double costol = Constant.CosTolerance;
        static readonly double sintol = Constant.SinTolerance;
        Cartesian up, west;
       
        Cartesian v;
        Topo topo;
        /// <summary>
        /// list of arcs in outline that contain the vertex.
        /// Used in SetParentArcsAndTopo before wedge list is made. 
        /// </summary>
        internal List<Wedge> wedgelist;
        /// <summary>
        /// Get position angle of arc going through this vertex
        /// </summary>
        /// Angle is positive (right hand rule curled) from the "up" direction
        /// <param name="a"></param>
        /// <returns>Radians</returns>
        double GetArcPosAngle(Arc a) {
            double gamma, xproj, yproj;

            yproj = -a.Circle.Vector.Dot(up);
            xproj = a.Circle.Vector.Dot(west);
            gamma = Math.Atan2(yproj, xproj);
            return gamma;
        }
        /// <summary>
        /// Create a Wedge object for a pair of arcs coming in and out of this vertex
        /// </summary>
        /// called by SetLocalTopo in SmartTrixel
        /// <param name="arcin"></param>
        /// <param name="arcout"></param>
        /// <returns></returns>
        internal Wedge makeOneWedge(Arc arcin, Arc arcout){
            PositionAngle pain, paout;
            Wedge result;
            double gamma;

            gamma = GetArcPosAngle(arcin) + Math.PI;
            pain = new PositionAngle(gamma, PositionAngle.Direction.Begin); //-1);

            gamma = GetArcPosAngle(arcout);
            paout = new PositionAngle(gamma, PositionAngle.Direction.End); // 1);
            
            // Make wedge 
            result = new Wedge(paout.Angle, pain.Angle);
            return result;
        }
        /// <summary>
        /// Return sorted list  of angles of arcs in the tangent plane as measured from up (north)
        /// in the positive direction in radians.
        /// </summary>
        /// <param name="parentArcs"></param>
        /// <returns></returns>
        private List<PositionAngle> GetParentAngles(List<Arc> parentArcs) {
            List<PositionAngle> parentAngles = new List<PositionAngle>(2 * parentArcs.Count);
            double arcangle;
            double gamma;
            foreach(Arc a in parentArcs) {
                // determine incoming, outgoing or through going arc
                arcangle = a.GetAngle(this.Vertex);
                gamma = GetArcPosAngle(a);
                if(a.IsFull) {
                    //
                    // Whole Sphere is a special case!
                    //
                    if(a.Circle.Cos0 < -1.0 + Trixel.DblTolerance) {
                        parentAngles.Add(new PositionAngle(0.0, PositionAngle.Direction.End));
                        parentAngles.Add(new PositionAngle(2.0 * Math.PI , PositionAngle.Direction.Begin));
                    } else {

                        parentAngles.Add(new PositionAngle(gamma, PositionAngle.Direction.End)); //1));
                        // then, the incoming
                        gamma += Math.PI;
                        parentAngles.Add(new PositionAngle(gamma, PositionAngle.Direction.Begin)); //-1));
                    }
                } else {
                    //
                    // TODO: Sensitive tolerances!
                    if(arcangle < Trixel.Epsilon2) {
                        // outgoing arc
                        parentAngles.Add(new PositionAngle(gamma, PositionAngle.Direction.End)); //1));

                    } else if(arcangle > a.Angle - Trixel.Epsilon2) {
                        // incoming arc
                        gamma += Math.PI;
                        parentAngles.Add(new PositionAngle(gamma, PositionAngle.Direction.Begin));//-1));
                    } else {
                        // through is divided into in and out
                        // first, the outgoing arc
                        parentAngles.Add(new PositionAngle(gamma, PositionAngle.Direction.End));//1));
                        // then, the incoming
                        gamma += Math.PI;
                        parentAngles.Add(new PositionAngle(gamma, PositionAngle.Direction.Begin)); //-1));
                    }
                }
            }
            parentAngles.Sort(PositionAngle.CompareTo);
            return parentAngles;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentArcs"></param>
        void makeWedgeList(List<Arc> parentArcs) {
            this.wedgelist = new List<Wedge>();
            // angles from up (north) in positive PositionAngle.Direction
            List<PositionAngle> parentAngles = GetParentAngles(parentArcs);
            // Make wedge list
            int count = parentAngles.Count;
            if(count < 2)
                return;

            int i;
            int firstStarter = -1;
            for(i = 0; i < parentAngles.Count; i++) {
                if(parentAngles[i].State == PositionAngle.Direction.End){ //1) { // Out starting
                    firstStarter = i;
                    break;
                }

            }
            if(firstStarter < 0)
                return;

            // ow go from firstStarter to fistStarter - 1 MOD count.
            // Count should always be even
            i = firstStarter;
            for(int k = 0; k < count; k += 2) {
                //
                // wedge is parentAngles[i], parentAngles[i+1]
                // aka out, in
                int next = (i + 1) % count;
                wedgelist.Add(new Wedge(parentAngles[i].Angle, parentAngles[next].Angle));
                i += 2;
            }
        }
        [CLSCompliant(true)]
        /// <summary>
        /// Sets or gets smartvertex's topo value
        /// </summary>
        public Topo Topo {
            get { return topo; }
            set { topo = value; }
        }
        [CLSCompliant(true)]
        /// <summary>
        /// Set this vertex Topo and keep all arcs that 
        /// contain this vertex on their edges
        /// 
        /// </summary>
        /// <param name="plist"></param>
        /// <returns></returns>
        public void SetParentArcsAndTopo(IList<IPatch> plist, Spherical.Region reg) {
            this.topo = reg.Contains(this.Vertex, costol, sintol) ? Topo.Inner : Topo.Outer;
            //
            // If outer (not inner), there is nothing else to do
            //
            if(this.topo != Topo.Inner) {
                return;
            }
            //
            // Save all arcs on which this vertex is resting
            //
            List<Arc> parentArcs = new List<Arc>();

            foreach(IPatch p in plist) {
                foreach(Arc a in p.ArcList) {
                    if(a.ContainsOnEdge(this.Vertex)) {
                        parentArcs.Add(a);
                        this.topo = Topo.Same;
                    }
                }
            }
            makeWedgeList(parentArcs);

        }
        /// <summary>
        /// Vertex is inside the region if it is either on the border or really inside
        /// </summary>
        public bool IsInside {
            get {
                return (topo == Topo.Inner || topo == Topo.Same);
            }
            set {
                if(value) {
                    topo = Topo.Inner;
                } else {
                    topo = Topo.Outer;
                }
            }
        }
        /// <summary>
        /// Property to expose the cartesian part of this smart vertex
        /// </summary>
        public Cartesian Vertex {
            get { return v; }
        }
        /// <summary>
        /// Constructor may force the topo of smartvertex
        /// </summary>
        /// <param name="v"></param>
        /// <param name="inside"></param>
        [CLSCompliant(true)]
        public SmartVertex(Cartesian v, bool inside) : this(v) {
            if(inside) {
                this.topo = Topo.Inner;
            } else {
                this.topo = Topo.Outer;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        [CLSCompliant(false)]
        public SmartVertex(Cartesian v) {
            this.v = v;
            this.topo = Topo.Intersect; // TODO: NEED an undefined enum
            this.v.Tangent(out this.west, out this.up);
        }
    }
}
