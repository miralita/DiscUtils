using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscUtils.Hdi;

namespace DiscUtils.Partitions {
    public sealed class PC98PartitionTable : PartitionTable {
        private Stream _diskData;
        private Geometry _diskGeometry;
        private VirtualDisk disk;

        public override Guid DiskGuid => Guid.Empty;

        public override ReadOnlyCollection<PartitionInfo> Partitions {
            get {
                var result = new List<PartitionInfo>();
                var rec = new PC98PartitionRecord(_diskData, disk.SectorSize);
                result.Add(new PC98PartitionInfo(this, rec));
                return new ReadOnlyCollection<PartitionInfo>(result);
            }
        }

        public int Surfaces => disk.Geometry.HeadsPerCylinder;
        public long Sectors => disk.Geometry.TotalSectorsLong;
        public int Sectorsize => disk.Geometry.BytesPerSector;
        public int SectorsPerTrack => disk.Geometry.SectorsPerTrack;

        public override int Create(WellKnownPartitionType type, bool active) {
            var geometry = new Geometry(_diskData.Length, _diskGeometry.HeadsPerCylinder, _diskGeometry.SectorsPerTrack,
                _diskGeometry.BytesPerSector);
            var start = new ChsAddress(0, 1, 1);
            var last = geometry.LastSector;
            var startLba = geometry.ToLogicalBlockAddress(start);
            var lastLba = geometry.ToLogicalBlockAddress(last);
            var record = new PC98PartitionRecord();
            record.Bootable = true;
            record.PartitionType = PartType.DOS;
            record.FsType = FSType.FAT12;
            record.IplCyl = record.Cylinder = 1;
            record.IplHead = record.Head = 0;
            record.IplSect = record.Sector = 0;
            //record.EndCyl = (ushort) (disk.BiosGeometry.Cylinders - 2);
            record.EndCyl = (ushort) (last.Cylinder - 2);
            //record.EndHead = (byte) last.Head;
            //record.EndSector = (byte) last.Sector;
            record.EndHead = 0;
            record.EndSector = 0;
            record.Name = "PC98DiskUtils";
            _diskData.Position = disk.SectorSize;
            record.Write(_diskData);
            _diskData.Position = disk.SectorSize * 2;
            var buf = new byte[disk.SectorSize];
            for (var i = 0; i < buf.Length; i++) {
                buf[i] = 0xe5;
            }
            _diskData.Write(buf, 0, buf.Length);
            _diskData.Write(buf, 0, buf.Length);
            _diskData.Position += 0x2000; // отступим загрузчик
            for (var i = _diskData.Position; i < _diskData.Length; i += disk.SectorSize) {
                _diskData.Write(buf, 0, buf.Length);
            }
            _diskData.Flush();
            return 0;
        }

        private static byte ConvertType(WellKnownPartitionType type, long size) {
            switch (type) {
                case WellKnownPartitionType.PC98Fat:
                case WellKnownPartitionType.WindowsFat:
                    if (size < 512 * Sizes.OneMiB) {
                        return BiosPartitionTypes.Fat16;
                    } else if (size < 1023 * (long) 254 * 63 * 512) {
                        // Max BIOS size
                        return BiosPartitionTypes.Fat32;
                    } else {
                        return BiosPartitionTypes.Fat32Lba;
                    }
                case WellKnownPartitionType.WindowsNtfs:
                    return BiosPartitionTypes.Ntfs;
                case WellKnownPartitionType.Linux:
                    return BiosPartitionTypes.LinuxNative;
                case WellKnownPartitionType.LinuxSwap:
                    return BiosPartitionTypes.LinuxSwap;
                case WellKnownPartitionType.LinuxLvm:
                    return BiosPartitionTypes.LinuxLvm;
                default:
                    throw new ArgumentException($"Unrecognized partition type: '{type}'", "type");
            }
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

        public static PC98PartitionTable Initialize(Disk disk, WellKnownPartitionType type) {
            var table = new PC98PartitionTable(disk);
            table.Create(type, true);
            return table;
        }
    }
}