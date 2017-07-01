using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Partitions;
using DiscUtils.Hdi;

namespace Test {
    internal class Program {
        private static FileSystemParameters parameters;
        private static string exportPath = @"D:\Translations\HDI\Export";
        public static void Main(string[] args) {
            parameters = new FileSystemParameters();
            parameters.FileNameEncoding = Encoding.GetEncoding("shift-jis");
            //Test3(args.Length > 0 ? args[0] : "");
            //Test3(@"D:\Translations\DK4\DK4.hdi");
            //Test3(@"S:\Translations\HDI\Guardian Recall.hdi");
            //Test3();
            var file = "";
            file = @"S:\Translations\HDI\Star Striders (J).HDI";
            file = @"S:\Translations\HDI\MacrossCompilation.hdi";
            file = @"S:\Translations\HDI\DK4.hdi";
            file = @"S:\Translations\HDI\Harlem Blade (1996)(Giga).hdi";
            Test3(file);
            //Test4();
            //Test5();
            //Test6();
            //ExportFile(file);
            //TestPartition(file);
        }

        private static void TestPartition(string file) {
            using (var disk = new Disk(file)) {
                PartitionTable.IsPartitioned(disk.Content);
                var pt = PartitionTable.GetPartitionTables(disk);
                var p = pt[0].Partitions[0];
                var fs = new FatFileSystem(p.Open());
                var files = fs.GetFiles(@"\");
                walkDir(fs, @"\", 0);
            }
        }

        public static void Test6() {
            var dir = @"S:\Translations\HDI";
            var files = Directory.GetFiles(dir);
            Array.Sort(files);
            foreach (var file in files) {
                if (!file.ToLower().EndsWith("hdi")) continue;
                if (file.Contains("Appareden") || file.Contains("zai.hdi")) {
                    //Console.WriteLine("Skip");
                    //continue;
                }
                ListCheck(file);
            }
        }

        public static void ExportFile(string filename) {
            Console.WriteLine($@"===================== {filename} ===================");
            using (var disk = new Disk(filename)) {
                using (var fs = new FatFileSystem(PartitionTable.GetPartitionTables(disk)[0].Partitions[0].Open())) {
                    var path = Path.Combine(exportPath, new FileInfo(filename).Name.Replace(".hdi", ""));
                    ExportFs(fs, @"\", path);
                }
            }
        }

        private static void ExportFs(FatFileSystem fs, string dirname, string path) {
            Directory.CreateDirectory(path);
            var dirs = fs.GetDirectories(dirname);
            Array.Sort(dirs);
            foreach (var dir in dirs) {
                Console.WriteLine(dir);
                var newpath = Path.Combine(path, new FileInfo(dir).Name);
                ExportFs(fs, dir, newpath);
            }
            var files = fs.GetFiles(dirname);
            Array.Sort(files);
            foreach (var file in files) {
                var length = fs.GetFileLength(file) / 1024.0;
                Console.WriteLine(file + $@" ({length:F2} Kb)");
                var exportPath = Path.Combine(path, new FileInfo(file).Name);
                using (var outfh = File.OpenWrite(exportPath)) {
                    using (var infh = fs.OpenFile(file, FileMode.Open)) {
                        var buf = new byte[1024];
                        var n = 0;
                        do {
                            n = infh.Read(buf, 0, 1024);
                            outfh.Write(buf, 0, n);
                        } while (n == 1024);
                    }
                }
            }
        }

        public static void ListCheck(string filename) {
            Console.WriteLine($@"===================== {filename} ===================");
            using (var disk = new Disk(filename)) {
                var header = disk.Header;
                Console.WriteLine(
                    $@"	Type: {header.Hddtype}, Size: {header.Hddsize}, SectorSize: 0x{header.Sectorsize:X2}, Sectors: {
                            header.Sectors
                        }, Surfaces: {header.Surfaces}, Cylinders: {header.Cylinders}");
                var part = disk.PartitionInfo;
                Console.WriteLine(
                    $@"	Bootable: {part.Bootable}, PartType: {part.PartitionType}, Active: {part.Active}, FSType: {
                            part.FsType
                        }, IPL section: 0x{part.IplSect:X2}, IPL Head: 0x{part.IplHead:X2}, IPL Cyl: 0x{
                            part.IplCyl
                        :X4}, Sector: 0x{part.Sector:X2}, Head: 0x{part.Head:X2}, Cylinder: 0x{
                            part.Cylinder
                        :X4}, EndSector: 0x{part.EndSector:X2}, EndHead: 0x{part.EndHead:X2}, EndCyl: 0x{
                            part.EndCyl
                        :X4}, Name: {part.Name}");
                Console.WriteLine($@"   Partition offset: 0x{disk.PartitionOffset:X6}");
                //using (var fs = new FatFileSystem(disk.Content, disk.Header.Sectorsize)) {
                using (var fs = new FatFileSystem(PartitionTable.GetPartitionTables(disk)[0].Partitions[0].Open())) {
                    try {
                        tryWalkDir(fs, @"\", null);
                        Console.WriteLine($@"OK");
                    } catch (Exception ex) {
                        Console.WriteLine($@"{ex.Message}");
                    }
                }
            }
        }

        private static List<string> tryWalkDir(FatFileSystem fs, string dirname, List<string> fileList) {
            if (fileList == null) {
                fileList = new List<string>();
            }
            var dirs = fs.GetDirectories(dirname);
            Array.Sort(dirs);
            foreach (var dir in dirs) {
                fileList.Add(dir);
                tryWalkDir(fs, dir, fileList);
            }
            var files = fs.GetFiles(dirname);
            Array.Sort(files);
            foreach (var file in files) {
                fileList.Add(file);
                var length = fs.GetFileLength(file) / 1024.0;
                //Console.WriteLine(prefix + file + $@" ({length:F2} Kb)");
            }
            return fileList;
        }

        public static void List(string filename) {
            Console.WriteLine($@"===================== {filename} ===================");
            using (var disk = new Disk(filename)) {
                var header = disk.Header;
                Console.WriteLine($@"	Type: {header.Hddtype}, Size: {header.Hddsize}, SectorSize: 0x{header.Sectorsize:X2}, Sectors: {header.Sectors}, Surfaces: {header.Surfaces}, Cylinders: {header.Cylinders}");
                var part = disk.PartitionInfo;
                Console.WriteLine($@"	Bootable: {part.Bootable}, PartType: {part.PartitionType}, Active: {part.Active}, FSType: {part.FsType}, IPL section: 0x{part.IplSect:X2}, IPL Head: 0x{part.IplHead:X2}, IPL Cyl: 0x{part.IplCyl:X4}, Sector: 0x{part.Sector:X2}, Head: 0x{part.Head:X2}, Cylinder: 0x{part.Cylinder:X4}, EndSector: 0x{part.EndSector:X2}, EndHead: 0x{part.EndHead:X2}, EndCyl: 0x{part.EndCyl:X4}, Name: {part.Name}");
                Console.WriteLine($@"   Partition offset: 0x{disk.PartitionOffset:X6}");
                //using (var fs = new FatFileSystem(disk.Content, disk.Header.Sectorsize)) {
                using (var fs = new FatFileSystem(PartitionTable.GetPartitionTables(disk)[0].Partitions[0].Open())) {
                    walkDir(fs, @"\", 0);
                    //tryWalkDir(fs, @"\", null);
                }
            }
        }

        private static void Test5() {
            var files = new string[] {
                @"S:\Translations\HDI\Harlem Blade (1996)(Giga).hdi",
                @"S:\Translations\HDI\Isaku (1995)(Elf).hdi",
                @"S:\Translations\HDI\Strange World (J).hdi",
                @"S:\Translations\HDI\VirginAngel.hdi",
};
            foreach (var file in files) {
                List(file);
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
                        Console.WriteLine("Wrong dirname");
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
                var info = fs.GetFileInfo(file);
                Console.WriteLine(prefix + file + $@" ({length:F2} Kb) {info.CreationTime} {info.LastAccessTime} {info.LastWriteTime}");
            }
        }

        public static void Test4() {
            var dir = @"S:\Translations\HDI";
            var files = Directory.GetFiles(dir);
            Array.Sort(files);
            foreach (var file in files) {
                if (!file.ToLower().EndsWith("hdi")) continue;
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