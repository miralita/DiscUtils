using System.IO;

namespace DiscUtils.Hdi {
    [VirtualDiskFactory("HDI", ".hdi")]
    internal sealed class DiskFactory : VirtualDiskFactory {
        public override string[] Variants => new string[] { };

        public override VirtualDiskTypeInfo GetDiskTypeInformation(string variant) {
            return MakeDiskTypeInfo();
        }

        public override DiskImageBuilder GetImageBuilder(string variant) {
            throw new System.NotImplementedException();
        }

        public override VirtualDisk CreateDisk(FileLocator locator, string variant, string path, VirtualDiskParameters diskParameters) {
            throw new System.NotImplementedException();
        }

        public override VirtualDisk OpenDisk(string path, FileAccess access) {
            return new Disk(path, access);
        }

        public override VirtualDisk OpenDisk(FileLocator locator, string path, FileAccess access) {
            var share = access == FileAccess.Read ? FileShare.Read : FileShare.None;
            return new Disk(locator.Open(path, FileMode.Open, access, share), Ownership.Dispose);
        }

        public override VirtualDiskLayer OpenDiskLayer(FileLocator locator, string path, FileAccess access) {
            return null;
        }
        
        internal static VirtualDiskTypeInfo MakeDiskTypeInfo()
        {
            return new VirtualDiskTypeInfo()
            {
                Name = "HDI",
                Variant = string.Empty,
                CanBeHardDisk = true,
                DeterministicGeometry = true,
                PreservesBiosGeometry = false,
                CalcGeometry = c => Geometry.FromCapacity(c),
            };
        }
    }
}
