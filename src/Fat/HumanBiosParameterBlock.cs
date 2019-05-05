using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DiscUtils.Fat {

    class HumanBiosParameterBlock : IBiosParameterBlock {
        /// <summary>68k bra.S</summary>
        public readonly byte[] jump; // 0
        /// <summary>OEM Name, 16 bytes, space-padded</summary>
        public string Oemname { get; set; } // 2
        /// <summary>Bytes per cluster</summary>
        public readonly ushort bpc; // 18
        /// <summary>Unknown, seen 1, 2 and 16</summary>
        public readonly byte unknown1; // 20
        /// <summary>Unknown, always 512?</summary>
        public readonly ushort unknown2; // 21
        /// <summary>Unknown, always 1?</summary>
        public readonly byte unknown3; // 23
        /// <summary>Number of entries on root directory</summary>
        public ushort RootEntriesCnt { get; set; } // 24
        /// <summary>Clusters, set to 0 if more than 65536</summary>
        public readonly ushort clusters; // 26
        /// <summary>Media descriptor</summary>
        public byte Media { get; set; } // 28
        /// <summary>Clusters per FAT, set to 0</summary>
        public readonly byte cpfat; // 29
        /// <summary>Clustersin volume</summary>
        public readonly uint big_clusters; // 30
        /// <summary>Boot code.</summary>
        public readonly byte[] boot_code; // 34

        public ushort BytesPerSector { get; set; }
        public byte SectorsPerCluster { get; set; }
        public FatType FatType { get; set; }
        public ushort ReservedSectors => 1;
        public byte NumFATs => 2;
        public ushort FSVer => 0;

        public uint TotalSectors => clusters;
        public uint SectorsPerFat => (uint)cpfat * SectorsPerCluster;
        //unused
        public ushort SectorsPerTrack => 0;
        public ushort NumHeads => 0;
        public uint HiddenSectors => 0;
        public ushort ExtFlags => 0;
        public uint RootClus => 0;
        public ushort BkBootSec => 0;

        public HumanBiosParameterBlock(byte[] data) {
            Oemname = Encoding.ASCII.GetString(data, 2, 16).TrimEnd('\0');
            bpc = Utilities.ToUInt16BigEndian(data, 18);
            RootEntriesCnt = Utilities.ToUInt16BigEndian(data, 24);
            clusters = Utilities.ToUInt16BigEndian(data, 26);
            Media = data[28];
            cpfat = data[29];
            big_clusters = Utilities.ToUInt32BigEndian(data, 30);
            BytesPerSector = bpc;
            FatType = DetectFatType();
        }

        public static HumanBiosParameterBlock TryInit(byte[] data, long size, uint sectorSize) {
            if (!CheckClusters(data, size) || !CheckOem(data) || !CheckBranch(data)) return null;
            var bpb = new HumanBiosParameterBlock(data);
            bpb.BytesPerSector = (ushort)sectorSize;
            bpb.SectorsPerCluster = (byte)(bpb.bpc / sectorSize);
            return bpb;
        }

        private static bool CheckClusters(byte[] bpb, long size) {
            var bpc = Utilities.ToInt16BigEndian(bpb, 18);
            long expectedClusters = bpc > 0 ? size / bpc : 0;
            ushort clusters = Utilities.ToUInt16BigEndian(bpb, 26);
            uint big_clusters = Utilities.ToUInt32BigEndian(bpb, 30);

            // Check clusters for Human68k are correct
            return clusters == 0
                ? big_clusters == expectedClusters
                : clusters == expectedClusters;
        }

        private static bool CheckOem(byte[] bpb) {
            // Check OEM for Human68k is correct
            return bpb[2] >= 0x20 && bpb[3] >= 0x20 && bpb[4] >= 0x20 &&
                bpb[5] >= 0x20 && bpb[6] >= 0x20 && bpb[7] >= 0x20 &&
                bpb[8] >= 0x20 && bpb[9] >= 0x20 && bpb[10] >= 0x20 &&
                bpb[11] >= 0x20 && bpb[12] >= 0x20 && bpb[13] >= 0x20 &&
                bpb[14] >= 0x20 && bpb[15] >= 0x20 && bpb[16] >= 0x20 &&
                bpb[17] >= 0x20;
        }

        private static bool CheckBranch(byte[] bpb) {
            // Check correct branch for Human68k
            return bpb[0] == 0x60 && bpb[1] >= 0x1C && bpb[1] < 0xFE;
        }

        public FatType DetectFatType() {
            return FatType.Fat12;
        }
    }
}
