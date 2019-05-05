using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Fat {
    class FatBiosParameterBlock : IBiosParameterBlock {
        public string Oemname { get; set; }
        public ushort BytesPerSector { get; set; }
        public byte SectorsPerCluster { get; set; }

        public ushort ReservedSectors { get; set; }
        public byte NumFATs { get; set; }
        public ushort RootEntriesCnt { get; set; }
        public ushort TotalSectors16 { get; set; }
        public byte Media { get; set; }
        public ushort SectorsPerFAT16 { get; set; }
        public ushort SectorsPerTrack { get; set; }
        public ushort NumHeads { get; set; }
        public uint HiddenSectors { get; set; }
        public uint TotalSectors32 { get; set; }

        public uint SectorsPerFAT32 { get; set; }
        public ushort ExtFlags { get; set; }
        public ushort FSVer { get; set; }
        public uint RootClus { get; set; }
        public ushort FSInfo { get; set; }
        public ushort BkBootSec { get; set; }

        public byte DrvNum { get; set; }
        public byte BootSig { get; set; }
        public uint VolId { get; set; }
        public string VolLab { get; set; }
        public string FilSysType { get; set; }
        public FatType FatType { get; set; }
        public uint TotalSectors => TotalSectors16 != 0 ? TotalSectors16 : TotalSectors32;
        public uint SectorsPerFat => SectorsPerFAT16 != 0 ? SectorsPerFAT16 : SectorsPerFAT32;

        public FatBiosParameterBlock(byte[] data) {
            Oemname = Encoding.ASCII.GetString(data, 3, 8).TrimEnd('\0');
            BytesPerSector = Utilities.ToUInt16LittleEndian(data, 11);
            SectorsPerCluster = data[13];
            ReservedSectors = Utilities.ToUInt16LittleEndian(data, 14);
            NumFATs = data[16];
            RootEntriesCnt = Utilities.ToUInt16LittleEndian(data, 17);
            TotalSectors16 = Utilities.ToUInt16LittleEndian(data, 19);
            Media = data[21];
            SectorsPerFAT16 = Utilities.ToUInt16LittleEndian(data, 22);
            SectorsPerTrack = Utilities.ToUInt16LittleEndian(data, 24);
            NumHeads = Utilities.ToUInt16LittleEndian(data, 26);
            HiddenSectors = Utilities.ToUInt32LittleEndian(data, 28);
            TotalSectors32 = Utilities.ToUInt32LittleEndian(data, 32);

            SectorsPerFAT32 = Utilities.ToUInt32LittleEndian(data, 36);
            DetectFatType();

            if (FatType != FatType.Fat32) {
                ReadBS(data, 36);
            } else {
                SectorsPerFAT32 = Utilities.ToUInt32LittleEndian(data, 36);
                ExtFlags = Utilities.ToUInt16LittleEndian(data, 40);
                FSVer = Utilities.ToUInt16LittleEndian(data, 42);
                RootClus = Utilities.ToUInt32LittleEndian(data, 44);
                FSInfo = Utilities.ToUInt16LittleEndian(data, 48);
                BkBootSec = Utilities.ToUInt16LittleEndian(data, 50);
                ReadBS(data, 64);
            }
        }

        private void ReadBS(byte[] _bootSector, int offset) {
            DrvNum = _bootSector[offset];
            BootSig = _bootSector[offset + 2];
            VolId = Utilities.ToUInt32LittleEndian(_bootSector, offset + 3);
            VolLab = Encoding.ASCII.GetString(_bootSector, offset + 7, 11);
            FilSysType = Encoding.ASCII.GetString(_bootSector, offset + 18, 8);
        }

        public FatType DetectFatType() {

            uint rootDirSectors = (((uint)RootEntriesCnt * 32) + BytesPerSector - 1) / BytesPerSector;
            uint fatSz = (SectorsPerFAT16 != 0) ? (uint)SectorsPerFAT16 : SectorsPerFAT32;
            uint totalSec = (TotalSectors16 != 0) ? (uint)TotalSectors16 : TotalSectors32;

            uint dataSec = totalSec - (ReservedSectors + (NumFATs * fatSz) + rootDirSectors);
            uint countOfClusters = dataSec / SectorsPerCluster;

            if (countOfClusters < 4085) {
                this.FatType = FatType.Fat12;
            } else if (countOfClusters < 65525) {
                this.FatType = FatType.Fat16;
            } else {
                this.FatType = FatType.Fat32;
            }
            return this.FatType;
        }
    }
}
