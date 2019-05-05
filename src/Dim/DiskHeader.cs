using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Dim {
    public enum DimDiskType : byte {
        Hd2 = 0,
        Hs2 = 1,
        Hc2 = 2,
        Hde2 = 3,
        Hq2 = 9,
        N88 = 17
    }
    public class DiskHeader {
        const int DATA_OFFSET = 0x100;
        readonly static string headerId = "DIFC HEADER  ";

        public long DiskSize { get; private set; }
        public DimDiskType DiskType { get; private set; }
        public string HeaderId { get; private set; }
        public string Comment { get; private set; }
        public uint SectorSize { get; private set; }
        public uint Cylinders { get; private set; }
        public uint Heads { get; private set; }
        public uint SectorsPerTrack { get; private set; }
        public ulong Sectors { get; private set; }

        internal static DiskHeader Read(Stream s) {
            var header = new DiskHeader();
            s.Seek(0, SeekOrigin.Begin);
            var data = Utilities.ReadFully(s, DATA_OFFSET);

            header.DiskSize = s.Length - DATA_OFFSET;
            header.DiskType = (DimDiskType)data[0];
            header.HeaderId = Utilities.ToASCIIString(data, 0xab, 13);
            header.Comment = Utilities.ToASCIIString(data, 0xc2, 60);
            if (header.HeaderId != headerId) {
                throw new InvalidDataException("Wrong Header ID for DIM image");
            }
            switch (header.DiskType) {
                // 8 spt, 1024 bps
                case DimDiskType.Hd2:
                    if (header.DiskSize % (2 * 8 * 1024) != 0) {
                        throw new InvalidDataException($"DIM shows unknown image with {header.DiskSize / (2 * 8 * 1024)} tracks");
                    }

                    if (header.DiskSize / (2 * 8 * 1024) == 77) {
                        header.Cylinders = 77;
                        header.Heads = 2;
                        header.SectorsPerTrack = 8;
                    }
                    header.SectorSize = 1024;
                    break;
                // 9 spt, 1024 bps
                case DimDiskType.Hs2:
                    if (header.DiskSize % (2 * 9 * 512) != 0) {
                        throw new InvalidDataException($"DIM shows unknown image with {header.DiskSize / (2 * 9 * 512)} tracks");
                    }

                    if (header.DiskSize / (2 * 9 * 512) == 80) {
                        header.Cylinders = 80;
                        header.Heads = 2;
                        header.SectorsPerTrack = 9;
                    }
                    header.SectorSize = 512;
                    break;
                // 15 spt, 512 bps
                case DimDiskType.Hc2:
                    if (header.DiskSize % (2 * 15 * 512) != 0) {
                        throw new InvalidDataException($"DIM shows unknown image with {header.DiskSize / (2 * 15 * 512)} tracks");
                    }

                    if (header.DiskSize / (2 * 15 * 512) == 80) {
                        header.Cylinders = 80;
                        header.Heads = 2;
                        header.SectorsPerTrack = 15;
                    }
                    header.SectorSize = 512;
                    break;
                // 9 spt, 1024 bps
                case DimDiskType.Hde2:
                    if (header.DiskSize % (2 * 9 * 512) != 0) {
                        throw new InvalidDataException($"DIM shows unknown image with {header.DiskSize / (2 * 9 * 512)} tracks");
                    }

                    if (header.DiskSize / (2 * 9 * 512) == 80) {
                        header.Cylinders = 80;
                        header.Heads = 2;
                        header.SectorsPerTrack = 9;
                    }
                    header.SectorSize = 512;
                    break;
                // 18 spt, 512 bps
                case DimDiskType.Hq2:
                    if (header.DiskSize % (2 * 18 * 512) != 0) {
                        throw new InvalidDataException($"DIM shows unknown image with {header.DiskSize / (2 * 18 * 512)} tracks");
                    }

                    if (header.DiskSize / (2 * 18 * 512) == 80) {
                        header.Cylinders = 80;
                        header.Heads = 2;
                        header.SectorsPerTrack = 18;
                    }
                    header.SectorSize = 512;
                    break;
                // 26 spt, 256 bps
                case DimDiskType.N88:
                    if (header.DiskSize % (2 * 26 * 256) == 0) {
                        //if (header.DiskSize % (2 * 26 * 256) == 77);
                        header.SectorSize = 256;
                    } else if (header.DiskSize % (2 * 26 * 128) == 0) {
                        //if (header.DiskSize % (2 * 26 * 128) == 77);
                        header.SectorSize = 256;
                    } else {
                        throw new InvalidDataException($"DIM shows unknown image with {header.DiskSize / (2 * 26 * 256)} tracks");
                    }
                    header.Cylinders = 77;
                    header.Heads = 2;
                    header.SectorsPerTrack = 26;

                    break;
            }
            header.Sectors = (ulong)header.DiskSize / header.SectorSize;

            return header;
        }
    }
}
