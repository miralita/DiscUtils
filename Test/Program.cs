using System;
using System.Collections.Generic;
using System.IO;
using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Partitions;
using DiscUtils.Hdi;

namespace Test {
    internal class Program {
        public static void Main(string[] args) {
            //Test3(args.Length > 0 ? args[0] : "");
            //Test3(@"D:\Translations\DK4\DK4.hdi");
            Test4();
        }

        public static void List(string filename) {
            using (var disk = new Disk(filename)) {
                Console.WriteLine($@"~~~~~~~~ {disk.PartitionInfo.FsType} ~~~~~~~~");
                using (var fs = new FatFileSystem(disk.Content, disk.Header.Sectorsize)) {
                    walkDir(fs, @"\", 0);
                }
            }
        }

        private static void walkDir(FatFileSystem fs, string dirname, int level) {
            var prefix = new string(' ', level * 4);
            try {
                var dirs = fs.GetDirectories(dirname);
                Array.Sort(dirs);
                foreach (var dir in dirs) {
                    Console.WriteLine(prefix + dir);
                    try {
                        if (!fs.Exists(dir)) {
                            Console.WriteLine("Not found");
                            continue;
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("Wrong filename");
                        continue;
                    }
                    try {
                        walkDir(fs, dir, level + 1);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            var files = fs.GetFiles(dirname);
            Array.Sort(files);
            foreach (var file in files) {
                var length = fs.GetFileLength(file) / 1024.0;
                Console.WriteLine(prefix + file + $@" ({length:F2} Kb)");
            }
        }

        public static void Test4() {
            var dir = @"S:\Translations\HDI";
            var files = Directory.GetFiles(dir);
            Array.Sort(files);
            foreach (var file in files) {
                if (!file.ToLower().EndsWith("hdi")) continue;
                Console.WriteLine($@"=========== {file} ===========");
                if (file.Contains("Appareden") || file.Contains("zai.hdi")) {
                    //Console.WriteLine("Skip");
                    //continue;
                }
                List(file);
            }
        }

        public static void Test3(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                Console.WriteLine("Usage: Test.exe filename.hdi");
                return;
            }
            List(filename);
        }

        public static void Test2() {
            var dir = @"S:\Translations\HDI";
            var files = Directory.GetFiles(dir);
            Array.Sort(files);
            foreach (var file in files) {
                if (! file.ToLower().EndsWith("hdi")) continue;
                Console.WriteLine(new FileInfo(file).Name);
                var disk = new Disk(file);
                var header = disk.Header;
                Console.WriteLine($@"	Type: {header.Hddtype}, Size: {header.Hddsize}, SectorSize: 0x{header.Sectorsize:X2}, Sectors: {header.Sectors}, Surfaces: {header.Surfaces}, Cylinders: {header.Cylinders}");
                var part = disk.PartitionInfo;
                Console.WriteLine($@"	Bootable: {part.Bootable}, PartType: {part.PartitionType}, Active: {part.Active}, FSType: {part.FsType}, IPL section: 0x{part.IplSect:X2}, IPL Head: 0x{part.IplHead:X2}, IPL Cyl: 0x{part.IplCyl:X4}, Sector: 0x{part.Sector:X2}, Head: 0x{part.Head:X2}, Cylinder: 0x{part.Cylinder:X4}, EndSector: 0x{part.EndSector:X2}, EndHead: 0x{part.EndHead:X2}, EndCyl: 0x{part.EndCyl:X4}, Name: {part.Name}");
                Console.WriteLine($@"   Partition offset: 0x{disk.PartitionOffset:X6}");
            }
        }

        public static void Test1() {
            //var disk = new Disk(@"D:\Translations\DK4\DK4.hdi");

            //var content = disk.Content;
            var content = File.Open(@"D:\Translations\DK4\DK4.hdi", FileMode.Open);
            content.Seek(0, SeekOrigin.Begin);
            content.Position = 0;
            var limit = content.Length - content.Position;
            for (var i = 0; i < limit; i++) {
                var offset = content.Position;
                //Console.WriteLine(offset);
                if (FatFileSystem.Detect(content)) {
                    Console.WriteLine("Found at {0}", i);
                    //break;
                }
                content.Seek(offset + 1, SeekOrigin.Begin);
                content.Position = offset + 1;
            }
            /*var fs = new FatFileSystem(disk.Content);
            var files = fs.GetFiles(@"\");
            foreach (var file in files) {
                Console.WriteLine(file);
            }*/

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}