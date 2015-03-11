using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simsip.LineRunner.ContentPipeline
{
    public class ModelVertices
    {
        private Vector3[] _vertices;
        private int[] _indices;

        public ModelVertices(Vector3[] vertices, int[] indices)
        {
            this._vertices = vertices;
            this._indices = indices;
        }

        public Vector3[] Vertices 
        { 
            get 
            { 
                return this._vertices; 
            } 
        }

        public int[] Indices
        {
            get
            {
                return this._indices;
            }
        }
    }
}
