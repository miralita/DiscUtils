using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Partitions
{
    public enum PartType : uint{
        N88_PCUX = 0,
        PCUX = 4,
        N88_1 = 6,
        N88_2 = 0x10,
        BSD = 0x14,
        DOS_Data = 0x20,
        DOS = 0x21,
        Minix = 0x40,
    }

    public enum FSType : uint {
        FAT12 = 0x01,
        FAT16 = 0x11,
        FAT16Large = 0x21,
        NTFS = 0x31,
        NT1 = 0x28,
        NT2 = 0x41,
        NT3 = 0x48,
        FAT32 = 0x61,
        PCUX = 0x04,
        N88 = 0x06,
        BSD = 0x44,
        EXT2 = 0x62,
    }

    public class PC98PartitionRecord : IComparable<PC98PartitionRecord>
    {
        private byte mid;
        private bool bootable;
        private PartType partType;
        private Stream _stream;

        private byte sid;
        private bool active;
        private FSType fsType;

        /*private byte dum1;
        private byte dum2;*/
        private byte ipl_sect;
        private byte ipl_head;
        private ushort ipl_cyl;
        private byte sector;
        private byte head;
        private ushort cyl;
        private byte end_sector;
        private byte end_head;
        private ushort end_cyl;
        private string name;

        public PC98PartitionRecord(Stream s) {
            _stream = s;
            Init();
        }

        private void Init() {
            var buf = new byte[32];
            _stream.Read(buf, 0, buf.Length);
            var i = 0;
            mid = buf[i++];
            bootable = (mid & 0x80) > 0;
            var m = (byte) (mid & 0x7f);
            if (m >= 0x21 && m <= 0x2f) {
                partType = PartType.DOS;
            } else {
                partType = (PartType) m;
            }

            sid = buf[i++];
            active = (sid & 0x80) > 0;
            m = (byte) (sid & 0x7f);
            fsType = (FSType) m;


            i += 2;
            ipl_sect = buf[i++];
            ipl_head = buf[i++];
            ipl_cyl = (ushort) (buf[i] | (buf[i + 1] << 8));
            i += 2;
            sector = buf[i++];
            head = buf[i++];
            cyl = (ushort) (buf[i] | (buf[i + 1] << 8));
            i += 2;
            end_sector = buf[i++];
            end_head = buf[i++];
            end_cyl = (ushort) (buf[i] | (buf[i + 1] << 8));
            i += 2;
            name = Encoding.ASCII.GetString(buf, i, 16);
        }

        public PC98PartitionRecord(Stream s, int position) {
            s.Position = position;
            _stream = s;
            Init();
        }

        public bool Bootable {
            get { return bootable; }
        }

        public PartType PartitionType {
            get { return partType; }
        }

        public bool Active {
            get { return active; }
        }

        public FSType FsType {
            get { return fsType; }
        }

        public byte IplSect {
            get { return ipl_sect; }
        }

        public byte IplHead {
            get { return ipl_head; }
        }

        public ushort IplCyl {
            get { return ipl_cyl; }
        }

        public byte Sector {
            get { return sector; }
        }

        public byte Head {
            get { return head; }
        }

        public ushort Cylinder {
            get { return cyl; }
        }

        public byte EndSector {
            get { return end_sector; }
        }

        public byte EndHead {
            get { return end_head; }
        }

        public ushort EndCyl {
            get { return end_cyl; }
        }

        public string Name {
            get { return name; }
        }

        public int CompareTo(PC98PartitionRecord other) {
            throw new NotImplementedException();
        }
    }
}
