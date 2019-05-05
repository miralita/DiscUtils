using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Xdf {
    public class DiskHeader {
        struct XdfInfo {
            public readonly int tracks;
            public readonly int sectorsPerTrack;
            public readonly int sectorsize;
            public readonly int rpm;

            public XdfInfo(int tracks, int sectors, int sectorsize, int rpm) : this() {
                this.tracks = tracks;
                this.sectorsPerTrack = sectors;
                this.sectorsize = sectorsize;
                this.rpm = rpm;
            }

            public int Size() {
                int sz = tracks * sectorsPerTrack * sectorsize;
                return sz;
            }
        }

        static XdfInfo[] supported = new XdfInfo[] {
            new XdfInfo(160, 15, 512, 0),
            new XdfInfo(154,  8, 1024, 0),
            // 1.44MB
            new XdfInfo(160, 18, 512, 1),
        };
        /*typedef struct {
	UINT32	headersize;
	UINT8	tracks;
	UINT8	sectors;
	UINT8	n;
	UINT8	disktype;
	UINT8	rpm;
} _XDFINFO, *XDFINFO;
static const _XDFINFO supportxdf[] = {
#if 0
			// 256
			{0, 154, 26, 1, DISKTYPE_2HD, 0},
			// 512
			{0, 154, 15, 2, DISKTYPE_2HD, 0},
#endif
#if 1
			// 512
			{0, 160, 15, 2, DISKTYPE_2HD, 0},
#endif
			// 1024
			{0, 154,  8, 3, DISKTYPE_2HD, 0},
			// 1.44MB
			{0, 160, 18, 2, DISKTYPE_2HD, 1},
};

 fdsize = file_getsize(fh);
	file_close(fh);

	xdf = supportxdf;
	while(xdf < (supportxdf + NELEMENTS(supportxdf))) {
		size = xdf->tracks;
		size *= xdf->sectors;
		size <<= (7 + xdf->n);
		if (size == fdsize) {
			fdd->type = DISKTYPE_BETA;
			fdd->protect = ((attr & 1) || (ro))?TRUE:FALSE;
			fdd->inf.xdf = *xdf;
			return(SUCCESS);
		}
		xdf++;
	}*/

        public int Cylinders { get; internal set; }

        internal static Geometry DetectGeometry(long c) {
            var _header = DiskHeader.FromCapacity(c);
            return new Geometry((int)_header.Cylinders, 1, (int)_header.Sectors, (int)_header.Sectorsize);
        }

        public int Sectors { get; internal set; }
        public int Sectorsize { get; internal set; }
        public long Fddsize { get; internal set; }

        internal static DiskHeader Read(Stream s) {
            return DiskHeader.FromCapacity(s.Length);
        }

        internal static DiskHeader FromCapacity(long c) {
            var header = new DiskHeader();
            foreach (var t in supported) {
                if (t.Size() == c) {
                    header.Fddsize = t.Size();
                    header.Cylinders = t.tracks;
                    header.Sectors = t.sectorsPerTrack;
                    header.Sectorsize = t.sectorsize;
                    return header;
                }
            }
            throw new NotSupportedException("Unsupported XDF type: Can't detect geometry from file size");
        }
    }
}
