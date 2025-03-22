using System;
using System.Collections.Generic;
using System.IO;

namespace DNS.Protocol.Utils
{
    public class ByteStream : Stream
    {
        private readonly byte[] _buffer;
        private int _offset = 0;

        public ByteStream(int capacity)
        {
            _buffer = new byte[capacity];
        }

        public ByteStream Append(IEnumerable<byte[]> buffers)
        {
            foreach (byte[] buf in buffers)
            {
                Write(buf, 0, buf.Length);
            }

            return this;
        }

        public ByteStream Append(byte[] buf)
        {
            Write(buf, 0, buf.Length);
            return this;
        }

        public byte[] ToArray()
        {
            return _buffer;
        }

        public void Reset()
        {
            _offset = 0;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _buffer.Length > 0 && _offset < _buffer.Length; }
        }

        public override void Flush() { }

        public override long Length
        {
            get { return _offset; }
        }

        public override long Position
        {
            get => throw new NotImplementedException(); set => throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Array.Copy(buffer, offset, this._buffer, this._offset, count);
            this._offset += count;
        }
    }
}
