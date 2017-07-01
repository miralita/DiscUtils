using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscUtils.Partitions
{
    class PC98PartitionInfo : PartitionInfo
    {
        private PC98PartitionTable _table;
        private PC98PartitionRecord _rec;
        public long PartitionOffset { get; }
        public long PartitionEnd { get; }

        public PC98PartitionInfo(PC98PartitionTable table, PC98PartitionRecord rec) {
            this._table = table;
            this._rec = rec;
            var cylinder = _rec.Cylinder;
            var heads = _table.Surfaces;
            var head = _rec.Head;
            var sectors = _table.SectorsPerTrack;
            var sector = _rec.Sector;
            //var offset = (cylinder * heads + head) * sectors + sector - 1;
            var offset = (cylinder * heads + head) * sectors + sector;
            FirstSector = offset;
            PartitionOffset = offset * _table.Sectorsize;
            cylinder = _rec.EndCyl;
            head = _rec.EndHead;
            sector = _rec.EndSector;
            offset = (cylinder * heads + head) * sectors + sector;
            LastSector = offset;
            PartitionEnd = offset * _table.Sectorsize;
        }

        public override long FirstSector { get; }

        public override long LastSector { get; }
        public override Guid GuidType { get; }
        public override byte BiosType { get; }
        public override string TypeAsString { get; }
        internal override PhysicalVolumeType VolumeType { get; }
        public override SparseStream Open() {
            return _table.Open(this);
        }
    }
}
