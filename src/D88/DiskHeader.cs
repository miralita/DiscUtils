using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.D88 {
    /*typedef struct {
	UINT8	fd_name[17];		// Disk Name
	UINT8	reserved1[9]; 		// Reserved
	UINT8	protect;			// Write Protect bit:4
	UINT8	fd_type;			// Disk Format
	UINT8	fd_size[4];			// Disk Size
	UINT8	trackp[D88_TRACKMAX][4];
} _D88HEAD, *D88HEAD;

// D88セクタ (size: 16bytes)
typedef struct {
	UINT8	c;
	UINT8	h;
	UINT8	r;
	UINT8	n;
	UINT8	sectors[2];			// Sector Count
	UINT8	mfm_flg;			// sides
	UINT8	del_flg;			// DELETED DATA
	UINT8	stat;				// STATUS (FDC ret)
	UINT8	seektime;			// Seek Time
	UINT8	reserved[3];		// Reserved
	UINT8	rpm_flg;			// rpm			0:1.2  1:1.44
	UINT8	size[2];			// Sector Size
} _D88SEC, *D88SEC;*/
    public class DiskHeader {
        private byte[] fd_name = new byte[17];
        private string _fdName;
        private byte[] reserved1 = new byte[9];
        private byte protect;
        private byte fd_type;
        private uint fd_size;

        public uint Fddsize => fd_size - 0x2b0;
        public uint Offset => 0x2b0;

        private static uint ReadUint(byte[] buf, int offset) {
            return (uint)(buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16) | (buf[offset + 3] << 24));
        }

        internal static DiskHeader Read(Stream s) {
            var buf = new byte[32];
            s.Read(buf, 0, buf.Length);
            var header = new DiskHeader();
            Array.Copy(buf, 0, header.fd_name, 0, 17);
            header._fdName = Encoding.ASCII.GetString(buf, 0, 17);
            Array.Copy(buf, 17, header.reserved1, 0, 9);
            var i = 17+9;
            header.protect = buf[i++];
            header.fd_type = buf[i++];
            header.fd_size = ReadUint(buf, i);
            return header;
        }
    }
}