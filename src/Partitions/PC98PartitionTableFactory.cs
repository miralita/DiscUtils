using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Partitions
{
    [PartitionTableFactory]
    internal sealed class PC98PartitionTableFactory : PartitionTableFactory {
        public override bool DetectIsPartitioned(Stream s) {
            return PC98PartitionTable.IsValid(s);
        }

        public override PartitionTable DetectPartitionTable(VirtualDisk disk) {
            if (PC98PartitionTable.IsValid(disk.Content, disk.SectorSize)) {
                var table = new PC98PartitionTable(disk);
                return table;
            } else {
                return null;
            }
        }
    }
}
