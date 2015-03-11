using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace Simsip.LineRunner.Shaders
{
    public class TexturedCube
    {
        private float[] _vertices;
        private int[] _indices;
        private float[] _textureCoords;

        public TexturedCube()
            : base()
        {
            Initialize();

            VertexCount = 24;
            IndicesCount = 36;
            TextureCoordsCount = 24;
        }
        public float[] Vertices { get { return _vertices; } }
        public float[] TextureCoords { get { return _textureCoords; } }
        public int[] Indices { get { return _indices; } }


        private void Initialize()
        {
            _vertices = new float[] {
                //left
                0.5f, 0.5f, 0.5f,
                 200.5f,  200.5f, 0.5f,
                 200.5f, 0.5f, 0.5f,
                0.5f,  200.5f, 0.5f,
 
                //back
                200.5f, 0.5f, 0.5f,
                200.5f,  200.5f, 0.5f,
                200.5f,  200.5f,  200.5f,
                200.5f, 0.5f,  200.5f,
 
                //right
                0.5f, 0.5f,  200.5f,
                 200.5f, 0.5f,  200.5f,
                 200.5f,  200.5f,  200.5f,
                0.5f,  200.5f,  200.5f,
 
                //top
                 200.5f, 200.5f,  0.5f,
                0.5f, 200.5f,  0.5f,
                 200.5f, 200.5f,   200.5f,
                0.5f, 200.5f,   200.5f,
 
                //front
                0.5f, 0.5f,  0.5f, 
                0.5f, 200.5f,   200.5f, 
                0.5f, 200.5f,  0.5f,
                0.5f, 0.5f,   200.5f,
 
                //bottom
                0.5f, 0.5f, 0.5f, 
                200.5f, 0.5f, 0.5f,
                 200.5f, 0.5f,  200.5f,
                0.5f, 0.5f,  200.5f
            };

            _indices = new int[] {
                //left
                0,1,2,0,3,1,
 
                //back
                4,5,6,4,6,7,
 
                //right
                8,9,10,8,10,11,
 
                //top
                13,14,12,13,15,14,
 
                //front
                16,17,18,16,19,17,
 
                //bottom 
                20,21,22,20,22,23
            };

            _textureCoords = new float[] {
                // left
                 0.0f, 0.0f,
                -1.0f, 1.0f,
                -1.0f, 0.0f,
                 0.0f, 1.0f,
 
                // back
                 0.0f, 0.0f,
                 0.0f, 1.0f,
                -1.0f, 1.0f,
                -1.0f, 0.0f,
 
                // right
                -1.0f, 0.0f,
                 0.0f, 0.0f,
                 0.0f, 1.0f,
                -1.0f, 1.0f,
 
                // top
                 0.0f, 0.0f,
                 0.0f, 1.0f,
                -1.0f, 0.0f,
                -1.0f, 1.0f,
 
                // front
                0.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                1.0f, 0.0f,
 
                // bottom
                 0.0f, 0.0f,
                 0.0f, 1.0f,
                -1.0f, 1.0f,
                -1.0f, 0.0f
            };
        }

        public int VertexCount { get; set; }
        public int IndicesCount { get; set; }
        public int TextureCoordsCount { get; set; }

        public Vector3[] GetVerts()
        {
            return new Vector3[] {
            //left
            new Vector3(-0.5f, -0.5f,  -0.5f),
            new Vector3(0.5f, 0.5f,  -0.5f),
            new Vector3(0.5f, -0.5f,  -0.5f),
            new Vector3(-0.5f, 0.5f,  -0.5f),
 
            //back
            new Vector3(0.5f, -0.5f,  -0.5f),
            new Vector3(0.5f, 0.5f,  -0.5f),
            new Vector3(0.5f, 0.5f,  0.5f),
            new Vector3(0.5f, -0.5f,  0.5f),
 
            //right
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3(0.5f, -0.5f,  0.5f),
            new Vector3(0.5f, 0.5f,  0.5f),
            new Vector3(-0.5f, 0.5f,  0.5f),
 
            //top
            new Vector3(0.5f, 0.5f,  -0.5f),
            new Vector3(-0.5f, 0.5f,  -0.5f),
            new Vector3(0.5f, 0.5f,  0.5f),
            new Vector3(-0.5f, 0.5f,  0.5f),
 
            //front
            new Vector3(-0.5f, -0.5f,  -0.5f), 
            new Vector3(-0.5f, 0.5f,  0.5f), 
            new Vector3(-0.5f, 0.5f,  -0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
 
            //bottom
            new Vector3(-0.5f, -0.5f,  -0.5f), 
            new Vector3(0.5f, -0.5f,  -0.5f),
            new Vector3(0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f)
 
        };
        }

        public int[] GetIndices(int offset = 0)
        {
            int[] inds = new int[] {
            //left
            0,1,2,0,3,1,
 
            //back
            4,5,6,4,6,7,
 
            //right
            8,9,10,8,10,11,
 
            //top
            13,14,12,13,15,14,
 
            //front
            16,17,18,16,19,17,
 
            //bottom 
            20,21,22,20,22,23
        };

            if (offset != 0)
            {
                for (int i = 0; i < inds.Length; i++)
                {
                    inds[i] += offset;
                }
            }

            return inds;
        }

        public Vector2[] GetTextureCoords()
        {
            return new Vector2[] {
            // left
            new Vector2(0.0f, 0.0f),
            new Vector2(-1.0f, 1.0f),
            new Vector2(-1.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
 
            // back
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(-1.0f, 1.0f),
            new Vector2(-1.0f, 0.0f),
 
            // right
            new Vector2(-1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(-1.0f, 1.0f),
 
            // top
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(-1.0f, 0.0f),
            new Vector2(-1.0f, 1.0f),
 
            // front
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
 
            // bottom
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(-1.0f, 1.0f),
            new Vector2(-1.0f, 0.0f)
        };
        }
    }
}
