using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Partitions
{
    public sealed class PC98PartitionTable : PartitionTable {

        private Stream _diskData;
        private Geometry _diskGeometry;
        private VirtualDisk disk;

        public override Guid DiskGuid => Guid.Empty;

        public override ReadOnlyCollection<PartitionInfo> Partitions {
            get {
                var result = new List<PartitionInfo>();
                var rec = new PC98PartitionRecord(_diskData, _diskGeometry.BytesPerSector);
                result.Add(new PC98PartitionInfo(this, rec));
                return new ReadOnlyCollection<PartitionInfo>(result);
            }
        }

        public int Surfaces => disk.Geometry.HeadsPerCylinder;
        public long Sectors => disk.Geometry.TotalSectorsLong;
        public int Sectorsize => disk.Geometry.BytesPerSector;
        public int SectorsPerTrack => disk.Geometry.SectorsPerTrack;

        public override int Create(WellKnownPartitionType type, bool active) {
            throw new NotImplementedException();
        }

        internal static bool IsValid(SparseStream content, int sectorSize) {
            return true;
        }

        public override int Create(long size, WellKnownPartitionType type, bool active) {
            throw new NotImplementedException();
        }

        public override int CreateAligned(WellKnownPartitionType type, bool active, int alignment) {
            throw new NotImplementedException();
        }

        public override int CreateAligned(long size, WellKnownPartitionType type, bool active, int alignment) {
            throw new NotImplementedException();
        }

        public override void Delete(int index) {
            throw new NotImplementedException();
        }

        public static bool IsValid(Stream s) {
            if (s.Length < 0x100) {
                return false;
            }
            s.Position = 0;
            var bootSector = Utilities.ReadFully(s, 0x100);
            if (bootSector[0x100 - 2] != 0x55 || bootSector[0x100 - 1] != 0xAA) {
                return false;
            }
            var nextSector = Utilities.ReadFully(s, 0x100);
            if (nextSector[0] == 0 && nextSector[0x100 - 2] == 0x55 && nextSector[0x100 - 1] == 0xAA) {
                nextSector = Utilities.ReadFully(s, 0x100);
            }
            return true;
        }

        public PC98PartitionTable(VirtualDisk disk) {
            this.disk = disk;
            Init(disk.Content, disk.BiosGeometry);
        }

        private void Init(Stream s, Geometry geometry) {
            _diskData = s;
            _diskGeometry = geometry;
            s.Position = 0;
            byte[] bootSector = Utilities.ReadFully(_diskData, geometry.BytesPerSector);
            var len = bootSector.Length;
            if (bootSector[len - 2] != 0x55 && bootSector[len - 1] != 0xaa) {
                throw new IOException("Invalid boot sector - no magic number 0xAA55");
            }
        }

        internal SparseStream Open(PC98PartitionInfo info) {
            var stream = new SubStream(_diskData, Ownership.None, info.PartitionOffset, info.PartitionEnd);
            stream.FileNameEncoding = Encoding.GetEncoding("shift-jis");
            return stream;
        }
    }
}
