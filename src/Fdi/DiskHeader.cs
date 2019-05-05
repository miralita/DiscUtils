using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Fdi {
    public class DiskHeader {
        /*typedef struct {
	UINT8	dummy[4];
	UINT8	fddtype[4];
	UINT8	headersize[4];
	UINT8	fddsize[4];
	UINT8	sectorsize[4];
	UINT8	sectors[4];
	UINT8	surfaces[4];
	UINT8	cylinders[4];
} FDIHDR;*/
        public static int Size => 8 * 4;
        private uint dummy;

        private uint fddtype;
        private uint headersize;
        private uint fddsize;
        private uint sectorsize;
        private uint sectors;
        private uint surfaces;
        private uint cylinders;
        public uint Dummy {
            get { return dummy; }
            set { dummy = value; }
        }

        public uint Fddtype {
            get { return fddtype; }
            set { fddtype = value; }
        }

        public uint Headersize {
            get { return headersize; }
            set { headersize = value; }
        }

        public uint Fddsize {
            get { return fddsize; }
            set { fddsize = value; }
        }

        public uint Sectorsize {
            get { return sectorsize; }
            set { sectorsize = value; }
        }

        public uint Sectors {
            get { return sectors; }
            set { sectors = value; }
        }

        public uint Surfaces {
            get { return surfaces; }
            set { surfaces = value; }
        }

        public uint Cylinders {
            get { return cylinders; }
            set { cylinders = value; }
        }

        private static uint ReadUint(byte[] buf, int offset) {
            return (uint)(buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16) | (buf[offset + 3] << 24));
        }

        internal static DiskHeader Read(Stream s) {
            var buf = new byte[4 * 8];
            s.Read(buf, 0, buf.Length);
            var header = new DiskHeader();
            var i = 0;
            header.Dummy = ReadUint(buf, i);
            i += 4;
            header.Fddtype = ReadUint(buf, i);
            i += 4;
            header.Headersize = ReadUint(buf, i);
            i += 4;
            header.Fddsize = ReadUint(buf, i);
            i += 4;
            header.Sectorsize = ReadUint(buf, i);
            i += 4;
            header.Sectors = ReadUint(buf, i);
            i += 4;
            header.Surfaces = ReadUint(buf, i);
            i += 4;
            header.Cylinders = ReadUint(buf, i);
            return header;
        }
    }
}
