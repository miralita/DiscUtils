using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.D88
{
    public sealed class DiskLayer : VirtualDiskLayer
    {
        private Geometry _geometry;
        private long _capacity;
        private FileLocator _relativeFileLocator;
        private DiskHeader _header;
        private Stream _stream;
        private Ownership _ownStream;
        private uint partitionOffset;
        private uint bytesPerBlock;

        //public override Geometry Geometry => Geometry.FromCapacity(_header.Hddsize, (int)_header.Sectorsize);
        //public Geometry(int cylinders, int headsPerCylinder, int sectorsPerTrack, int bytesPerSector)
        public override Geometry Geometry => Geometry.FromCapacity(_header.Fddsize);

        public override bool IsSparse => false;

        public override bool NeedsParent => false;

        internal override long Capacity => _totalSize;

        internal override FileLocator RelativeFileLocator => null;

        public DiskHeader Header => _header;

        public uint PartitionOffset => partitionOffset;

        public uint BytesPerBlock {
            get { return bytesPerBlock; }
        }

        public override SparseStream OpenContent(SparseStream parent, Ownership ownsParent) {
            //var s = new DiskStream(_stream, partitionOffset);
            //var s = new SubStream(_stream, _header.Offset, _header.Fddsize);
            //return s;
            return new SubStream(_content, 0, _totalSize);
        }

        public override string[] GetParentLocations() {
            throw new NotImplementedException();
        }

        private Stream _content;
        private int _totalSize = 0;
        public DiskLayer(Stream stream) {
            _stream = stream;
            _header = DiskHeader.Read(stream);
            // skip bootloader
            stream.Position = _header.Offset;
            var ms = new MemoryStream();
            var hd = new byte[0x10];
            var sector = new byte[4096];
            var totalSize = 0;
            while (stream.Position < _header.Fddsize) {
                stream.Read(hd, 0, 0x10);
                var size = hd[14] | (hd[15] << 8);
                if (size > sector.Length) sector = new byte[size];
                stream.Read(sector, 0, size);
                ms.Write(sector, 0, size);
                totalSize += size;
            }
            ms.Position = 0;
            _content = ms;
            _totalSize = totalSize;
            stream.Position = _header.Offset;
        }

        public DiskLayer(Stream stream, Ownership ownStream) : this(stream) {
            _ownStream = ownStream;
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) _stream.Dispose();
            } finally {
                base.Dispose(disposing);
            }
        }
    }
}
