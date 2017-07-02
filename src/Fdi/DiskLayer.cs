using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Fdi
{
    public sealed class DiskLayer : VirtualDiskLayer
    {
        private Geometry _geometry;
        private long _capacity;
        private FileLocator _relativeFileLocator;
        private DiskHeader _header;
        private Stream _stream;
        private Ownership _ownStream;
        private const int blockOffset = 4096;
        private uint partitionOffset;
        private uint bytesPerBlock;

        //public override Geometry Geometry => Geometry.FromCapacity(_header.Hddsize, (int)_header.Sectorsize);
        //public Geometry(int cylinders, int headsPerCylinder, int sectorsPerTrack, int bytesPerSector)
        public override Geometry Geometry => new Geometry((int)_header.Cylinders, (int)_header.Surfaces, (int)_header.Sectors, (int)_header.Sectorsize);

        public override bool IsSparse => false;

        public override bool NeedsParent => false;

        internal override long Capacity => _header.Fddsize;

        internal override FileLocator RelativeFileLocator => null;

        public DiskHeader Header => _header;

        public uint PartitionOffset => partitionOffset;

        public uint BytesPerBlock {
            get { return bytesPerBlock; }
        }

        public override SparseStream OpenContent(SparseStream parent, Ownership ownsParent) {
            //var s = new DiskStream(_stream, partitionOffset);
            var s = new SubStream(_stream, blockOffset, _stream.Length - blockOffset);
            return s;
        }

        public override string[] GetParentLocations() {
            throw new NotImplementedException();
        }

        public DiskLayer(Stream stream) {
            _stream = stream;
            _header = DiskHeader.Read(stream);
            // skip bootloader
            stream.Position = blockOffset + _header.Sectorsize;
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
