using System;
using System.IO;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace Craft.Net.Networking
{
    public class DualStream : Stream
    {
        private IInputStream _inputStream;
        private IOutputStream _outputStream;

        public DualStream(IInputStream inputStream, IOutputStream outputStream)
        {
            this._inputStream = inputStream;
            this._outputStream = outputStream;
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get { return true; } }

        public override void Flush()
        {
            // No-op
        }

        public long PendingWrites
        {
            get { return 0; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return 0; }
            set {  }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            
        }
    }
}
