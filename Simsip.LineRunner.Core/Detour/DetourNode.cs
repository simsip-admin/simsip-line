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

//#ifndef DETOURNODE_H
//#define DETOURNODE_H

using System;

namespace EGEngine
{
    public enum dtNodeFlags
    {
        DT_NODE_OPEN = 0x01,
        DT_NODE_CLOSED = 0x02,
    };

    class dtNode
    {
        public float cost;
        public float total;
        public uint id;
        public uint pidx;
        public uint flags;
        public int nodeIdx;

        public dtNode(){}
        public dtNode(int i, uint p, uint f)
        {
            pidx = p;
            flags = f;
            nodeIdx = i;

            cost = 0;
            total = 0;
            id = 0;
        }
    };

    class dtNodePool
    {
        dtNode[] m_nodes;
        int m_maxNodes;
        int m_nodeCount;

        public dtNodePool()
        {
        }

        public dtNodePool(int maxNodes)
        {
            m_maxNodes = maxNodes;
            m_nodeCount = 0;

            m_nodes = new dtNode[maxNodes];
            for (int i = 0; i < m_maxNodes; i++)
            {
                m_nodes[i] = new dtNode(-1, 30, 2);
            }
        }

        public void clear()
        {
            for (int i = 0; i < m_maxNodes; i++)
            {
                m_nodes[i].nodeIdx = -1;
                m_nodes[i].id = 0;
            }
        
            m_nodeCount = 0;
        }

        public dtNode getNode(uint id)
        {
            for (int i = 0; i < m_maxNodes; i++)
            {
                if (m_nodes[i].id == id)
                {
                    return m_nodes[i];
                }                

                if (m_nodes[i].nodeIdx < 0)
                {
                    m_nodes[i].nodeIdx = i;
                    m_nodes[i].pidx = 0;
                    m_nodes[i].cost = 0;
                    m_nodes[i].total = 0;
                    m_nodes[i].id = id;
                    m_nodes[i].flags = 0;

                    return m_nodes[i];
                }
            }

            return null;
        }

        public uint getNodeIdx(dtNode node)
        {
            if (node == null) return 0;

            return (uint)(node.nodeIdx) + 1;
        }

        public dtNode getNodeAtIdx(uint idx)
        {
            if (idx == 0) return null;

            return m_nodes[idx - 1];
        }

        public dtNode findNode(uint id)
        {
            for (int i = 0; i < m_maxNodes; i++)
            {
                if (m_nodes[i].id == id)
                {
                    return m_nodes[i];
                }
            }

            return null;
        }
    };

    unsafe class dtNodeQueue
    {
        dtNode[] m_heap;

        int m_capacity;
        int m_size;

        public dtNodeQueue(int n)
        {
            m_capacity = n;
            m_size = 0;
            m_heap = new dtNode[m_capacity + 1];
        }

        public void clear()
        {
            m_size = 0;
        }

        public dtNode top()
        {
            return m_heap[0];
        }

        public dtNode pop()
        {
            int bestIndex = 0;
            float bestCost = float.MaxValue;

            for (int i = 0; i < m_size; i++)
            {
                if (m_heap[i].total < bestCost)
                {
                    bestIndex = i;
                    bestCost = m_heap[i].total;
                }
            }

            dtNode result = m_heap[bestIndex];

            for (int i = bestIndex; i < (m_size - 1); i++)
            {
                m_heap[i] = m_heap[i + 1];
            }

            m_size--;

            return result;
        }

        public void push(dtNode node)
        {
            m_heap[m_size] = node;
            m_size++;
        }

        public void modify(dtNode node)
        {
            for (int i = 0; i < m_size; i++)
            {
                if (m_heap[i].id == node.id)
                {
                    m_heap[i] = node;
                }
            }
        }

        public bool empty()
        {
            return m_size == 0;
        }
    };
}