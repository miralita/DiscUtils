using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Hdi
{
    internal class DiskStream : SparseStream {
        private Stream stream;
        private uint dataOffset;
        private List<StreamExtent> _extents;

        public override void Flush() {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return stream.Seek(offset + dataOffset, origin);
        }

        public override void SetLength(long value) {
            stream.SetLength(dataOffset + value);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            stream.Write(buffer, offset, count);
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length - dataOffset;

        public override long Position {
            get => stream.Position - dataOffset;
            set => stream.Position = value + dataOffset;
        }

        public override IEnumerable<StreamExtent> Extents => new[] { new StreamExtent(0, stream.Length - dataOffset) };

        public DiskStream(Stream stream, uint offset) {
            dataOffset = offset;
            this.stream = stream;
            stream.Position = offset;
            this.FileNameEncoding = Encoding.GetEncoding("shift-jis");
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) stream.Dispose();
            } finally {
                base.Dispose(disposing);
            }
        }
    }
}
