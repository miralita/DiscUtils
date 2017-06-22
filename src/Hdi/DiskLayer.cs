using System;
using System.IO;

namespace DiscUtils.Hdi {
    public sealed class DiskLayer :VirtualDiskLayer {
        private Geometry _geometry;
        private long _capacity;
        private FileLocator _relativeFileLocator;
        private DiskHeader _header;
        private PC98Partition _partitionInfo;
        private Stream _stream;
        private Ownership _ownStream;
        private const int blockOffset = 4096;
        private uint partitionOffset;

        public override Geometry Geometry => Geometry.FromCapacity(_header.Hddsize, (int)_header.Sectorsize);

        public override bool IsSparse => false;

        public override bool NeedsParent => false;

        internal override long Capacity => _header.Hddsize;

        internal override FileLocator RelativeFileLocator => null;

        public DiskHeader Header => _header;

        public PC98Partition PartitionInfo => _partitionInfo;

        public uint PartitionOffset => partitionOffset;

        public override SparseStream OpenContent(SparseStream parent, Ownership ownsParent) {
            var s = new DiskStream(_stream, partitionOffset);
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
            _partitionInfo = new PC98Partition(stream);
            partitionOffset = CalcPartitionOffset() + blockOffset;
        }

        public DiskLayer(Stream stream, Ownership ownStream) :this(stream){
            _ownStream = ownStream;
        }

        public DiskLayer(Stream stream, Ownership ownsStream, DiskHeader header) {
            stream.Position = 0;
            _ownStream = ownsStream;
            _header = header;
            var buf = new byte[4096];
            stream.Write(buf, 0, buf.Length);
            stream.Position = 0;
            header.Write(stream);
            stream.Position = buf.Length;
            fillImage(256, (int)_header.Hddsize);
            stream.SetLength(_header.Hddsize);
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
            return offset * _header.Sectorsize;
        }

        public static DiskLayer InitializeFixed(Stream stream, Ownership ownsStream, HddType capacity) {
            var header = new DiskHeader(capacity);
            return new DiskLayer(stream, ownsStream, header);
        }

        void fillImage(int ssize, int tsize) {
            var work = new byte[1024];
            var size = 0;
            Array.Copy(hdddiskboot, work, work.Length);
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

        private static readonly byte[] hdddiskboot = {
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

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) _stream.Dispose();
            } finally {
                base.Dispose(disposing);
            }
        }
    }
}
