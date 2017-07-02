using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Fat
{
    public class PC98FatFileSystem : FatFileSystem
    {
        public PC98FatFileSystem(Stream data) : base(data) {
        }

        public PC98FatFileSystem(Stream data, Ownership ownsData) : base(data, ownsData) {
        }

        public PC98FatFileSystem(Stream data, TimeConverter timeConverter) : base(data, timeConverter) {
        }

        public PC98FatFileSystem(Stream data, Ownership ownsData, TimeConverter timeConverter) : base(data, ownsData, timeConverter) {
        }

        public PC98FatFileSystem(Stream data, Ownership ownsData, FileSystemParameters parameters) : base(data, ownsData, parameters) {
        }

        public PC98FatFileSystem(Stream data, FileSystemParameters parameters) : base(data, parameters) {
        }

        protected override void DetectEncoding(Stream data) {
            var encoding = (data as SparseStream)?.FileNameEncoding;
            if (encoding == null) {
                encoding = (data as SubStream)?.FileNameEncoding;
            }
            if (encoding == null) {
                encoding = Encoding.GetEncoding("shift-jis");
            }
            var opts = this.Options as FatFileSystemOptions;
            if (opts != null) {
                opts.FileNameEncoding = encoding;
            }
        }

        public static bool Detect(Stream stream)
        {
            if (stream.Length < 512)
            {
                return false;
            }

            stream.Position = 0;
            byte[] bytes = Utilities.ReadFully(stream, 512);
            ushort bpbBytesPerSec = Utilities.ToUInt16LittleEndian(bytes, 11);
            if (bpbBytesPerSec != 256 && bpbBytesPerSec != 512 && bpbBytesPerSec != 1024 && bpbBytesPerSec != 2048)
            {
                return false;
            }

            byte bpbNumFATs = bytes[16];
            if (bpbNumFATs == 0 || bpbNumFATs > 2)
            {
                return false;
            }

            ushort bpbTotSec16 = Utilities.ToUInt16LittleEndian(bytes, 19);
            uint bpbTotSec32 = Utilities.ToUInt32LittleEndian(bytes, 32);

            if ((bpbTotSec16 == 0) || (bpbTotSec32 == 0))
            {
                // SKIP - this is condition for ordinary FAT, don't duplicate FSs!
                return false;
            }

            uint totalSectors = bpbTotSec16 + bpbTotSec32;
            if (bpbTotSec16 > 0 && bpbTotSec32 > 0) {
                totalSectors = bpbTotSec16;
            }
            return totalSectors * (long)bpbBytesPerSec <= stream.Length;
        }
    }
}
