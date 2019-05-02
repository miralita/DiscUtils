using System;
using System.IO;
using System.Text;
using DiscUtils.Partitions;

namespace DiscUtils.Hdi {
    public sealed class DiskLayer :VirtualDiskLayer {
        private Geometry _geometry;
        private long _capacity;
        private FileLocator _relativeFileLocator;
        private DiskHeader _header;
        private PC98PartitionRecord _partitionInfo;
        private Stream _stream;
        private Ownership _ownStream;
        private const int blockOffset = 4096;
        private uint partitionOffset;
        private uint bytesPerBlock;

        //public override Geometry Geometry => Geometry.FromCapacity(_header.Hddsize, (int)_header.Sectorsize);
        //public Geometry(int cylinders, int headsPerCylinder, int sectorsPerTrack, int bytesPerSector)
        public override Geometry Geometry => new Geometry((int)_header.Cylinders, (int)_header.Surfaces, (int)_header.Sectors, (int)_header.Sectorsize);

        public override bool IsSparse => false;

        public override bool NeedsParent => false;

        internal override long Capacity => _header.Hddsize;

        internal override FileLocator RelativeFileLocator => null;

        public DiskHeader Header => _header;

        public PC98PartitionRecord PartitionInfo => _partitionInfo;

        public uint PartitionOffset => partitionOffset;

        public uint BytesPerBlock {
            get { return bytesPerBlock; }
        }

        public override SparseStream OpenContent(SparseStream parent, Ownership ownsParent) {
            //var s = new DiskStream(_stream, partitionOffset);
            var s = new DiskStream(_stream, blockOffset);
            return s;
        }

        public override string[] GetParentLocations() {
            throw new NotImplementedException();
        }

        public DiskLayer(Stream stream) {
            _stream = stream;
            _header = DiskHeader.Read(stream);
            // skip bootloader
            stream.Position = blockOffset + _header.Sectorsize;
            _partitionInfo = new PC98PartitionRecord(stream);
            partitionOffset = CalcPartitionOffset() + blockOffset;
        }

        public DiskLayer(Stream stream, Ownership ownStream) :this(stream) {
            _stream = stream;
            _ownStream = ownStream;
        }

        public DiskLayer(Stream stream, Ownership ownsStream, DiskHeader header) {
            stream.Position = 0;
            _stream = stream;
            _ownStream = ownsStream;
            _header = header;
            var buf = new byte[4096];
            stream.Write(buf, 0, buf.Length);
            stream.Position = 0;
            header.Write(stream);
            stream.Position = buf.Length;
            fillImage(256, (int)_header.Hddsize);
            stream.SetLength(_header.Hddsize + buf.Length);
            stream.Position = _header.Hddsize + buf.Length - 1;
            stream.WriteByte(0);
            stream.Flush();
        }

        /*
         * Всего секторов = C\ * H  * S
LBA=c * H * S + h * S + s - 1 = (c * H + h) * S + s - 1
LBA: Адрес блока по LBA (Logical block addressing)
C: Количество цилиндров
c: Номер цилиндра
H: Количество головок
h: Номер выбранной головки
S: Количество секторов в одном треке
s: Номер сектора*/
        private uint CalcPartitionOffset() {
            var cylinders = _header.Cylinders;
            var cylinder = _partitionInfo.Cylinder;
            var heads = _header.Surfaces;
            var head = _partitionInfo.Head;
            var sectors = _header.Sectors;
            var sector = _partitionInfo.Sector;
            //var offset = (cylinder * heads + head) * sectors + sector - 1;
            var offset = (cylinder * heads + head) * sectors + sector;
            var partitionOffset = offset * _header.Sectorsize;

            var pos = _stream.Position;
            _stream.Position = partitionOffset + blockOffset + 3;
            var buf = new byte[8];
            _stream.Read(buf, 0, 8);
            //Console.Write($@"Descr: {Encoding.ASCII.GetString(buf)} ");
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            var val = (b2 << 8) | b1;
            bytesPerBlock = (uint)val;
            //Console.Write($@"Bytes per block: {val:X4} ");
            b1 = _stream.ReadByte();
            //Console.Write($@"Blocks per unit: {b1:X2} ");
            b1 = _stream.ReadByte();
            b2 = _stream.ReadByte();
            val = (b2 << 8) | b1;
            //Console.Write($@"Reserved: {val:X4} ");
            b1 = _stream.ReadByte();
            //Console.Write($@"FATs: {b1:X2} ");
            b1 = _stream.ReadByte();
            b2 = _stream.ReadByte();
            val = (b2 << 8) | b1;
            //Console.Write($@"Root entries: {val:X4} ");
            b1 = _stream.ReadByte();
            b2 = _stream.ReadByte();
            val = (b2 << 8) | b1;
            //Console.Write($@"Total blocks: {val:X4} ");
            b1 = _stream.ReadByte();
            //Console.Write($@"Media: {b1:X2} ");
            b1 = _stream.ReadByte();
            b2 = _stream.ReadByte();
            val = (b2 << 8) | b1;
            //Console.Write($@"FAT size: {val:X4} ");
            b1 = _stream.ReadByte();
            b2 = _stream.ReadByte();
            val = (b2 << 8) | b1;
            //Console.Write($@"Blocks per track: {val:X4} ");
            b1 = _stream.ReadByte();
            b2 = _stream.ReadByte();
            val = (b2 << 8) | b1;
            //Console.WriteLine($@"Heads: {val:X4} ");

            _stream.Position = pos;
            return partitionOffset;
        }

        public static DiskLayer InitializeFixed(Stream stream, Ownership ownsStream, HddType capacity) {
            var header = new DiskHeader(capacity);
            return new DiskLayer(stream, ownsStream, header);
        }

        void fillImage(int ssize, int tsize) {
            var work = new byte[1024];
            var size = 0;
            Array.Copy(hdddiskboot, work, hdddiskboot.Length);
            if (ssize < 1024) {
                work[ssize - 2] = 0x55;
                work[ssize - 1] = 0xaa;
            }
            _stream.Write(work, 0, work.Length);
            if (tsize > work.Length) {
                tsize -= work.Length;
                for (var i = 0; i < work.Length; i++) {
                    work[i] = 0;
                }
                while (tsize > 0) {
                    size = tsize > work.Length ? work.Length : tsize;
                    tsize -= size;
                    _stream.Write(work, 0, size);
                }
            }
        }

        private static readonly byte[] hdddiskboot1 = {
            0xeb,0x0a,0x90,0x90,0x49,0x50,0x4c,0x31,0x00,0x00,0x00,0x1e,
            0xb8,0x04,0x0a,0xcd,0x18,0xb4,0x16,0xba,0x20,0xe1,0xcd,0x18,
            0xfa,0xfc,0xb8,0x00,0xa0,0x8e,0xc0,0xbe,0x3c,0x00,0x31,0xff,
            0xe8,0x09,0x00,0xbf,0xa0,0x00,0xe8,0x03,0x00,0xf4,0xeb,0xfd,
            0x2e,0xad,0x85,0xc0,0x74,0x05,0xab,0x47,0x47,0xeb,0xf5,0xc3,
            0x04,0x33,0x04,0x4e,0x05,0x4f,0x01,0x3c,0x05,0x49,0x05,0x47,
            0x05,0x23,0x05,0x39,0x05,0x2f,0x05,0x24,0x05,0x61,0x01,0x3c,
            0x05,0x38,0x04,0x4f,0x05,0x55,0x05,0x29,0x01,0x3c,0x05,0x5e,
            0x05,0x43,0x05,0x48,0x04,0x35,0x04,0x6c,0x04,0x46,0x04,0x24,
            0x04,0x5e,0x04,0x3b,0x04,0x73,0x01,0x25,0x00,0x00,0x05,0x47,
            0x05,0x23,0x05,0x39,0x05,0x2f,0x05,0x24,0x05,0x61,0x01,0x3c,
            0x05,0x38,0x04,0x72,0x21,0x5e,0x26,0x7e,0x18,0x65,0x01,0x24,
            0x05,0x6a,0x05,0x3b,0x05,0x43,0x05,0x48,0x04,0x37,0x04,0x46,
            0x12,0x3c,0x04,0x35,0x04,0x24,0x01,0x25,0x00,0x00
        };

        private static readonly byte[] hdddiskboot = {
            0xEB, 0x0A, 0x90, 0x90, 0x49, 0x50, 0x4C, 0x31,
            0x00, 0x00, 0x00, 0x1E, 0xA0, 0x84, 0x05, 0xB4,
            0x8E, 0xCD, 0x1B, 0xA8, 0x20, 0x74, 0x22, 0x32,
            0xDB, 0xB4, 0x14, 0xCD, 0x1B, 0x72, 0x1A, 0x80,
            0xFB, 0x84, 0x75, 0x15, 0xE8, 0x96, 0x00, 0x73,
            0x03, 0xEB, 0x6B, 0x90, 0xB4, 0x24, 0xBB, 0x00,
            0x04, 0xB9, 0x30, 0x12, 0xBA, 0x40, 0x01, 0xCD,
            0x1B, 0xBB, 0x00, 0x01, 0xB4, 0x84, 0xCD, 0x1B,
            0xB4, 0x06, 0x33, 0xC9, 0x33, 0xD2, 0x50, 0x8C,
            0xC8, 0x2D, 0xC0, 0x03, 0x8E, 0xC0, 0x58, 0x33,
            0xED, 0xCD, 0x1B, 0x72, 0x41, 0xB4, 0x06, 0xBA,
            0x01, 0x00, 0x81, 0xC5, 0x00, 0x08, 0xCD, 0x1B,
            0x72, 0x34, 0xBA, 0x04, 0x00, 0xF7, 0xC3, 0x00, 0xAA, 0x74, 0x03, 0xBA,
            0x02, 0x00, 0xB4, 0x06, 0xBB, 0x00, 0x1C, 0x81, 0xC5, 0x00, 0x08, 0xCD, 0x1B, 0x72, 0x1B, 0x50, 0x8B, 0xC5,
            0xB1, 0x04, 0xD3, 0xE8, 0x8C, 0xC1, 0x03, 0xC1, 0x8B, 0xF0, 0x58, 0xE8, 0x15, 0x00, 0x2E, 0x89, 0x36, 0x0A,
            0x00, 0x2E, 0xFF, 0x1E, 0x08, 0x00, 0xE8, 0x08, 0x00, 0xB4, 0x0E, 0xCD, 0x1B, 0xB9, 0x01, 0x00, 0xCB, 0x56,
            0xA0, 0x84, 0x05, 0x32, 0xDB, 0xB4, 0x14, 0xCD, 0x1B, 0x72, 0x0E, 0x80, 0xFB, 0x84, 0x75, 0x09, 0x2E, 0xC6,
            0x06, 0xC6, 0x49, 0x00, 0xE8, 0x02, 0x00, 0x5E, 0xC3, 0xB4, 0xB0, 0xBE, 0xC2, 0x49, 0xBA, 0x06, 0x00, 0x1E,
            0x0E, 0x1F, 0xCD, 0x1B, 0xB4, 0xB0, 0xCD, 0x1B, 0x1F, 0xC3, 0x1E, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x09, 0x00, 0x55, 0xAA
        };

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) _stream.Dispose();
            } finally {
                base.Dispose(disposing);
            }
        }
    }
}
