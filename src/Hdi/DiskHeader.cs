using System;
using System.IO;

namespace DiscUtils.Hdi {
    public class DiskHeader {
        /*typedef struct {
	UINT8	dummy[4];
	UINT8	hddtype[4];
	UINT8	headersize[4];
	UINT8	hddsize[4];
	UINT8	sectorsize[4];
	UINT8	sectors[4];
	UINT8	surfaces[4];
	UINT8	cylinders[4];
} HDIHDR;*/
        public static int Size => 8 * 4;
        private uint dummy;

        private uint hddtype;
        private uint headersize;
        private uint hddsize;
        private uint sectorsize;
        private uint sectors;
        private uint surfaces;
        private uint cylinders;

        public DiskHeader(HddType capacity) {
            headersize = 4096;
            sectorsize = 256;
            sectors = 33;
            switch (capacity) {
                case HddType.Size5Mb:
                    hddtype = 0;
                    surfaces = 4;
                    cylinders = 153;
                    break;
                case HddType.Size10Mb:
                    hddtype = 1;
                    surfaces = 4;
                    cylinders = 310;
                    break;
                case HddType.Size15Mb:
                    hddtype = 2;
                    surfaces = 6;
                    cylinders = 310;
                    break;
                case HddType.Size20Mb:
                    hddtype = 3;
                    surfaces = 8;
                    cylinders = 615;
                    break;
                case HddType.Size30Mb:
                    hddtype = 5;
                    surfaces = 6;
                    cylinders = 615;
                    break;
                case HddType.Size40Mb:
                    hddtype = 6;
                    surfaces = 8;
                    cylinders = 615;
                    break;
            }
            hddsize = sectors * surfaces * cylinders;
        }

        public uint Dummy {
            get { return dummy; }
            set { dummy = value; }
        }

        public uint Hddtype {
            get { return hddtype; }
            set { hddtype = value; }
        }

        public uint Headersize {
            get { return headersize; }
            set { headersize = value; }
        }

        public uint Hddsize {
            get { return hddsize; }
            set { hddsize = value; }
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
/*#ifndef STOREINTELDWORD
#define	STOREINTELDWORD(a, b)	*((a)+0) = (UINT8)((b));		\
								*((a)+1) = (UINT8)((b)>>8);		\
								*((a)+2) = (UINT8)((b)>>16);	\
								*((a)+3) = (UINT8)((b)>>24)
*/

        /*#ifndef LOADINTELDWORD
#define	LOADINTELDWORD(a)		(((UINT32)(a)[0]) |				\
								((UINT32)(a)[1] << 8) |			\
								((UINT32)(a)[2] << 16) |		\
								((UINT32)(a)[3] << 24))
*/
        internal static DiskHeader Read(Stream s) {
            var buf = new byte[4 * 8];
            s.Read(buf, 0, buf.Length);
            var header = new DiskHeader();
            var i = 0;
            header.Dummy = ReadUint(buf, i);
            i += 4;
            header.Hddtype = ReadUint(buf, i);
            i += 4;
            header.Headersize = ReadUint(buf, i);
            i += 4;
            header.Hddsize = ReadUint(buf, i);
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

        public DiskHeader() {
        }

        private static uint ReadUint(byte[] buf, int offset) {
            return (uint)(buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16) | (buf[offset + 3] << 24));
        }

        internal void Write(Stream s) {
            var buf = new byte[4 * 8];
            WriteUint(Dummy, buf, 0);
            WriteUint(Hddsize, buf, 4);
            WriteUint(Headersize, buf, 8);
            WriteUint(Hddsize, buf, 12);
            WriteUint(Sectorsize, buf, 16);
            WriteUint(Sectors, buf, 20);
            WriteUint(Surfaces, buf, 24);
            WriteUint(Cylinders, buf, 28);
        }

        private static void WriteUint(uint val, byte[] buf, int offset) {
            buf[offset++] = (byte)(val & 0x000000ff);
            buf[offset++] = (byte) ((val & 0x0000ff00) >> 8);
            buf[offset++] = (byte) ((val & 0x00ff0000) >> 16);
            buf[offset] = (byte) ((val & 0xff000000) >> 24);
        }
    }
}
