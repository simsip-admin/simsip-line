#undef ROBUSTTEST

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

  File:      SmarTrixel.cs
  Summary:   An object that represents a trixel in the context of a region.
             Its smarts has to do with its ability to provide its markup in 
             this context. Markup is one of {Inner, Partial, Reject}
  Date:	     Jan 16, 2007

======================================================= */

namespace Spherical.Htm {
    /// <summary>
    /// Smart trixels evaluate their own relation to the region outline
    /// </summary>
    public class SmartTrixel {
        private Topo[] localTopo = new Topo[3];
        private Arc[] edges = new Arc[3];
        private SmartTrixel parent;
        private Cover covmachine; // supplies region, GetSmartArc, used by sortandprune, isinvere?
        private SmartVertex[] v = new SmartVertex[3];
        private Halfspace bc;      // bounding circle of this trixel
        private Int64 hid;
        /// <summary>
        /// The HtmID of this trixel
        /// </summary>
        public Int64 Hid {
            get { return hid; }
        }
        private IList<IPatch> plist = null;
        private List<SortableRoot>[] fractionals = new List<SortableRoot>[3]; // one for each edge

        internal bool Terminal;
        internal int Level;

        static readonly double costol = Constant.CosTolerance;
        static readonly double sintol = Constant.SinTolerance;

#if DEBUG
        private static Int64 watchHid = 13L;
        private bool stopcond() {
            return (this.hid == watchHid);
        }
#endif
        private bool rootCheck(SortableRoot root) {
            if(root.Lower > root.Upper)
                return false;
            double zero = -Trixel.Epsilon;
            double one = 1 + Trixel.Epsilon;
            if(root.Lower < zero)
                return false;
            if(root.Upper < zero)
                return false;
            if(root.Lower > one)
                return false;
            if(root.Upper > one)
                return false;
            return true;
        }
#if DEBUG
        /// <summary>
        /// Throw exception if the fractionals are not within range (0,1)
        /// with tolerances
        /// </summary>
        private void sanitycheck() {
            for(int i = 0; i < 3; i++) {
                if(fractionals[i].Count > 0) {
                    List<SortableRoot> rootlist = fractionals[i];
                    foreach(SortableRoot root in rootlist) {
                        if(!rootCheck(root)) {
                            throw new InternalErrorException(String.Format("Sanitycheck: {0:R} {1:R}",
                                root.Lower, root.Upper));
                        }
                    }
                }
            }
        }
#endif
        [CLSCompliant(true)]
        /// <summary>
        /// Expand tis node, that is make 4 children and place them at the
        /// end of the queue. Intermediate vertices' topos are computed
        /// </summary>
        /// <param name="que"></param>
        public void Expand(){
            /*
             * Expand creates new smartevertex points.
             * However, the new BC is known only after the new SmartTrixel is created...
             * */

            SmartVertex w0 = new SmartVertex(this.v[1].Vertex.GetMiddlePoint(this.v[2].Vertex, true));
            SmartVertex w1 = new SmartVertex(this.v[2].Vertex.GetMiddlePoint(this.v[0].Vertex, true));
            SmartVertex w2 = new SmartVertex(this.v[0].Vertex.GetMiddlePoint(this.v[1].Vertex, true));

            Region reg = covmachine.reg;

            w0.SetParentArcsAndTopo(this.plist, reg);
            w1.SetParentArcsAndTopo(this.plist, reg);
            w2.SetParentArcsAndTopo(this.plist, reg);

 
            //
            // Sort my fractionals if I have any
            
            for (int i = 0; i < 3; i++) {
                if (fractionals[i] != null && fractionals[i].Count > 1) {
                    fractionals[i].Sort(CompareTo);
                }
            }
            Int64 id0 = this.hid << 2;
            Queue<SmartTrixel> que = covmachine.smartQue;
            que.Enqueue(new SmartTrixel(this, id0++, this.v[0], w2, w1));
            que.Enqueue(new SmartTrixel(this, id0++, this.v[1], w0, w2));
            que.Enqueue(new SmartTrixel(this, id0++, this.v[2], w1, w0));
            que.Enqueue(new SmartTrixel(this, id0++,        w0, w1, w2));
        }
        /// <summary>
        /// Constructor
        /// </summary>
        private SmartTrixel() { }
        /// <summary>
        /// Compute bounding circle (BC) and patchlist filtered by the BC.
        /// If parent trixel exists use its patch list if possible.
        /// If parent doesn't exist, or has no patchlist, then use patches
        /// from the Region.
        /// </summary>
        private void initBCandPList() {
            this.bc = new Spherical.Halfspace(v[0].Vertex, this.v[1].Vertex, v[2].Vertex);
            /* See if I or the parent have a plist already */
            if (this.parent != null) {
                if (this.parent.plist != null) {
                    this.plist = FilterByBC(this.parent.plist);
                }
            }
            if (this.plist == null) {
                this.plist = FilterByBC(covmachine.outline.EnumIPatches());
            }
        }
        /// <summary>
        /// Virtual root of trixel tree, must have offsprings
        /// </summary>
        /// <param name="machine"></param>
        internal SmartTrixel(Cover machine) {
            this.parent = null;
            this.covmachine = machine;
            this.hid = 0L;
            this.Level = -1;
        }
        /// <summary>
        /// Create a new SmartTrixel. The parent passes on the reference
        /// to the machine, and tells this new trixel its "child number" (0-3)
        /// 
        /// </summary>
        /// <param name="parent">SmartTrixel responsible for this child</param>
        /// <param name="hid">hid of this child</param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        internal SmartTrixel(SmartTrixel parent, Int64 hid,
            SmartVertex iv0, SmartVertex iv1, SmartVertex iv2) {
            // null parent allowes
            //
            // Initialize v0 - v3; from parent
            //
            this.hid = hid;
            this.parent = parent;
            this.covmachine = parent.covmachine;
            this.Level = parent.Level + 1;
            
            this.v[0] = iv0;
            this.v[1] = iv1;
            this.v[2] = iv2;
            initBCandPList();
            localTopo[0] = this.v[0].Topo;
            localTopo[1] = this.v[1].Topo;
            localTopo[2] = this.v[2].Topo;

            // v.Topo is Same if ON an arc, and Inner if inside region
            // Here, we make v to be either Inner (in region) or Outer (outside region)
            this.edges[0] = new Arc(this.v[1].Vertex, this.v[2].Vertex);
            this.edges[1] = new Arc(this.v[2].Vertex, this.v[0].Vertex);
            this.edges[2] = new Arc(this.v[0].Vertex, this.v[1].Vertex);
            if(this.Level == 0) {
                updateFractionals(); //keeper
            }
        }
        /// <summary>
        /// Count the number of vertices inside region.
        /// </summary>
        /// <returns></returns>
        private int nrVerticesInner() {
            int n = 0;
            n += (localTopo[0] == Topo.Inner ? 1 : 0);
            n += (localTopo[1] == Topo.Inner ? 1 : 0);
            n += (localTopo[2] == Topo.Inner ? 1 : 0);
            return n;
        }
        /// <summary>
        /// make a new fractional root list for the given edge and from the parent trixel's
        /// list of fractional roots
        /// </summary>
        /// <param name="edgenr"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        private List<SortableRoot> splitFractionals(int edgenr, bool bottom/* , IList<IPatch> plist*/ ) {
            List<SortableRoot> result = new List<SortableRoot>();
            foreach (SortableRoot frac in parent.fractionals[edgenr]) {

                if(!rootCheck(frac)) {
                    throw new InternalErrorException(String.Format("splifractionals starts with bad root"));
                }
                double upper = frac.Upper;
                double lower = frac.Lower;

                if(bottom) {
                    lower *= 2.0;
                    upper *= 2.0;
                    if(lower < 1.0) {
                        upper = upper > 1.0 ? 1.0 : upper;
                        result.Add(new SortableRoot(lower, upper, frac.ParentArc, frac.topo));
                    }
                } else {
                    lower = 2.0 * (lower - 0.5);
                    upper = 2.0 * (upper - 0.5);
                    if(upper > 0.0) {
                        lower = lower < 0.0 ? 0.0 : lower;
                        result.Add(new SortableRoot(lower, upper, frac.ParentArc, frac.topo));
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Does this trixel properly intersext the region outline?
        /// </summary>
        /// If the intersection grazes the edge, then it is not a proper intersection
        /// <returns>true if yes</returns>
        private bool intersectingFractionals() {
            for (int i = 0; i < 3; i++) {
                foreach (SortableRoot sroot in this.fractionals[i]) {
                    if (sroot.topo == Topo.Intersect) {
                        // Is it between 0 and 1 NOT including 0 or 1
                        if(sroot.Upper > Trixel.Epsilon &&
                            sroot.Upper < 1.0 - Trixel.Epsilon) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// updates the three fractional lists, tries to inherit from parent whenever possible 
        /// </summary>
        private void updateFractionals() {
            //
            if(this.Level == 0) { // Top level trixel
                for(int i = 0; i < 3; i++) {
                    this.fractionals[i] = GetFractionalRoots(edges[i], plist, false);
                    // sort but not simplify
                }
            } else {
                int whoami = (int)(hid & 3);
                int mylevel = Trixel.LevelOfHid(hid);
                // if parent has no fractionals, create them
                for(int i = 0; i < 3; i++) {
                    if(parent.fractionals[i] == null) {
                        parent.fractionals[i] = GetFractionalRoots(parent.edges[i], parent.plist, true);
                    }
                }
                switch(whoami) {
                    case 0:
                        this.fractionals[0] = GetFractionalRoots(edges[0], plist, false);
                        this.fractionals[1] = splitFractionals(1, false);
                        this.fractionals[2] = splitFractionals(2, true);
                        break;
                    case 1:
                        this.fractionals[0] = GetFractionalRoots(edges[0], plist, false);
                        this.fractionals[1] = splitFractionals(2, false);
                        this.fractionals[2] = splitFractionals(0, true);
                        break;
                    case 2:
                        this.fractionals[0] = GetFractionalRoots(edges[0], plist, false);
                        this.fractionals[1] = splitFractionals(0, false);
                        this.fractionals[2] = splitFractionals(1, true);
                        break;
                    case 3:
                        this.fractionals[0] = GetFractionalRoots(edges[0], plist, false);
                        this.fractionals[1] = GetFractionalRoots(edges[1], plist, false);
                        this.fractionals[2] = GetFractionalRoots(edges[2], plist, false);
                        break;
                }
            }
            // Check sanity of fractionals
#if DEBUG
            this.sanitycheck();
#endif
            return;
        }
        /// <summary>
        /// Set the vertex topo information
        /// </summary>
        /// <param name="ix">index number of vertex</param>
        /// <returns>Either Partial or Undefined</returns>
        /// 
        private Markup setLocalTopo(int ix) {
            Markup res = Markup.Undefined;
            this.localTopo[ix] = Topo.Outer; // Unless we prove it wrong

            int prev = (ix + 1) % 3; // incoming
            int next = (ix + 2) % 3; // ix - 1, outgoing
            
            // Get the trixel's in and out arc for this vertex
            Arc ain = this.edges[prev];
            Arc aout = this.edges[next];
            Wedge tw = this.v[ix].makeOneWedge(ain, aout);

            foreach(Wedge ow in this.v[ix].wedgelist) {
                Markup comp = ow.Compare(tw);
                if(comp == Markup.Partial) {
                    return Markup.Partial;
                    // caller also knows not to worry about the rest
                }
                if(comp == Markup.Inner) {
                    this.localTopo[ix] = Topo.Inner;
                }
            }
            return res;
        }

        
        /// <summary>
        /// Assigns markup value to this Trixel
        /// </summary>
        /// <returns>Partial, Inner, Reject or Undefined</returns>
        internal Markup GetMarkup() {
            Markup mark = Markup.Undefined;
            for(int i = 0; i < 3; i++) {
                if(v[i].Topo == Topo.Same) {
                    mark = setLocalTopo(i); // either Undef or Partial expected.
                    if(mark != Markup.Undefined) {
                        return mark;
                    }
                }
            }
            int nvinr = this.nrVerticesInner(); // compute from localtopos
            if(nvinr == 1 || nvinr == 2) {
                return Markup.Partial;
            }
            /* no trixel  vertices are in region: partial or reject ?******************************/
            if(plist.Count == 0) {
                return Markup.Reject;
            }
            // If any point of any arc is in the Trixel, then PARTIAL
            // This segment is vital.
            foreach(PatchPart p in plist) {
                foreach(Arc a in p.ArcList)
                    if(IsArcInTrixel(covmachine.GetSmartArc(a), false)) {
                        return Markup.Partial;
                    }
            }
            updateFractionals(); // Sorts too
            if(nvinr == 0) {
                for(int i = 0; i < 3; i++) {
                    simplifySortableRoots(this.fractionals[i]); // essential!
                }
            }
            if(intersectingFractionals()) {
                return Markup.Partial;
            }
            if(nvinr == 0) {
                return Markup.Reject;
            }
            // method always returns something.
            return Markup.Inner;
        }
        /// <summary>
        /// Create list of patches from a region
        /// whose bounding circle (BC) intersects with the trixel's BC
        /// </summary>
        /// <param name="reg">Region</param>
        /// <param name="BC">Trixel's bounding circle</param>
        /// <param name="hid">HtmID for bookkeeping</param>
        /// <returns></returns>
        /// 
        private IList<IPatch> FilterByBC(IEnumerable<IPatch> patches) {
            List<IPatch> result = new List<IPatch>();
            foreach(IPatch p in patches) {
                try {
                    Topo t = p.Mec.GetTopo(bc);
                    if(t != Topo.Disjoint && t != Topo.Inverse) {
                        result.Add(p);
                    }
                } catch(InternalErrorException) {
                    //Used without an argument to preserve stack location where excpetion was
                    //originally raised (CA2200, Microsoft.Usage)
                    throw;  
                }
            }
            return result;
        }

        /// <summary>
        /// Eliminate redundant roots
        /// </summary>
        /// <param name="sortableRoots"></param>
        internal static void simplifySortableRoots(List<SortableRoot> sortableRoots) {
            if (sortableRoots == null)
                return;


#if DEBUG
            for(int i = 0; i < sortableRoots.Count - 1; i++) {
                if(sortableRoots[i].Lower > sortableRoots[i + 1].Lower) {
                    throw new InternalErrorException("TestNode.sortandprune(): Bad sort!");
                }
            }
#endif
            if (sortableRoots.Count > 0) {
                for (int k = 0; k < sortableRoots.Count; ) {
                    bool deleteme = false;
                    // I will delete myself if I am a point and
                    // 1a) degenerate (0,0) or (1,1)
                    // 1b) I can be merged with the successive element (pt or interval)
                    // If I am an interval, I will remove my successors 
                    // 2a) that is a point, and can be merged with me
                    // then I may remove myself too, 
                    // 2b) if I am degenerate
                    //
                    // 
                    SortableRoot me = sortableRoots[k];
                    if (me.topo == Topo.Intersect) { 
                        // ////////////////////////////////// I am a point
                        /* only if I can be merged ... NEW:12/20
                        if (me.Lower < Trixel.Epsilon || (1.0 - me.Lower < Trixel.Epsilon)) {
                            // I am an endpoint dangling
                            deleteme = true;
                        } else */ 
                        if (k < sortableRoots.Count - 1) { // Look ahead, see if I can merge
                            SortableRoot next = sortableRoots[k + 1];
                            if (next.Lower - me.Upper < Trixel.Epsilon) {
                                deleteme = true;
                            }
                        }
                    } else { 
                        // ///////////////////////////////// I am a interval
                        // I will try to delete successor points...
                        // 
                        while (k < sortableRoots.Count - 1) {
                            SortableRoot successor = sortableRoots[k + 1];
                            if(successor.topo == Topo.Intersect) {
                                if((me.Lower <= successor.Lower + Trixel.Epsilon) &&
                                    (successor.Lower <= me.Upper + Trixel.Epsilon)) {
                                    sortableRoots.RemoveAt(k + 1);
                                } else {
                                    break;
                                }
                            } else {
                                break;
                            }
                        }
                        // Am I still degenerate?
                        if(me.Upper - me.Lower < Trixel.Epsilon) {
                            if(me.Lower < Trixel.Epsilon || (1.0 - me.Lower < Trixel.Epsilon)) {
                                deleteme = true;
                            }
                        }
                    }
                    if (deleteme) {
                        sortableRoots.RemoveAt(k);
                    } else {
                        k++;
                    }
                }
            }
        }
        /// <summary>
        /// Decide whether or not any endpoint of arc is inside the trixel
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="both"></param>
        /// <returns></returns>
        internal bool IsArcInTrixel(SmartArc sa, bool both) {
            //
            // Only if not degenerate trixel
            //
            for(int i = 0; i < 3; i++) {
                if(v[i].Topo == Topo.Same)
                    return false;
            }
            if(sa.Arc.IsFull) {
                // first see if it is an infinitely small hole...
                if(sa.Arc.Circle.Cos0 < -costol) {
                    return false;
                }
                Topo t = this.bc.GetTopo(sa.Arc.Circle);
                if(t == Topo.Disjoint) {
                    return false;
                }
                // else :
                // look in this trixel's fractionals and see if there is an
                // intersecting pair from the same arc. Takes care of cases
                // where circle intersects trixel exactly so that trixel edge
                // precisely grazes the circle.

                // But first, try the obvious here too
                // DO NOT REMOVE, the following is essential
                if(Trixel.IsAncestor(this.hid, sa.Hid1)) {
                    return true;
                }

                for(int i = 0; i < 3; i++) {
                    Arc arc = null;
                    if(this.fractionals[i] != null) {
                        foreach(SortableRoot sroot in this.fractionals[i]) {
                            if(arc == null) {
                                arc = sroot.ParentArc;
                            } else if(sroot.ParentArc == arc) {
                                return true;
                            }
                        }
                    }
                }
                return false;
            } else {
                bool p1in = Trixel.IsAncestor(this.hid, sa.Hid1);
                bool p2in;
                if(sa.Hid2 == 0L) {
                    p2in = both; // if both, then p2in is forced true else 
                } else {
                    p2in = Trixel.IsAncestor(this.hid, sa.Hid2);
                }
                if(both) {
                    return p1in && p2in;
                } else {
                    return p1in || p2in;
                }
            }
        }
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="a"></param>
        /// <param name="arclo"></param>
        /// <param name="archi"></param>
        /// <returns></returns>
        private static bool haveCommonInterval(Arc edge, Arc a, out double arclo, out double archi) {
            arclo = archi = -1.0;
            //
            // Fractional angle  (fraction of edge's length)
            // of arc's two endpoint
            double lo = edge.GetAngle(a.Point1) / edge.Angle;
            double hi = edge.GetAngle(a.Point2) / edge.Angle;
            double pa, pb;

            if(lo <= hi) {
                pa = lo;
                pb = hi;
            } else {
                pb = lo;
                pa = hi;
            }
            if(pb < 0.0)
                return false;
            if(pa > 1.0)
                return false;

            arclo = pa < 0 ? 0.0 : pa;
            archi = pb > 1 ? 1.0 : pb;
            return true;        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="a"></param>
        /// <param name="root"></param>
        /// <param name="arcint"></param>
        /// <returns></returns>
        private static bool haveIntersect(Arc edge, Arc a, Cartesian root, out double arcint) {
            bool haveone = false;
            arcint = 0.0;
            double phi = edge.GetAngle(root);

            phi = (a.IsFull && (phi >= Math.PI * 2.0 - Trixel.DblTolerance)) ? 0.0 : phi;

            if (phi <= edge.Angle + Trixel.DblTolerance &&
                a.GetAngle(root) <= a.Angle + Trixel.DblTolerance) {
                haveone = true;
                arcint = phi / edge.Angle;
            }
            return haveone;
        }
        /// <summary>
        /// Get a sorted list of distances of intersections (roots) from the given edge's first point
        /// </summary>
        /// <remarks>
        /// An edge of a trixel is intersected with each arc in each patch of the patchlist.
        /// The roots that are on both the edge and the arc are put
        /// on a list in the form of angular distance from the edge's Point1.
        /// </remarks>
        /// <param name="edge">An arc representing the edge (of the trixel)</param>
        /// <param name="in_plist">Patch list</param>
        /// <param name="simplify">if true, root list simplification will occur</param>
        /// <returns>list of roots tagged with fractional angles tagged with  in radians</returns>
        public static List<SortableRoot> GetFractionalRoots(Arc edge, IList<IPatch> in_plist, bool simplify) {
            List<SortableRoot> resultListRoots = new List<SortableRoot>();

            Topo topo;
            Cartesian root1 = Cartesian.NaN;
            Cartesian root2 = Cartesian.NaN;
            int flag;

            double arclo, archi, arcint; 
            foreach(IPatch p in in_plist) {
                foreach(Arc a in p.ArcList) {
                    topo = edge.Circle.GetTopo(a.Circle, out root1, out root2, out flag);
                    if(topo == Topo.Inverse || topo == Topo.Same) {
                        // arc is collinear with edge
                        // e1 and e2 are 0 and trixelEdgeAngle, resp.
                        // a1 , a2 are edge.GetAngle(a.Point1, 2) resp.
                        if(a.IsFull) {
                            arclo = 0.0;
                            archi = 1.0; // was: edge.Angle;
                            resultListRoots.Add(new SortableRoot(0.0, 1.0, a, topo));
                        } else {
                            if(haveCommonInterval(edge, a, out arclo, out archi)) {
                                resultListRoots.Add(new SortableRoot(arclo, archi, a, topo));
                            }
                        }
                    }
                    if(topo == Topo.Intersect) {
                        if(haveIntersect(edge, a, root1, out arcint)){
                            resultListRoots.Add(new SortableRoot(arcint, a));
                        }
                        if(haveIntersect(edge, a, root2, out arcint)) {
                            resultListRoots.Add(new SortableRoot(arcint, a));
                        }
                    }
                }
            }
            if(resultListRoots.Count > 1) {
                resultListRoots.Sort(CompareTo); // new RootComparator());
            }
            if(simplify) {
                simplifySortableRoots(resultListRoots);
            }
            return resultListRoots;
        }
        /// <summary>
        /// Comparator for sorting
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int CompareTo(SortableRoot a, SortableRoot b) {
            return a.Lower.CompareTo(b.Lower);
        }
        /// <summary>
        /// Standard ToString override
        /// </summary>
        /// 
        /// <returns>Hid formatted as [NS]{[0-3]}+</returns>
        public override string ToString() {
            return Trixel.ToString(this.Hid);
        }

    }
}
