using System;
using System.Collections;
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

  File:      HidRanges.cs 
  Summary:   Implements a sorted list of HtmID intervals.
  Date:	     December, 2006
 
======================================================= */

namespace Spherical.Htm
{
    public struct Int64AugPair {
        public bool full;
		public Int64 lo;
		public Int64 hi;
        public Int64AugPair(Int64Pair lohi, bool full) {
            this.lo = lohi.lo;
            this.hi = lohi.hi;
            this.full = full;
        }
		public Int64AugPair(Int64 lo, Int64 hi, bool full) {
			this.lo = lo;
			this.hi = hi;
            this.full = full;
		}
    }
	public struct Int64Pair
	{
		public Int64 lo;
		public Int64 hi;
		public Int64Pair(Int64 lo, Int64 hi) {
			this.lo = lo;
			this.hi = hi;
		}
	};

	public class Hidranges
	{
		public List<Int64Pair> pairList = new List<Int64Pair>();
		public Hidranges() {
		}
        internal Hidranges(List<SmartTrixel> nodeList, bool sortandcheck, int deepest) {
            if(nodeList != null) {
                foreach(SmartTrixel qt in nodeList) {
                    if(qt.Level <= deepest) {
                        Int64Pair range = Trixel.Extend(qt.Hid, 20);
                        this.AddRange(range);
                    } else {
                        break;
                    }
                }
                if(sortandcheck) {
                    this.Sort();
                    this.Compact();
                    this.Check();
                }
            }
        }
            
        internal Hidranges(List<SmartTrixel> nodeList, bool sortandcheck){
            if (nodeList != null) {
                foreach (SmartTrixel qt in nodeList) {
                    Int64Pair range = Trixel.Extend(qt.Hid, 20);
                    this.AddRange(range);
                }
                if (sortandcheck) {
                    this.Sort();
                    this.Compact();
                    this.Check();
                }
            }
        }
		static private bool isDisjoint(Int64Pair a, Int64Pair b) {
			return (a.hi < b.lo || b.hi < a.lo);
		}
        public static List<Int64Pair> Combine(List<Int64Pair> ranges, List<Int64Pair> newranges) {
            List<Int64Pair> result = new List<Int64Pair>(ranges.Count);
            Int64Pair proto;
            // Pop one list into proto
            bool protomod = false;
            bool protovalid = false;
            if(newranges.Count < 1) {
                foreach(Int64Pair p in ranges) {
                    result.Add(p);
                }
                return result;
            }

            // Build a new range
            int left = newranges.Count + ranges.Count;
            int i = 0, j = 0;
            if(ranges.Count > 0 && ranges[0].lo <= newranges[0].lo) {
                proto.lo = ranges[0].lo;
                proto.hi = ranges[0].hi;
                protomod = true;
                protovalid = true;
                i++;
            } else {
                proto.lo = newranges[0].lo;
                proto.hi = newranges[0].hi;
                protomod = true;
                protovalid = true;
                j++;
            }
            left--;
            while(left > 0) {
                protomod = false; // See if this iteration modifies proto..
                if(!protovalid) {
                    if(i >= ranges.Count) { // main list is exhausted
                        proto.lo = newranges[j].lo;
                        proto.hi = newranges[j].hi;
                        protomod = true;
                        protovalid = true;
                        j++;
                    } else if(j >= newranges.Count) {   // newrange exhausted
                        proto.lo = ranges[i].lo;
                        proto.hi = ranges[i].hi;
                        protomod = true;
                        protovalid = true;
                        i++;
                    } else if(ranges.Count > 0 && ranges[i].lo <= newranges[j].lo) {
                        proto.lo = ranges[i].lo;
                        proto.hi = ranges[i].hi;
                        protomod = true;
                        protovalid = true;
                        i++;
                    } else {
                        proto.lo = newranges[j].lo;
                        proto.hi = newranges[j].hi;
                        protomod = true;
                        protovalid = true;
                        j++;
                    }
                    left--;
                }
                if(i < ranges.Count) {
                    if(protovalid && !isDisjoint(proto, ranges[i])) {
                        proto.lo = Math.Min(ranges[i].lo, proto.lo);
                        proto.hi = Math.Max(ranges[i].hi, proto.hi);
                        i++;
                        left--;
                        protomod = true;
                    }
                }
                // proto may have been modified above
                //
                if(j < newranges.Count) {
                    if(protovalid && !isDisjoint(proto, newranges[j])) {
                        proto.lo = Math.Min(newranges[j].lo, proto.lo);
                        proto.hi = Math.Max(newranges[j].hi, proto.hi);
                        j++;
                        left--;
                        protomod = true;
                    }
                }
                if(!protomod) { // proto was disjoint from both rangetops
                    result.Add(proto);
                    protomod = false;
                    protovalid = false;
                }
                // only continue, if both lists are nonempty
                //if (j >= newranges.Count || i >= pairList.Count) {
                //    break;
                //}
            }
            if(protovalid) {
                result.Add(proto);
                protovalid = false;
            }
            return result;
        }
        /// <summary>
        /// merge a given list into the current list of pairs
        /// </summary>
        /// <param name="newranges"></param>
        internal void Merge(List<Int64Pair> newranges) {
            pairList = Combine(this.pairList, newranges);
        }
        /// <summary>
        /// Add pair of hids to internal list of pairs. Does not do checking
        /// </summary>
        /// <param name="pair"></param>
        //TODO: make internal
        public void AddRange(Int64Pair pair) {
            pairList.Add(pair);
        }
        /// <summary>
        /// add pair of hids to internal list of pairs. Does not do checking
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="hi"></param>
        //TODO: deprecate:
		public void AddRange(Int64 lo, Int64 hi) {
			Int64Pair tp;
			tp.lo = lo;
			tp.hi = hi;
			pairList.Add(tp);
			return;
		}
		public void Clear() {
			pairList.Clear();
		}
        /// <summary>
        /// Comparator for sorting ranges
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
		public static int CompareTo(Int64Pair a, Int64Pair b) {
			return a.lo.CompareTo(b.lo);
		}
        /// <summary>
        /// Ensure that ranges are disjoint
        /// </summary>
		public void Check() {
			for (int i = 1; i < pairList.Count; i++) {
				if (pairList[i - 1].hi > pairList[i].lo) {
					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("Range.check i={0}: ({1}, {2}), ({3}, {4})",
						i, pairList[i-1].lo, pairList[i-1].hi,
						pairList[i].lo, pairList[i].hi);
					
					throw new InternalErrorException(sb.ToString());
				}
			}
		}
        /// <summary>
        /// Sort Interval by lows
        /// </summary>
		public void Sort() {
			pairList.Sort(CompareTo);
		}
        /// <summary>
        /// perform sanity check for the health of sorted pairs
        /// throws an exception if check fails, else returns quietly
        /// </summary>
        /// <param name="sortedpairs"></param>
        /// <returns></returns>
		public static bool IsWellFormed(List<Int64Pair> sortedpairs){
			for (int i = 1; i < sortedpairs.Count; i++) {
				if (sortedpairs[i - 1].lo > sortedpairs[i].lo) {
					throw new InternalErrorException("sorted pair is not sorted");
				}
				if (sortedpairs[i - 1].hi + 1 >= sortedpairs[i].lo) {
					throw new InternalErrorException("sorted pair is not disjoint from neighbor");
				}
            }
			return true;
		}
        /// <summary>
        /// Merge adjacent intervals, i.e., eliminate zero gaps
        /// </summary>
        //TODO: eliminate one of these (body of code)
        //TODO: work from back to front
		public void Compact() {
			Int64Pair tp;
			int count;
			count = pairList.Count;
			count--; // because we compare i to t+1
			for (int i = 0; i < count; ) { // third part purposely empty
				if (pairList[i].hi + 1 == pairList[i + 1].lo) {
					tp.lo = pairList[i].lo;
					tp.hi = pairList[i + 1].hi;
					pairList.RemoveAt(i + 1);
					pairList.RemoveAt(i);
					pairList.Insert(i, tp);
					count--;
				} else {
					i++;
				}
			}
			return;
		}
        /// <summary>
        /// Merge adjacent intervals
        /// </summary>
        /// <param name="sortedPairs"></param>
		public static void Compact(List<Int64Pair> sortedPairs) {
			Int64Pair tp;
			int count;
			count = sortedPairs.Count;
			count--; // because we compare i to t+1
			for (int i = 0; i < count; ) { // third part purposely empty
				if (sortedPairs[i].hi + 1 == sortedPairs[i + 1].lo) {
					tp.lo = sortedPairs[i].lo;
					tp.hi = sortedPairs[i + 1].hi;
					sortedPairs.RemoveAt(i + 1);
					sortedPairs.RemoveAt(i);
					sortedPairs.Insert(i, tp);
					count--;
				} else {
					i++;
				}
			}
			return;
		}
	}
}
