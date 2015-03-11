using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Engine.Blocks
{
    public class BlockVertexBuffer
    {
        private List<float> _buffer;
        private float[] _bufferProxy;

        public BlockVertexBuffer()
        {
            _buffer = new List<float>();
        }

        public float[] Buffer
        {
            get
            {
                return _bufferProxy;
            }
        }

        public void Add(BlockVertex vertex)
        {
            /* TODO: Swapping back
            _buffer.Add(vertex.Position.X);
            _buffer.Add(vertex.Position.Y);
            _buffer.Add(vertex.Position.Z);
            _buffer.Add(vertex.BlockTextureCoordinate.X);
            _buffer.Add(vertex.BlockTextureCoordinate.Y);
            _buffer.Add(vertex.SunLight);
            */
        }

        public void Update()
        {
            _bufferProxy = _buffer.ToArray();
        }
    }
}