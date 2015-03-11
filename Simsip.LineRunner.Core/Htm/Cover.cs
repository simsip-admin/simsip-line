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

  File:      Cover.cs 
  Summary:   Implements a circular region of interest on the celestial sphere.
  Date:	     August 10, 2005
 
======================================================= */

namespace Spherical.Htm {
    /// <summary>
    /// Cover provides static (a) methods to generate trixel coverings of regions
    /// 
    /// A covering can be extracted as a list of trixels, or lists of HtmID 
    /// (start, end) pairs.
    /// In the latter case, the HtmIDs are always for level 20 trixels. 
    /// The trixels making up the covering are either "inner" or "partial".
    /// Inner trixels are completely contained
    /// in the region. Partial trixels intersect the boundary of the
    /// region. Partial trixels follow the outline(s) of the region.
    /// The "outer" cover is the union of
    /// "inner" and "partial" covers.
    /// Each covering has an associated "cost," which is currently the number of
    /// (start, end) HtmID range pairs in the outer cover. The cost is used to deciding 
    /// to terminate the search.
    /// 
    /// The Cover object uses an internal "machine" that 
    /// computes the covering. The machine runs in stages, each successive step generates a finer
    /// resolution covering. Each step increases the depth of the partial 
    /// trixels in the covering.
    /// </summary>
    public class Cover {
        /// <summary>
        /// stores the Region object
        /// </summary>
        internal Spherical.Region reg;
        /// <summary>
        /// The list of patches making up the outline of the region.
        /// </summary>
        internal Spherical.Outline outline;
        // <summary>
        /// Dictionary that returns a SmartArc
        /// </summary>
        private Dictionary<Arc, SmartArc> smartArcTable;
        /// <summary>
        /// Keep track of all partial trixels for each level
        /// </summary>
        internal Stack<List<SmartTrixel>> stackOfPartialLists;
        /// <summary>
        /// List of inner trixels
        /// </summary>
        internal List<SmartTrixel> listOfInners;
        /// <summary>
        /// Current list of partials. Always selected from the stack of partials
        /// </summary>
        private List<SmartTrixel> savedListOfPartials;
        /// <summary>
        /// The queue for storing node in breadth-first search
        /// </summary>
        internal Queue<SmartTrixel> smartQue = new Queue<SmartTrixel>(100);
        internal int currentLevel;
        private int previousLevel;
        private HaltCondition haltcondition;
        /// <summary>
        /// Used for evaluating halt condition. Prevents going too far
        /// </summary>
        internal int maxRanges;
        /// <summary>
        /// used for evaluating halt condition. Prevents stopping too early
        /// </summary>
        internal int minRanges;
        /// <summary>
        /// used for evaluating halt condition. Prevents stopping too early
        /// </summary>
        internal int minLevel;
        /// <summary>
        /// Used for evaluating halt condition. Prevents going too far
        /// </summary>
        internal int maxLevel;

        /// <summary>
        /// Process current smart trixel. called from machine's Step() function
        /// </summary>
        /// <param name="sq"></param>
        private void Visit(SmartTrixel sq) {
            Markup m = sq.GetMarkup();
            switch (m) {
                case Markup.Inner:

#if ROBUSTTEST
                    // Check if an ancestor is in the FULL list
                    // Should not need this!!!
                    if (!IsAncestorInList(q, _inners)) {
                        _inners.Add(q);
                    } else {
                        throw new Spherical.HTM.InternalErrorException("Visit: ancestor is in inner ");
                    }
#else
                    listOfInners.Add(sq);
#endif
                    sq.Terminal = true;
                    break;
                case Markup.Reject:
                    sq.Terminal = true;
                    break;
                case Markup.Partial:
                    stackOfPartialLists.Peek().Add(sq);
                    break;
            }
        }
        /// <summary>
        /// Evaluate current cost in terms of number of 
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        internal int Cost(Markup kind) {
            switch (kind) {
                case Markup.Outer:
                    return RowCost(stackOfPartialLists.Peek(), listOfInners);

                case Markup.Inner:
                    return RowCost(null, listOfInners);

                case Markup.Partial:
                    return RowCost(stackOfPartialLists.Peek(), null);

            }
            return -1;
        }
        /// <summary>
        /// Compute the number of rows that would be returned
        /// </summary>
        /// <param name="partials">list of partial trixels</param>
        /// <param name="inners">list of inner trixels</param>
        /// <returns></returns>
        private static int RowCost(List<SmartTrixel> partials, List<SmartTrixel> inners) {            
            Hidranges PR = new Hidranges(partials, true);
            Hidranges ran = new Hidranges(inners, true);
            ran.Merge(PR.pairList);
            ran.Sort();
            ran.Compact();
            ran.Check();
            return ran.pairList.Count;
        }
        /// <summary>
        /// Given some idea about the size of features in a region,
        /// this allows limiting the levels to which search for cover proceeds
        /// </summary>
        internal void estimateMinMaxLevels() {

            double featuresize = 1.0;
            double a;
            
            double minArea = Constant.WholeSphereInSquareDegree, maxArea = 0.0;
            foreach (Convex con in this.reg.ConvexList) {
                foreach (Patch p in con.PatchList) {
                    a = p.Area;
                    if (a < 0.0 + Trixel.DblTolerance) {
                        a += Constant.WholeSphereInSquareDegree;
                    }
                    if (minArea > a) minArea = a;
                    if (maxArea < a) maxArea = a;
                }
            }


            featuresize = Math.Sqrt(maxArea * 2.0);
            featuresize *= Constant.Degree2Radian;
            minLevel = HtmState.Instance.getLevel(featuresize);

            featuresize = Math.Sqrt(minArea / 2.0);
            featuresize *= Constant.Degree2Radian;
            maxLevel = HtmState.Instance.getLevel(featuresize);
            if (maxLevel < minLevel + HtmState.Instance.hdelta) {
                maxLevel += HtmState.Instance.hdelta / 2;
                minLevel -= HtmState.Instance.hdelta / 2;
            }
            //
            // Min must never be below 3
            // 
            int nudgeup = minLevel - 3;
            if (nudgeup < 0) {
                minLevel -= nudgeup;
                maxLevel -= nudgeup;
            }

            maxLevel += HtmState.Instance.deltalevel;
            minLevel += HtmState.Instance.deltalevel;
        }
        /// <summary>
        /// Relate an Arc to a SmartArc
        /// </summary>
        /// <param name="ol"></param>
        private void buildArcTable(Outline ol) {
            this.smartArcTable = new Dictionary<Arc, SmartArc>();
            foreach (PatchPart patch in ol.EnumIPatches()) {
                foreach (Arc a in patch.ArcList) {
                    this.smartArcTable.Add(a, new SmartArc(a));
                }
            }
            return;
        }

        // PRIVATE MEMBERS

        /// <summary>
        /// Try to return at least this many ranges, default value
        /// </summary>
        internal  const int DefaultMinRanges = 12;
        /// <summary>
        /// Try to return at most this many ranges, default value
        /// </summary>
        internal  const int DefaultMaxRanges = 20;
        private SmartVertex[] points;
        /// <summary>
        /// Initialize internals
        /// </summary>
        /// <param name="r"></param>
        public void Init(Spherical.Region r) {
            this.reg = r;
            
            this.listOfInners = new List<SmartTrixel>();
            this.stackOfPartialLists = new Stack<List<SmartTrixel>>();
            this.stackOfPartialLists.Push(new List<SmartTrixel>());

            this.savedListOfPartials = null; // When we pop, we save one popped level
            this.currentLevel = 0;

            this.previousLevel = -1;
            this.estimateMinMaxLevels(); // , out _minlevel, out _maxlevel);
            this.minRanges = Cover.DefaultMinRanges;
            this.maxRanges = Cover.DefaultMaxRanges;

            //
            // Make outline
            //
            this.outline = new Outline(this.reg.EnumPatches());
            buildArcTable(this.outline);

            List<IPatch> plist = new List<IPatch>();
            foreach(IPatch p in outline.EnumIPatches()) {
                plist.Add(p);
            }

            // /////////////// Initialize base polyhedron
             
            points = new SmartVertex[6];
            HtmState htm = HtmState.Instance;
            for (int i = 0; i < 6; i++) {
                points[i] = new SmartVertex(htm.Originalpoint(i), true);
                points[i].SetParentArcsAndTopo(plist, reg);
            }

            SmartTrixel root = new SmartTrixel(this);
            for(int i=0; i<8; i++) {
                Int64 hid;
                int ix, iy, iz;
                hid = htm.Face(i, out ix, out iy, out iz);
                this.smartQue.Enqueue(
                    new SmartTrixel(root, hid, points[ix], points[iy], points[iz]));
            }        
        }
        /// <summary>
        /// Create a Cover object with the given Region, and initialize the 
        /// internal cover machine.
        /// </summary>
        /// <param name="reg">Region object for which a trixel covering will be generated</param>
        public Cover(Spherical.Region reg) {
            this.Init(reg);
        }

        /// <summary>
        /// Get the SmartArc related to the given Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns>The SmartArc, or null</returns>
        internal SmartArc GetSmartArc(Arc arc) {
            if (smartArcTable.ContainsKey(arc)) {
                return this.smartArcTable[arc];
            } else {
                return null;
            }
        }
        /// <summary>
        /// Get the Region object associated with this Cover.
        /// </summary>
        /// <returns>the Region object</returns>
        public Spherical.Region GetRegion {
            get {
                return this.reg;
            }
        }
        /// <summary>
        /// Compute the pseudoare of selected part of the covering
        /// 
        /// </summary>
        /// <param name="kind">One of {Inner, Outer, Partial}</param>
        /// <returns></returns>
        public Int64 GetPseudoArea(Markup kind) {
            Int64 pa = 0;
            if(kind == Markup.Inner || kind == Markup.Outer) {
                foreach(SmartTrixel qt in this.listOfInners) {
                    pa += Cover.PseudoArea(qt.Hid);
                }
            }
            if(kind == Markup.Outer || kind == Markup.Partial) {
                foreach(SmartTrixel qt in this.stackOfPartialLists.Peek()) {
                    pa += Cover.PseudoArea(qt.Hid);
                }
            }
            return pa;
        }
        /// <summary>
        /// Compute the cost in resources in the current state of the Cover object.
        /// </summary>
        /// <returns>the cost</returns>
        public int Cost() {
            return this.Cost(Markup.Outer);
        }
        /// <summary>
        /// Return the current level of trixels in the current state of the covering.
        /// Levels run from 0 to 24.
        /// </summary>
        /// <returns>level number</returns>
        public int GetLevel {
            get {
                return this.currentLevel;
            }
        }
        /// <summary>
        /// Get the current maximum level in effect
        /// </summary>
        /// <returns>integer between 0 and 20, inclusive</returns>
        public int GetMaxLevel {
            get {
                return this.maxLevel;
            }
        }
        /// <summary>
        /// Change the tunable parameters for the generation of the covering
        /// </summary>
        /// <param name="minRanges"></param>
        /// <param name="maxRanges"></param>
        /// <param name="maxLevel"></param>
        public void SetTunables(int minr, int maxr, int maxl) {
            if(minr > 0) {
                this.minRanges = minr;
            }
            if(maxr > 0) {
                this.maxRanges = maxr;
            }
            this.maxLevel = maxl;
        }
        /// <summary>
        /// compute haltcondition for this cover generation
        /// </summary>
        /// <returns></returns>
        private HaltCondition evaluateCurrentLevel() {
            int computedcost = Cost(Markup.Outer);
            // ########################## STOP CRITERIA #############################
            // Stop whenever one of these is true:
            // (1) If the budget was exceeded, output stuff from previous level
            //     (backup)
            // (2) if level exceeds maxlevel, use current level (hold)
            // (3) If the budget was below limit, but above min. cost (hold)
            // 
            // Otherwise continue (go on)

            HaltCondition halt = HaltCondition.Continue;
            if (computedcost > maxRanges) { // (1)
                halt = HaltCondition.Backup;
            } else if (this.currentLevel >= maxLevel) { // (2)
                halt = HaltCondition.Hold;
            } else if (computedcost > minRanges && computedcost <= maxRanges) { // (3)
                halt = HaltCondition.Hold;
            }
            return halt;
        }
        /// <summary>
        /// Run this cover machine until completion.
        /// </summary>
        public void Run() {
            if (reg == null)
                return;
            haltcondition = HaltCondition.Continue;
            while (haltcondition == HaltCondition.Continue && smartQue.Count > 0) {
                Step();
                haltcondition = this.evaluateCurrentLevel();
            }
            if (haltcondition == HaltCondition.Backup) {
                savedListOfPartials = stackOfPartialLists.Pop();
                currentLevel--;
            } else {
                savedListOfPartials = null;
            }
        }
        /// <summary>
        /// Step this machine to the next level.
        /// </summary>
        public void Step() {
            if (reg == null)
                return;

            bool finishedLevel = false;

            if (savedListOfPartials != null) { // Maybe we popped during Run, no sense in wasting it
                stackOfPartialLists.Push(savedListOfPartials);
                savedListOfPartials = null;
                currentLevel++;
                return;
            }
            if (smartQue.Count > 0) {
                stackOfPartialLists.Push(new List<SmartTrixel>());
                currentLevel++;
            }
            while (!finishedLevel && smartQue.Count > 0) {
                SmartTrixel sq = smartQue.Peek();
                if (sq.Level != previousLevel) {
                    if (previousLevel < 0) {
                        currentLevel = 0;
                        previousLevel = sq.Level;
                    } else {
                        finishedLevel = true;
                    }
                    previousLevel = sq.Level;
                }
                if (!finishedLevel) {
                    smartQue.Dequeue();
                    Visit(sq);
                    if (!sq.Terminal) {
                        sq.Expand();
                    }
                }
            }
        }
        /// <summary>
        /// The One function
        /// </summary>
        /// <returns>1</returns>
        public int One() {
            return 1;
        }
        /// <summary>
        /// Convenience wrapper for a one step cover generation.
        /// </summary>
        /// <param name="reg"></param>
        /// <returns>list of (start, end) HtmID pairs of Outer trixels</returns>
        public static List<Int64> HidList(Region reg) {
            Cover cov = new Cover(reg);
            cov.Run();
            return cov.GetTrixels(Markup.Outer);
        }
        /// <summary>
        /// Create augmented range list (3rd column flags partial or inner)
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static List<Int64AugPair> HidAugRange(Region reg) {
            Cover cov = new Cover(reg);
            cov.Run();
            return cov.GetTriples(Markup.Outer);
        }
        /// <summary>
        /// Get range of outer HtmIDs
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static List<Int64Pair> HidRange(Region reg) {
            Cover cover = new Cover(reg);
            cover.Run();
            return cover.GetPairs(Markup.Outer);
        }
        /// <summary>
        /// Compute the PseudoArea (number of level 20 trixels) of the given trixel.
        /// </summary>
        /// <param name="hid">The trixel's HtmID</param>
        /// <returns>64-bit pseudoarea</returns>
        public static Int64 PseudoArea(Int64 hid) {
            int level = Trixel.LevelOfHid(hid);
            Int64 result;
            int iter = (20 - level);
            //
            // 4 ^ (20 - level)
            result = 1;
            for (iter = (20 - level); iter > 0; iter--) {
                result <<= 2;
            }
            return result;
        }
        /// <summary>
        /// Compute the PseudoArea (number of level 20 trixels) of the given region.
        /// </summary>
        /// <param name="it">Range containing HtmID (start,end) pairs</param>
        /// <returns>64-bit pseudoarea</returns>

        public static Int64 PseudoArea(Hidranges it) {
            Int64 result = 0L;
            foreach (Int64Pair pair in it.pairList) {
                result += (pair.hi - pair.lo + 1);
            }
            return result;
        }

        /// <summary>
        /// Get the covering as list of trixels the machine in the current state.
        /// </summary>
        /// <param name="kind">Inner, Outer or Partial</param>
        /// <returns></returns>
        public List<Int64> GetTrixels(Markup kind) {
            switch (kind) {
                case Markup.Outer:
                    return NodesToTrixels(stackOfPartialLists.Peek(), listOfInners);

                case Markup.Inner:
                    return NodesToTrixels(null, listOfInners);

                case Markup.Partial:
                    return NodesToTrixels(stackOfPartialLists.Peek(), null);

            }
            return new List<Int64>();
        }
        /// <summary>
        /// Get the covering as a list of level 20 HtmID
        /// (start, end) pairs from the machine in the current state
        /// </summary>
        /// <param name="kind">Inner, Outer or Partial</param>
        /// <returns></returns>
        public List<Int64Pair> GetPairs(Markup kind) {
            switch (kind) {
                case Markup.Outer:
                    return NodesToPairs(stackOfPartialLists.Peek(), listOfInners);

                case Markup.Inner:
                    return NodesToPairs(null, listOfInners);

                case Markup.Partial:
                    return NodesToPairs(stackOfPartialLists.Peek(), null);

            }
            return new List<Int64Pair>(); // (_savePairs);
        }
        /// <summary>
        /// Get the covering as a list of level 20 HtmID
        /// (start, end, flag) triples from the current state of the machine
        /// 
        /// The partial and inner trixels are separated. The flag indicates
        /// whether the (start, end) portion of the triple is from partial 
        /// or inner trixels.
        /// </summary>
        /// <param name="kind">Inner, Outer or Partial</param>
        /// <returns></returns>
        public List<Int64AugPair> GetTriples(Markup kind) {
            switch (kind) {
                case Markup.Outer:
                    return NodesToTriples(stackOfPartialLists.Peek(), listOfInners);

                case Markup.Inner:
                    return NodesToTriples(null, listOfInners);

                case Markup.Partial:
                    return NodesToTriples(stackOfPartialLists.Peek(), null);
            }
            return new List<Int64AugPair>();
        }
        /// <summary>
        /// Make a list of augmented pairs from the given partial and inner trixels
        /// </summary>
        /// <param name="partialNodes"></param>
        /// <param name="innerNodes"></param>
        /// <returns></returns>
        private List<Int64AugPair> NodesToTriples(List<SmartTrixel> partialNodes, List<SmartTrixel> innerNodes) {
            List<Int64AugPair> result = new List<Int64AugPair>();
            Hidranges partials = new Hidranges(partialNodes, true);
            Hidranges inners = new Hidranges(innerNodes, true, currentLevel);
            foreach (Int64Pair lohi in partials.pairList) {
                result.Add(new Int64AugPair(lohi, false));
            }
            foreach (Int64Pair lohi in inners.pairList) {
                result.Add(new Int64AugPair(lohi, true));
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partialNodes"></param>
        /// <param name="innerNodes"></param>
        /// <returns></returns>
        private List<Int64Pair> NodesToPairs(List<SmartTrixel> partialNodes, List<SmartTrixel> innerNodes) {
            //
            // Combine the two
            // for the range
            // List<Int64Pair> result = new List<Int64Pair>();
            Hidranges partials = new Hidranges(partialNodes, true);
            Hidranges fulls = new Hidranges(innerNodes, true, currentLevel);

            // ///////////////////////////////////////
            // COMBINE PARTIAL AND FULL into allist
            //
            List<Int64Pair> allList = Hidranges.Combine(partials.pairList, fulls.pairList);
            Hidranges.Compact(allList);
            Hidranges.IsWellFormed(allList); // this will throw exception if there is a problem
            return allList;
        }
        /// <summary>
        /// Make a list of HtmIDs from the given partial and inner nodes
        /// </summary>
        /// <param name="partialNodes"></param>
        /// <param name="innerNodes"></param>
        /// <returns></returns>
        private List<Int64> NodesToTrixels(List<SmartTrixel> partialNodes, List<SmartTrixel> innerNodes) {
            List<Int64> result;
            result = new List<long>();
            if (partialNodes != null) {
                foreach (SmartTrixel q in partialNodes) {
                    result.Add(q.Hid);
                }
            }
            if (innerNodes != null) {
                foreach (SmartTrixel q in innerNodes) {
                    if (q.Level <= currentLevel) {
                        result.Add(q.Hid);
                    }
                }
            }
            return result;
        }
    }
}

