using System;
using System.Collections.Generic;
using System.IO;
using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Partitions;
using DiscUtils.Vhd;

namespace Test {
    internal class Program {
        public static void Main(string[] args) {
            long diskSize = 30 * 1024 * 1024; //30MB
            
            using (Stream vhdStream = File.Create(@"C:\TEMP\mydisk.vhd"))
            {
                Disk disk = Disk.InitializeDynamic(vhdStream,Ownership.None, diskSize);
                BiosPartitionTable.Initialize(disk, WellKnownPartitionType.WindowsFat);
                using (FatFileSystem fs = FatFileSystem.FormatPartition(disk, 0, null))
                {
                    fs.CreateDirectory(@"TestDir\CHILD");
                    // do other things with the file system...
                }
            }
        }
    }
}