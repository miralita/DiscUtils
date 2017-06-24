using System.Collections.Generic;
using System.IO;

namespace DiscUtils.Hdi {
    public sealed class Disk : VirtualDisk {
        private DiskLayer _file;
        private SparseStream _content;
        public override Geometry Geometry => _file.Geometry;
        public override VirtualDiskClass DiskClass => VirtualDiskClass.HardDisk;
        public override long Capacity => _file.Capacity;

        public override SparseStream Content => _content ?? (_content = _file.OpenContent(null, Ownership.None));

        public override IEnumerable<VirtualDiskLayer> Layers {
            get { yield return _file; }
        }

        public override VirtualDiskTypeInfo DiskTypeInfo => DiskFactory.MakeDiskTypeInfo();
        public DiskHeader Header => _file.Header;

        public PC98Partition PartitionInfo => _file.PartitionInfo;

        public uint PartitionOffset => _file.PartitionOffset;

        public uint BytesPerBlock => _file.BytesPerBlock;

        public override VirtualDisk CreateDifferencingDisk(DiscFileSystem fileSystem, string path) {
            throw new System.NotImplementedException();
        }

        public override VirtualDisk CreateDifferencingDisk(string path) {
            throw new System.NotImplementedException();
        }

        public Disk(string path) {
            var stream = File.Open(path, FileMode.Open);
            _file = new DiskLayer(stream);
        }

        public Disk(DiskLayer file) {
            _file = file;
        }

        public Disk(Stream stream) {
            _file = new DiskLayer(stream);
        }

        public Disk(Stream stream, Ownership ownStream) {
            _file = new DiskLayer(stream, ownStream);
        }

        public static Disk InitializeFixed(Stream stream, Ownership ownsStream, HddType capacity) {
            return new Disk(DiskLayer.InitializeFixed(stream, ownsStream, capacity));
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) _file.Dispose();
            } finally {
                base.Dispose(disposing);
            }
        }
    }
}