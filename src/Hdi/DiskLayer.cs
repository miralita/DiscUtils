using System;
using System.IO;

namespace DiscUtils.Hdi {
    public sealed class DiskLayer :VirtualDiskLayer {
        private Geometry _geometry;
        private long _capacity;
        private FileLocator _relativeFileLocator;
        private DiskHeader _header;
        private Stream _stream;
        private Ownership _ownStream;

        public override Geometry Geometry => Geometry.FromCapacity(_header.Hddsize, (int)_header.Sectorsize);

        public override bool IsSparse => false;

        public override bool NeedsParent => false;

        internal override long Capacity => _header.Hddsize;

        internal override FileLocator RelativeFileLocator => null;

        public override SparseStream OpenContent(SparseStream parent, Ownership ownsParent) {
            throw new System.NotImplementedException();
        }

        public override string[] GetParentLocations() {
            throw new System.NotImplementedException();
        }

        public DiskLayer(Stream stream) {
            _stream = stream;
            _header = DiskHeader.Read(stream);
        }

        public DiskLayer(Stream stream, Ownership ownStream) {
            _stream = stream;
            _ownStream = ownStream;
            _header = DiskHeader.Read(stream);
        }

    }
}
