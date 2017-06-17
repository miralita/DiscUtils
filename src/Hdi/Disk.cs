using System.Collections.Generic;

namespace DiscUtils.Hdi {
    public sealed class Disk : VirtualDisk {
        public override Geometry Geometry { get; }
        public override VirtualDiskClass DiskClass { get; }
        public override long Capacity { get; }
        public override SparseStream Content { get; }
        public override IEnumerable<VirtualDiskLayer> Layers { get; }
        public override VirtualDiskTypeInfo DiskTypeInfo { get; }
        public override VirtualDisk CreateDifferencingDisk(DiscFileSystem fileSystem, string path) {
            throw new System.NotImplementedException();
        }

        public override VirtualDisk CreateDifferencingDisk(string path) {
            throw new System.NotImplementedException();
        }
    }
}