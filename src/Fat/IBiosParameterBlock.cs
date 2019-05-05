using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Fat {
    interface IBiosParameterBlock {
        FatType DetectFatType();
        FatType FatType { get; }
        string Oemname { get; }
        ushort BytesPerSector { get; }
        byte SectorsPerCluster { get; }
        ushort ReservedSectors { get; }
        byte NumFATs { get; }
        ushort RootEntriesCnt { get; }
        ushort FSVer { get; }
        uint TotalSectors { get; }
        byte Media { get;}
        uint SectorsPerFat { get; }
        ushort SectorsPerTrack { get; }
        ushort NumHeads { get; }
        uint HiddenSectors { get; }
        ushort ExtFlags { get; }
        uint RootClus { get; }
        ushort BkBootSec { get; }
    }
}
