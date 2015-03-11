//
// Copyright (c) 2009 Mikko Mononen memon@inside.org
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//

// port to c# kevin kelley 03/2012

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace EGEngine
{
    //#ifndef DETOURSTATNAVMESH_H
    //#define DETOURSTATNAVMESH_H

    // Reference to navigation polygon.
    //typedef ushort dtStatPolyRef;

    // Maximum number of vertices per navigation polygon.

    public class dtStatNavMesh
    {
        static int DT_STAT_VERTS_PER_POLYGON = 6;
        static int DT_STAT_NAVMESH_MAGIC = 0x4E41564D;//"NAVM";
        static int DT_STAT_NAVMESH_VERSION = 3;

        static public int MAX_POLYS = 256;

        static public int MaxRoutesThisUpdate = 1;

        static Random RandGen = new Random();

        bool Initialized = false;

        // Offsets into data buffer from header
        int headerSize;
        int vertsSize;
        int polysSize;
        int nodesSize;
        int detailMeshesSize;
        int detailVertsSize;
        int detailTrisSize;

        // Find path data
        //float[] StartPos = new float[3] { 0, 0, 0 };
        //float[] EndPos = new float[3] { 0, 0, 0 };
        //float[] PickExtents = new float[3] { 20, 80, 20 };
        Vector3 PickExtents = new Vector3( 2, 4, 2 );

        public const int Size_dtStatPoly = 26;
        public const int Size_dtStatPolyDetail = 8;
        public const int Size_dtStatBVNode = 16;


        #region NavMeshStructures
        /*
         * 
         * Navigation mesh data structures
         * 
         */
        public struct dtStatPoly
        {
            public ushort[] v;                  // Indices to vertices of the poly.
            public ushort[] n;                  // Refs to neighbours of the poly.
            public byte nv;				        // Number of vertices.
            public byte flags;		            // Flags (not used).

            public dtStatPoly(int size)
            {
                nv = 0;
                flags = 0;
                v = new ushort[size];
                n = new ushort[size];
            }
        };

        public struct dtStatPolyDetail
        {
            public ushort vbase;	            // Offset to detail vertex array.
            public ushort nverts;	            // Number of vertices in the detail mesh.
            public ushort tbase;	            // Offset to detail triangle array.
            public ushort ntris;	            // Number of triangles.
        };

        public struct dtStatBVNode
        {
            public ushort[] bmin;
            public ushort[] bmax;
            public int i;

            public dtStatBVNode(int size)
            {
                i = 0;
                bmin = new ushort[size];
                bmax = new ushort[size];
            }
        };

        public struct dtStatNavMeshHeader
        {
            public int magic;
            public int version;
            public int npolys;
            public int nverts;
            public int nnodes;
            public int ndmeshes;
            public int ndverts;
            public int ndtris;
            public float cs;

            public Vector3 bmin;
            public Vector3 bmax;

            public dtStatPoly[] polys;
            public Vector3[] verts;
            public dtStatBVNode[] bvtree;
            public dtStatPolyDetail[] dmeshes;
            public Vector3[] dverts;
            public byte[][] dtris;
        };


        /*
         * 
         * Temparary structures used to load data == BIG_HACK
         * 
         */
        public struct ReadStatPolyDetail
        {
            public ushort vbase;
            public ushort nverts;
            public ushort tbase;
            public ushort ntris;
        };

        public struct ReadStatPoly
        {
            public ushort v;
            public ushort v1;
            public ushort v2;
            public ushort v3;
            public ushort v4;
            public ushort v5;

            public ushort n;
            public ushort n1;
            public ushort n2;
            public ushort n3;
            public ushort n4;
            public ushort n5;

            public byte nv;
            public byte flags;
        };

        public struct ReadStatBVNode
        {
            public ushort bmin;
            public ushort bmin1;
            public ushort bmin2;

            public ushort bmax;
            public ushort bmax1;
            public ushort bmax2;

            public int i;
        };

        unsafe struct ReadNavMeshHeader
        {
            public int magic;
            public int version;
            public int npolys;
            public int nverts;
            public int nnodes;
            public int ndmeshes;
            public int ndverts;
            public int ndtris;
            public float cs;

            public float bminX;
            public float bminY;
            public float bminZ;
            public float bmaxX;
            public float bmaxY;
            public float bmaxZ;

            public ReadStatPoly* polys;
            public float* verts;
            public ReadStatBVNode* bvtree;
            public ReadStatPolyDetail* dmeshes;
            public float* dverts;
            public byte* dtris;
        };
        #endregion NavMeshStructures

        //unsafe public class dtStatNavMesh
        //{
        //public:

        //dtStatNavMesh();
        //~dtStatNavMesh();

        //static byte[] NavMeshBuffer = null;
        //dtStatNavMeshHeader* mHeader;

        dtStatNavMeshHeader mHeader;

        //byte* m_data;
        //int m_dataSize;

        //dtStatNavMeshHeader* mHeader;

        //class dtNodePool* m_nodePool;
        //class dtNodeQueue* m_openList;
        dtNodePool m_nodePool;
        dtNodeQueue m_openList;

        // Load navigation mesh from file and initialize
        // Params:
        // Returns:        
        unsafe public void LoadNavigationMesh(string filename)
        {
            try
            {
                FileStream fs = File.OpenRead(filename);
                BinaryReader br = new BinaryReader(fs);

                int nData = br.ReadInt32();

                byte[] tmpData = new byte[nData];
                br.Read(tmpData, 0, nData);
                GCHandle pinnedArray = GCHandle.Alloc(tmpData, GCHandleType.Pinned);
                Initialize((byte*)pinnedArray.AddrOfPinnedObject(), nData, true);

                br.Close();
                br.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        // Initializes the navmesh with data.
        // Params:
        //	data - (in) Pointer to navmesh data.
        //	dataSize - (in) size of the navmesh data.
        //	ownsData - (in) Flag indicating if the navmesh should own and delete the data.

        //static float* p0;// = new float[3];
        //static float* p1;// = new float[3];

        static float[][] straightPolys = new float[MAX_POLYS][];
        unsafe public bool Initialize(byte* data, int dataSize, bool ownsData)
        {
            ReadNavMeshHeader* header = (ReadNavMeshHeader*)data;

            if (header->magic != DT_STAT_NAVMESH_MAGIC)
                return false;

            if (header->version != DT_STAT_NAVMESH_VERSION)
                return false;

            headerSize = sizeof(ReadNavMeshHeader);

            vertsSize = sizeof(float) * 3 * header->nverts;
            //polysSize = sizeof(dtStatPoly) * header->npolys;
            polysSize = sizeof(ReadStatPoly) * header->npolys;
            nodesSize = sizeof(ReadStatBVNode) * header->npolys * 2;
            detailMeshesSize = sizeof(ReadStatPolyDetail) * header->ndmeshes;
            detailVertsSize = sizeof(float) * 3 * header->ndverts;
            detailTrisSize = sizeof(byte) * 4 * header->ndtris;

            byte* d = data + headerSize;
            header->verts = (float*)d; d += vertsSize;
            //header->polys = (dtStatPoly*)d; d += polysSize;
            header->polys = (ReadStatPoly*)d; d += polysSize;
            header->bvtree = (ReadStatBVNode*)d; d += nodesSize;
            header->dmeshes = (ReadStatPolyDetail*)d; d += detailMeshesSize;
            header->dverts = (float*)d; d += detailVertsSize;
            header->dtris = (byte*)d; d += detailTrisSize;


            /*
             * 
             * Set header data
             * 
             */
            mHeader.magic = header->magic;
            mHeader.version = header->version;
            mHeader.cs = header->cs;
            mHeader.bmin.X = header->bminX;
            mHeader.bmin.Y = header->bminY;
            mHeader.bmin.Z = header->bminZ;
            mHeader.bmax.X = header->bmaxX;
            mHeader.bmax.Y = header->bmaxY;
            mHeader.bmax.Z = header->bmaxZ;
            mHeader.nverts = header->nverts;
            mHeader.nnodes = header->nnodes;
            mHeader.npolys = header->npolys;
            mHeader.ndmeshes = header->ndmeshes;
            mHeader.ndverts = header->ndverts;
            mHeader.ndtris = header->ndtris;



            /*
             * 
             * Allocate data buffers
             * 
             */
            mHeader.verts = new Vector3[mHeader.nverts];
            mHeader.polys = new dtStatPoly[mHeader.npolys];
            mHeader.bvtree = new dtStatBVNode[mHeader.npolys * 2];
            mHeader.dmeshes = new dtStatPolyDetail[mHeader.ndmeshes];
            mHeader.dverts = new Vector3[mHeader.ndverts];
            mHeader.dtris = new byte[mHeader.ndtris][];
            for (int i = 0; i < mHeader.ndtris; i++)
                mHeader.dtris[i] = new byte[4];


            /*
             * 
             * navigation mesh vertices
             * 
             */
            for (int i = 0; i < mHeader.nverts; i++)
            {
                mHeader.verts[i] = Vector3.Zero;
                mHeader.verts[i].X = header->verts[i * 3 + 0];
                mHeader.verts[i].Y = header->verts[i * 3 + 1];
                mHeader.verts[i].Z = header->verts[i * 3 + 2];
            }

            /*
             * 
             * navigation mesh polygons
             * 
             */
            for (int i = 0; i < mHeader.npolys; i++)
            {
                mHeader.polys[i] = new dtStatPoly(DT_STAT_VERTS_PER_POLYGON);

                mHeader.polys[i].nv = header->polys[i].nv;
                mHeader.polys[i].flags = header->polys[i].flags;

                for (int j = 0; j < DT_STAT_VERTS_PER_POLYGON; j++)
                {
                    mHeader.polys[i].v[j] = ((ushort*)&header->polys[i].v)[j];
                    mHeader.polys[i].n[j] = ((ushort*)&header->polys[i].n)[j];
                }
            }


            /*
             * 
             * navigation mesh BV tree
             * 
             */
            for (int i = 0; i < (mHeader.npolys * 2); i++)
            {
                mHeader.bvtree[i] = new dtStatBVNode(3);

                mHeader.bvtree[i].i = header->bvtree[i].i;

                for (int j = 0; j < 3; j++)
                {
                    mHeader.bvtree[i].bmin[j] = ((ushort*)&header->bvtree[i].bmin)[j];
                    mHeader.bvtree[i].bmax[j] = ((ushort*)&header->bvtree[i].bmax)[j];
                }
            }


            /*
             * 
             * navigation mesh detail mesh
             * 
             */
            for (int i = 0; i < mHeader.ndmeshes; i++)
            {
                mHeader.dmeshes[i] = new dtStatPolyDetail();

                mHeader.dmeshes[i].vbase = header->dmeshes[i].vbase;
                mHeader.dmeshes[i].nverts = header->dmeshes[i].nverts;
                mHeader.dmeshes[i].tbase = header->dmeshes[i].tbase;
                mHeader.dmeshes[i].ntris = header->dmeshes[i].ntris;
            }


            /*
             * 
             * navigation mesh detail vertices
             * 
             */
            for (int i = 0; i < mHeader.ndverts; i++)
            {
                mHeader.dverts[i] = Vector3.Zero;
                mHeader.dverts[i].X = header->dverts[i * 3 + 0];
                mHeader.dverts[i].Y = header->dverts[i * 3 + 1];
                mHeader.dverts[i].Z = header->dverts[i * 3 + 2];
            }


            /*
             * 
             * navigation mesh detail triangles
             * 
             */
            for (int i = 0; i < mHeader.ndtris; i++)
            {
                mHeader.dtris[i][0] = header->dtris[i * 4 + 0];
                mHeader.dtris[i][1] = header->dtris[i * 4 + 1];
                mHeader.dtris[i][2] = header->dtris[i * 4 + 2];
                mHeader.dtris[i][3] = header->dtris[i * 4 + 3];
            }


            /*
             * 
             * Allocate poll/list
             * 
             */
            m_nodePool = new dtNodePool(2048);
            //if (m_nodePool == null)
            //    return false;

            m_openList = new dtNodeQueue(2048);
            //if (m_openList == null)
            //    return false;

            /*
             * 
             * Initialize pathing buffers
             * 
             */
            for (int i = 0; i < MAX_POLYS; i++)
                straightPolys[i] = new float[3];

            Initialized = true;

            return true;
        }


        /*
         * 
         * Get path
         * 
         */
        public int GetPath(ref Vector3 startpos, ref Vector3 endpos, ushort[] polys, Vector3[] spolys, bool randomDestination)
        {
            int nStraightPath = 0;

            if (MaxRoutesThisUpdate > 0)
            {
                MaxRoutesThisUpdate--;

                unsafe
                {
                    int nPath = 0;
                    ushort startRef = 0;
                    ushort endRef = 0;

                    //StartPos[0] = startpos.X;
                    //StartPos[1] = startpos.Y;
                    //StartPos[2] = startpos.Z;
                    startRef = findNearestPoly(ref startpos, ref PickExtents);

                    //EndPos[0] = endpos.X;
                    //EndPos[1] = endpos.Y;
                    //EndPos[2] = endpos.Z;
                    endRef = findNearestPoly(ref endpos, ref PickExtents);
                    //int maxTry = 5;
                    //Vector3 tmpEnd = endpos;
                    //while (endRef == 0 && maxTry > 0)
                    //{
                    //    maxTry--;
                    //    tmpEnd.X = endpos.X + ((float)RandGen.NextDouble() - 0.5f) * 400.0f;
                    //    tmpEnd.Z = endpos.Z + ((float)RandGen.NextDouble() - 0.5f) * 400.0f;
                    //    endRef = findNearestPoly(ref tmpEnd, ref PickExtents);
                    //}

                    //return 0;

                    if (startRef != 0 && endRef != 0)
                    {
                        nPath = findPath(startRef, endRef, ref startpos, ref endpos, polys, MAX_POLYS);
                    }

                    if (nPath > 0)
                    {
                        //return 0;

                        nStraightPath = findStraightPath(ref startpos, ref endpos, polys, nPath, straightPolys, MAX_POLYS);

                        for (int i = 0; i < nStraightPath; i++)
                        {
                            spolys[i].X = straightPolys[i][0];
                            spolys[i].Y = straightPolys[i][1];
                            spolys[i].Z = straightPolys[i][2];
                        }
                    }
                }
            }

            return nStraightPath;
        }



        // Finds the nearest navigation polygon around the center location.
        // Params:
        //	center - (in) The center of the search box.
        //	extents - (in) The extents of the search box.
        // Returns: Reference identifier for the polygon, or 0 if no polygons found.
        public ushort findNearestPoly(ref Vector3 center, ref Vector3 extents)
        {
	        if (!Initialized) return 0;

            ushort nearest = 0;

	        // Get nearby polygons from proximity grid.
	        //dtStatPolyRef polys[128];
            //fixed (ushort* polys = new ushort[128])
            ushort[] polys;// = new ushort[128];
            int npolys = queryPolygons(ref center, ref extents, out polys, 128);

            // Find nearest polygon amongst the nearby polygons.                
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < npolys; ++i)
            {
                ushort Ref = polys[i];

                //float[] closest = new float[3];
                Vector3 closest = Vector3.Zero;
                if (!closestPointToPoly(Ref, ref center, ref closest))
                    continue;

                //float d = vdistSqr(center, closest);
                float d = (center - closest).LengthSquared();
                if (d < nearestDistanceSqr)
                {
                    nearestDistanceSqr = d;
                    nearest = Ref;
                }
            }

	        return nearest;
        }

        // Returns polygons which touch the query box.
        // Params:
        //	center - (in) the center of the search box.
        //	extents - (in) the extents of the search box.
        //	polys - (out) array holding the search result.
        //	maxPolys - (in) The max number of polygons the polys array can hold.
        // Returns: Number of polygons in search result array.
        //public int queryPolygons(float* center, float* extents, ushort* polys, int maxPolys)
        public int queryPolygons(ref Vector3 center, ref Vector3 extents, out ushort[] polys, int maxPolys)
        {
            polys = new ushort[128];

            if (!Initialized) return 0;

            int n = 0;
            int curNodeIndex = 0;
            dtStatBVNode node = mHeader.bvtree[curNodeIndex];
	        dtStatBVNode end = mHeader.bvtree[mHeader.nnodes];

	        // Calculate quantized box
	        float ics = 1.0f / mHeader.cs;
            ushort[] bmin = new ushort[3];
            ushort[] bmax = new ushort[3];

            // Clamp query box to world box.
            float minx = MathHelper.Clamp(center.X - extents.X, mHeader.bmin.X, mHeader.bmax.X) - mHeader.bmin.X;
            float miny = MathHelper.Clamp(center.Y - extents.Y, mHeader.bmin.Y, mHeader.bmax.Y) - mHeader.bmin.Y;
            float minz = MathHelper.Clamp(center.Z - extents.Z, mHeader.bmin.Z, mHeader.bmax.Z) - mHeader.bmin.Z;
            float maxx = MathHelper.Clamp(center.X + extents.X, mHeader.bmin.X, mHeader.bmax.X) - mHeader.bmin.X;
            float maxy = MathHelper.Clamp(center.Y + extents.Y, mHeader.bmin.Y, mHeader.bmax.Y) - mHeader.bmin.Y;
            float maxz = MathHelper.Clamp(center.Z + extents.Z, mHeader.bmin.Z, mHeader.bmax.Z) - mHeader.bmin.Z;

            // Quantize
            bmin[0] = (ushort)((int)(ics * minx) & 0xfffe);
            bmin[1] = (ushort)((int)(ics * miny) & 0xfffe);
            bmin[2] = (ushort)((int)(ics * minz) & 0xfffe);
            bmax[0] = (ushort)((int)(ics * maxx + 1) | 1);
            bmax[1] = (ushort)((int)(ics * maxy + 1) | 1);
            bmax[2] = (ushort)((int)(ics * maxz + 1) | 1);

            // Traverse tree                    
            while (curNodeIndex < mHeader.nnodes)//node < end)
            {
                bool overlap = checkOverlapBox(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    if (n < maxPolys)
                    {
                        polys[n] = (ushort)node.i;
                        n++;
                    }
                }

                if (overlap || isLeafNode)
                {
                    //node++;
                    curNodeIndex++;
                    node = mHeader.bvtree[curNodeIndex];
                }
                else
                {
                    int escapeIndex = -node.i;
                    curNodeIndex += escapeIndex;
                    node = mHeader.bvtree[curNodeIndex];
                    //node += escapeIndex;
                }
            }
	
	        return n;
        }

        // Finds path from start polygon to end polygon.
        // If target polygon canno be reached through the navigation graph,
        // the last node on the array is nearest node to the end polygon.
        // Params:
        //	startRef - (in) ref to path start polygon.
        //	endRef - (in) ref to path end polygon.
        //	path - (out) array holding the search result.
        //	maxPathSize - (in) The max number of polygons the path array can hold.
        // Returns: Number of polygons in search result array.
        static float H_SCALE = 1.1f;	// Heuristic scale.
        public int findPath(ushort startRef, ushort endRef, ref Vector3 startPos, ref Vector3 endPos, ushort[] path, int maxPathSize)
        {
            if (!Initialized) return 0;
	
	        if (startRef == 0 || endRef == 0)
		        return 0;

	        if (maxPathSize == 0)
		        return 0;

	        if (startRef == endRef)
	        {
		        path[0] = startRef;
		        return 1;
	        }

            m_nodePool.clear();
	        m_openList.clear();	    
	
	        dtNode startNode = m_nodePool.getNode(startRef);
	        startNode.pidx = 0;
	        startNode.cost = 0;
	        //startNode.total = vdist(startPos, endPos) * H_SCALE;
            startNode.total = (startPos - endPos).Length() * H_SCALE;
	        startNode.id = startRef;
	        startNode.flags = (uint)dtNodeFlags.DT_NODE_OPEN;
	        m_openList.push(startNode);

	        dtNode lastBestNode = startNode;
	        float lastBestNodeCost = startNode.total;
	        while (!m_openList.empty())
	        {
		        dtNode bestNode = m_openList.pop();
	
		        if (bestNode.id == endRef)
		        {
			        lastBestNode = bestNode;
			        break;
		        }

		        dtStatPoly poly = getPoly((int)bestNode.id-1);
		        for (int i = 0; i < (int)poly.nv; ++i)
		        {
			        ushort neighbour = poly.n[i];
			        if (neighbour != 0)
			        {
				        // Skip parent node.
				        if ((bestNode.pidx != 0) && m_nodePool.getNodeAtIdx(bestNode.pidx).id == neighbour)
					        continue;

				        dtNode parent = bestNode;
				        dtNode newNode = new dtNode();
				        newNode.pidx = m_nodePool.getNodeIdx(parent);
				        newNode.id = neighbour;

				        // Calculate cost.
                        float h = 0;
                        Vector3 p0 = Vector3.Zero;
                        Vector3 p1 = Vector3.Zero;

                        if (parent.pidx == 0)
                            p0 = startPos;//vcopy(ref p0, ref startPos);
                        else
                            getEdgeMidPoint((ushort)m_nodePool.getNodeAtIdx(parent.pidx).id, (ushort)parent.id, ref p0);

                        getEdgeMidPoint((ushort)parent.id, (ushort)newNode.id, ref p1);
                        newNode.cost = parent.cost + (p0 - p1).Length();//vdist(p0, p1);

                        // Special case for last node.
                        if (newNode.id == endRef)
                            newNode.cost += (p1 - endPos).Length();//vdist(p1, endPos);

                        // Heuristic
                        //h = vdist(p1, endPos) * H_SCALE;
                        h = (p1 - endPos).Length() * H_SCALE;
                        newNode.total = newNode.cost + h;
				
				        dtNode actualNode = m_nodePool.getNode(newNode.id);
				        if (actualNode == null)
					        continue;

                        if (!((actualNode.flags & (uint)dtNodeFlags.DT_NODE_OPEN) > 0 && newNode.total > actualNode.total) &&
                            !((actualNode.flags & (uint)dtNodeFlags.DT_NODE_CLOSED) > 0 && newNode.total > actualNode.total))
				        {
                            actualNode.flags &= ~(uint)dtNodeFlags.DT_NODE_CLOSED;
					        actualNode.pidx = newNode.pidx;
					        actualNode.cost = newNode.cost;
					        actualNode.total = newNode.total;

					        if (h < lastBestNodeCost)
					        {
						        lastBestNodeCost = h;
						        lastBestNode = actualNode;
					        }

					        if ((actualNode.flags & (uint)dtNodeFlags.DT_NODE_OPEN) > 0)
					        {
						        m_openList.modify(actualNode);
					        }
					        else
					        {
                                actualNode.flags |= (uint)dtNodeFlags.DT_NODE_OPEN;
						        m_openList.push(actualNode);
					        }
				        }
			        }
		        }

		        bestNode.flags |= (uint)dtNodeFlags.DT_NODE_CLOSED;
	        }
            //return 0;

	        // Reverse the path.
            //int test = 0;
	        dtNode prev = null;
	        dtNode node = lastBestNode;
	        do
	        {
		        dtNode next = m_nodePool.getNodeAtIdx(node.pidx);
		        node.pidx = m_nodePool.getNodeIdx(prev);
		        prev = node;
		        node = next;
	        }
	        while (node != null);
	
	        // Store path
	        node = prev;
	        int n = 0;
	        do
	        {
                path[n++] = (ushort)node.id;
		        node = m_nodePool.getNodeAtIdx(node.pidx);
	        }
	        while (node != null && n < maxPathSize);


	        return n;
        }

        // Finds a straight path from start to end locations within the corridor
        // described by the path polygons.
        // Start and end locations will be clamped on the corridor.
        // Params:
        //	startPos - (in) Path start location.
        //	endPos - (in) Path end location.
        //	path - (in) Array of connected polygons describing the corridor.
        //	pathSize - (in) Number of polygons in path array.
        //	straightPath - (out) Points describing the straight path.
        //	maxStraightPathSize - (in) The max number of points the straight path array can hold.
        // Returns: Number of points in the path.
        public int findStraightPath(ref Vector3 startPos, ref Vector3 endPos, ushort[] path, int pathSize, float[][] straightPath, int maxStraightPathSize)
        {
	        if (!Initialized) return 0;
	
	        if (maxStraightPathSize == 0)
		        return 0;

	        if (path[0] == 0)
		        return 0;

	        int straightPathSize = 0;

            //float[] closestStartPos = new float[3];
            Vector3 closestStartPos = Vector3.Zero;
	        if (!closestPointToPoly(path[0], ref startPos, ref closestStartPos))
		        return 0;

	        // Add start point.
	        //vcopy(&straightPath[straightPathSize*3], closestStartPos);
            straightPath[straightPathSize][0] = closestStartPos.X;
            straightPath[straightPathSize][1] = closestStartPos.Y;
            straightPath[straightPathSize][2] = closestStartPos.Z;
	        straightPathSize++;
	        if (straightPathSize >= maxStraightPathSize)
		        return straightPathSize;

            //float[] closestEndPos = new float[3];
            Vector3 closestEndPos = Vector3.Zero;
	        if (!closestPointToPoly(path[pathSize-1], ref endPos, ref closestEndPos))
		        return 0;

	        //float portalApex[3], portalLeft[3], portalRight[3];
            float[] portalApex = new float[3];
            float[] portalLeft = new float[3];
            float[] portalRight = new float[3];

	        if (pathSize > 1)
	        {
		        portalApex[0] = closestStartPos.X;
                portalApex[1] = closestStartPos.Y;
                portalApex[2] = closestStartPos.Z;
		        vcopy(portalLeft, portalApex);
		        vcopy(portalRight, portalApex);
		        int apexIndex = 0;
		        int leftIndex = 0;
		        int rightIndex = 0;

		        for (int i = 0; i < pathSize; ++i)
		        {
                    //float[] left = new float[3];
                    //float[] right = new float[3];
                    Vector3 left = Vector3.Zero;
                    Vector3 right = Vector3.Zero;

			        if (i < pathSize-1)
			        {
				        // Next portal.
				        getPortalPoints(path[i], path[i+1], ref left, ref right);
			        }
			        else
			        {
				        // End of the path.
				        left = closestEndPos;
				        right = closestEndPos;
			        }

			        // Right vertex.
			        if (vequal(portalApex, portalRight))
			        {
				        portalRight[0] = right.X;
                        portalRight[1] = right.Y;
                        portalRight[2] = right.Z;
				        rightIndex = i;
			        }
			        else
			        {
				        if (triArea2D(portalApex, portalRight, ref right) <= 0.0f)
				        {
					        if (triArea2D(portalApex, portalLeft, ref right) > 0.0f)
					        {
                                portalRight[0] = right.X;
                                portalRight[1] = right.Y;
                                portalRight[2] = right.Z;
						        rightIndex = i;
					        }
					        else
					        {
						        vcopy(portalApex, portalLeft);
						        apexIndex = leftIndex;

						        if (!vequal(straightPath[straightPathSize - 1], portalApex))
						        {
							        vcopy(straightPath[straightPathSize], portalApex);
							        straightPathSize++;
							        if (straightPathSize >= maxStraightPathSize)
								        return straightPathSize;
						        }

						        vcopy(portalLeft, portalApex);
						        vcopy(portalRight, portalApex);
						        leftIndex = apexIndex;
						        rightIndex = apexIndex;

						        // Restart
						        i = apexIndex;

						        continue;
					        }
				        }
			        }

			        // Left vertex.
			        if (vequal(portalApex, portalLeft))
			        {
                        portalLeft[0] = left.X;
                        portalLeft[1] = left.Y;
                        portalLeft[2] = left.Z;
				        leftIndex = i;
			        }
			        else
			        {
				        if (triArea2D(portalApex, portalLeft, ref left) >= 0.0f)
				        {
					        if (triArea2D(portalApex, portalRight, ref left) < 0.0f)
					        {
                                portalLeft[0] = left.X;
                                portalLeft[1] = left.Y;
                                portalLeft[2] = left.Z;
						        leftIndex = i;
					        }
					        else
					        {
						        vcopy(portalApex, portalRight);
						        apexIndex = rightIndex;

						        if (!vequal(straightPath[straightPathSize - 1], portalApex))
						        {
							        vcopy(straightPath[straightPathSize], portalApex);
							        straightPathSize++;
							        if (straightPathSize >= maxStraightPathSize)
								        return straightPathSize;
						        }

						        vcopy(portalLeft, portalApex);
						        vcopy(portalRight, portalApex);
						        leftIndex = apexIndex;
						        rightIndex = apexIndex;

						        // Restart
						        i = apexIndex;

						        continue;
					        }
				        }
			        }
		        }
	        }

	        // Add end point.
	        //vcopy(straightPath[straightPathSize*3], closestEndPos);
            straightPath[straightPathSize][0] = closestEndPos.X;
            straightPath[straightPathSize][1] = closestEndPos.Y;
            straightPath[straightPathSize][2] = closestEndPos.Z;
	        straightPathSize++;
	
	        return straightPathSize;
        }


        // Returns closest point on navigation polygon.
        // Params:
        //	ref - (in) ref to the polygon.
        //	pos - (in) the point to check.
        //	closest - (out) closest point.
        // Returns: true if closest point found.
        public bool closestPointToPoly(ushort Ref, ref Vector3 pos, ref Vector3 closest)
        {
	        int idx = getPolyIndexByRef(Ref);
	        if (idx == -1)
		        return false;

	        float closestDistSqr = float.MaxValue;
	        dtStatPoly p = getPoly(idx);
	        dtStatPolyDetail pd = getPolyDetail(idx);

            for (int j = 0; j < pd.ntris; ++j)
            {
                byte[] t = getDetailTri(pd.tbase + j);

                //float* v[3];
                //for (int k = 0; k < 3; ++k)
                //{
                //    if (t[k] < p->nv)
                //        v[k] = getVertex((int)((ushort*)p->v)[t[k]]);
                //    else
                //        v[k] = getDetailVertex(pd->vbase + (t[k] - p->nv));
                //}

                //float[][] v = new float[3][];
                Vector3[] v = new Vector3[3];
                for (int k = 0; k < 3; ++k)
                {
                    v[k] = Vector3.Zero;

                    if (t[k] < p.nv)
                    {
                        v[k].X = getVertex((int)p.v[t[k]]).X;
                        v[k].Y = getVertex((int)p.v[t[k]]).Y;
                        v[k].Z = getVertex((int)p.v[t[k]]).Z;

                    }
                    else
                    {
                        v[k].X = getDetailVertex(pd.vbase + (t[k] - p.nv)).X;
                        v[k].Y = getDetailVertex(pd.vbase + (t[k] - p.nv)).Y;
                        v[k].Z = getDetailVertex(pd.vbase + (t[k] - p.nv)).Z;
                    }
                }

                //float[] pt = new float[3];
                Vector3 pt = Vector3.Zero;
                {
                    //closestPtPointTriangle(pt, pos, v[0], v[1], v[2]);
                    closestPtPointTriangle(ref pt, ref pos, ref v[0], ref v[1], ref v[2]);

                    float d = (pos - pt).LengthSquared();//vdistSqr(pos, pt);
                    if (d < closestDistSqr)
                    {
                        //vcopy(closest, pt);
                        closest = pt;
                        closestDistSqr = d;
                    }
                }
            }
	
	        return true;
        }


        // Returns pointer to a polygon based on ref.
        public dtStatPoly getPolyByRef(ushort Ref)
        {
            dtStatPoly result;

            if (!Initialized || (Ref == 0) || (int)Ref > mHeader.npolys)
            {
                //return null;
                result = mHeader.polys[Ref - 1];
            }

            return mHeader.polys[Ref - 1];
        }

        // Returns polygon index based on ref, or -1 if failed.
        public int getPolyIndexByRef(ushort Ref)
        {
            if (!Initialized || (Ref == 0) || (int)Ref > mHeader.npolys) return -1;
            return (int)Ref - 1;
        }

        // Returns number of navigation polygons.
        public int getPolyCount() { return Initialized ? mHeader.npolys : 0; }
        //public int getPolyCount() { return !Initialized ? mHeader.npolys : 0; }

        // Rerturns pointer to specified navigation polygon.
        public dtStatPoly getPoly(int i) { return mHeader.polys[i]; }

        // Returns number of vertices.
        public int getVertexCount() { return !Initialized ? mHeader.nverts : 0; }

        // Returns pointer to specified vertex.
        public Vector3 getVertex(int i) { return mHeader.verts[i]; }

        // Returns number of navigation polygons details.
        public int getPolyDetailCount() { return Initialized ? mHeader.ndmeshes : 0; }

        // Rerturns pointer to specified navigation polygon detail.
        public dtStatPolyDetail getPolyDetail(int i) { return mHeader.dmeshes[i]; }

        // Returns number of detail vertices.
        public int getDetailVertexCount() { return !Initialized ? mHeader.ndverts : 0; }

        // Returns pointer to specified vertex.
        public Vector3 getDetailVertex(int i) { return mHeader.dverts[i]; }

        // Returns number of detail tris.
        public int getDetailTriCount() { return !Initialized ? mHeader.ndtris : 0; }

        // Returns pointer to specified vertex.
        public byte[] getDetailTri(int i) { return mHeader.dtris[i]; }

        public bool isInClosedList(ushort Ref)
        {
	        if (m_nodePool == null) return false;

	        dtNode node = m_nodePool.findNode(Ref);
	        return (node != null) && ((node.flags & (uint)dtNodeFlags.DT_NODE_CLOSED) > 0);
        }

        //public int getMemUsed();

        //public byte* getData() { return m_data; }
        //public int getDataSize() { return m_dataSize; }
        //public dtStatNavMeshHeader* getHeader() { return mHeader; }
        //public dtStatBVNode getBvTreeNodes() { return !Initialized ? mHeader.bvtree : null; }
        public int getBvTreeNodeCount() { return !Initialized ? mHeader.nnodes : 0; }


        bool getPortalPoints(ushort from, ushort to, ref Vector3 left, ref Vector3 right)
        {
            dtStatPoly fromPoly = getPolyByRef(from);
            //if (fromPoly == null)
            //    return false;

            // Find common edge between the polygons and returns the segment end points.
            for (/*unsigned*/int i = 0, j = (int)fromPoly.nv - 1; i < (int)fromPoly.nv; j = i++)
            {
                ushort neighbour = fromPoly.n[j];
                if (neighbour == to)
                {
                    //vcopy(left, getVertex(fromPoly.v[j]));
                    //vcopy(right, getVertex(fromPoly.v[i]));
                    left = getVertex(fromPoly.v[j]);
                    right = getVertex(fromPoly.v[i]);
                    return true;
                }
            }

            return false;
        }


        // Returns edge mid point between two polygons.
        bool getEdgeMidPoint(ushort from, ushort to, ref Vector3 mid)
        {
            //float[] left = new float[3];
            //float[] right = new float[3];
            Vector3 right = Vector3.Zero;
            Vector3 left = Vector3.Zero;

	        if (!getPortalPoints(from, to, ref left, ref right)) return false;

            mid.X = (left.X + right.X) * 0.5f;
            mid.Y = (left.Y + right.Y) * 0.5f;
            mid.Z = (left.Z + right.Z) * 0.5f;

	        return true;
        }

        /*
         * 
         *  Helpers
         *  
         */
        float vdist(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        /*
         * 
         */
        void vcopy(float[] dest, float[] a)
        {
            dest[0] = a[0];
            dest[1] = a[1];
            dest[2] = a[2];
        }


        /*
         * 
         */
        void vsub(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];
        }


        /*
         * 
         */
        float vdot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }


        float vdistSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }


        /*
         * 
         */
        float triArea2D(float[] a, float[] b, ref Vector3 c)
        {
            return ((b[0] * a[2] - a[0] * b[2]) + (c.X * b[2] - b[0] * c.Z) + (a[0] * c.Z - c.X * a[2])) * 0.5f;
        }


        bool checkOverlapBox(ushort[] amin, ushort[] amax, ushort[] bmin, ushort[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        /*
         * 
         */
        static float thr = (float)Math.Sqrt(1.0f / 16384.0f);
        bool vequal(float[] p0, float[] p1)
        {
            float d = vdistSqr(p0, p1);
            return d < thr;
        }


        float distancePtLine2d(ref Vector3 pt, ref Vector3 p, ref Vector3 q)
        {
            float pqx = q.X - p.X;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;

            if (d != 0) t /= d;

            dx = p.X + t * pqx - pt.X;
            dz = p.Z + t * pqz - pt.Z;

            return dx * dx + dz * dz;
        }

        //void closestPtPointTriangle(float[] closest, float[] p, float[] a, float[] b, float[] c)
        void closestPtPointTriangle(ref Vector3 closest, ref Vector3 p, ref Vector3 a, ref Vector3 b, ref Vector3 c)
        {
	        // Check if P in vertex region outside A
            //float[] ab = new float[3];
            //float[] ac = new float[3];
            //float[] ap = new float[3];
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = p - a;

	        float d1 = Vector3.Dot(ab, ap);
	        float d2 = Vector3.Dot(ac, ap);
	        if (d1 <= 0.0f && d2 <= 0.0f)
	        {
		        // barycentric coordinates (1,0,0)
		        closest = a;
		        return;
	        }
	
	        // Check if P in vertex region outside B
            //float[] bp = new float[3];
	        Vector3 bp = p - b;
	        float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
	        if (d3 >= 0.0f && d4 <= d3)
	        {
		        // barycentric coordinates (0,1,0)
		        closest = b;
		        return;
	        }
	
	        // Check if P in edge region of AB, if so return projection of P onto AB
	        float vc = d1*d4 - d3*d2;
	        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
	        {
		        // barycentric coordinates (1-v,v,0)
		        float v = d1 / (d1 - d3);
                closest = a + v * ab; 
                //closest.X = a.X + v * ab.X;
		        //closest.Y = a.Y + v * ab.Y;
		        //closest.Z = a.Z + v * ab.Z;
		        return;
	        }
	
	        // Check if P in vertex region outside C
            //float[] cp = new float[3];
	        Vector3 cp = p -  c;
	        float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
	        if (d6 >= 0.0f && d5 <= d6)
	        {
		        // barycentric coordinates (0,0,1)
		        closest = c;
		        return;
	        }
	
	        // Check if P in edge region of AC, if so return projection of P onto AC
	        float vb = d5*d2 - d1*d6;
	        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
	        {
		        // barycentric coordinates (1-w,0,w)
		        float w = d2 / (d2 - d6);
                closest = a + w * ac;
		        //closest.X = a.X + w * ac.X;
		        //closest.Y = a.Y + w * ac.Y;
		        //closest.Z = a.Z + w * ac.Z;
		        return;
	        }
	
	        // Check if P in edge region of BC, if so return projection of P onto BC
	        float va = d3*d6 - d5*d4;
	        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
	        {
		        // barycentric coordinates (0,1-w,w)
		        float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                closest = b + w * (c - b);
                //closest.X = b.X + w * (c.X - b.X);
                //closest.Y = b.Y + w * (c.Y - b.Y);
                //closest.Z = b.Z + w * (c.Z - b.Z);
		        return;
	        }
	
	        // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
	        float denom = 1.0f / (va + vb + vc);
	        float v0 = vb * denom;
	        float w0 = vc * denom;
            closest = a + ab * v0 + ac * w0;
            //closest.X = a.X + ab.X * v0 + ac.X * w0;
            //closest.Y = a.Y + ab.Y * v0 + ac.Y * w0;
            //closest.Z = a.Z + ab.Z * v0 + ac.Z * w0;
        }

        static float EPS = 1e-4f;

    };

    //#endif // DETOURSTATNAVMESH_H
}
