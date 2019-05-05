using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private static string[] fileList1 = new string[] {
            @"D:\Translations\FDI\5x5go\lip_stick2_a.fdi",
            @"D:\Translations\FDI\5x5go\lip_stick2_b.fdi",
            @"D:\Translations\FDI\5x5go\madpa_5.fdi"
        };
        private static string[] fileList2 = new string[] {
            @"D:\Translations\FDI\5x5go\ccbsprta.fdi",
            @"D:\Translations\FDI\5x5go\circle_m_01.fdi",
            @"D:\Translations\FDI\5x5go\cpsycho_a.fdi",
            @"D:\Translations\FDI\5x5go\executioners_system.fdi",
            @"D:\Translations\FDI\5x5go\ftailk_1.fdi",
            @"D:\Translations\FDI\5x5go\kimidaia.fdi",
            @"D:\Translations\FDI\5x5go\ko1_a.fdi",
            @"D:\Translations\FDI\5x5go\kounai2_1.fdi",
            @"D:\Translations\FDI\5x5go\kounai_shasei_sp.fdi",
            @"D:\Translations\FDI\5x5go\madou monogatari - michikusa ibun (j).fdi",
            @"D:\Translations\FDI\5x5go\mai_a.fdi",
            @"D:\Translations\FDI\5x5go\mai_b.fdi",
            @"D:\Translations\FDI\5x5go\mai_c.fdi",
            @"D:\Translations\FDI\5x5go\marines - special disk (j).fdi",
            @"D:\Translations\FDI\5x5go\nooch2_1.fdi",
            @"D:\Translations\FDI\5x5go\salamander.fdi",
            @"D:\Translations\FDI\5x5go\saori_a.fdi",
            @"D:\Translations\FDI\5x5go\sekai_a.fdi",
            @"D:\Translations\FDI\5x5go\sschld4c.fdi",
            @"D:\Translations\FDI\5x5go\sshild3c.fdi",
            @"D:\Translations\FDI\5x5go\straw_c.fdi",
            @"D:\Translations\FDI\5x5go\themm_1.fdi",
            @"D:\Translations\FDI\5x5go\ulamander - 2.fdi",
            @"D:\Translations\FDI\5x5go\wiz98yutoufu4.fdi",
            @"D:\Translations\FDI\5x5go\xna_a.fdi",
            @"D:\Translations\FDI\5x5go\xna_b.fdi",
            @"D:\Translations\FDI\5x5go\zukan_a.fdi",
            @"D:\Translations\FDI\5x5go\zukan_a_alt.fdi",
            @"D:\Translations\FDI\5x5go\zukan_b.fdi"
        };
        private static FileSystemParameters parameters;
        private static string exportPath = @"H:\Translations\XDF\Export";
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
            //Test3(file);
            //Test4();
            //Test5();
            //Test6();
            //ExportFile(file);
            //TestPartition(file);
            //FDIInfo(@"D:\Translations\FDI\5x5go");
            //FDIChecker(@"D:\Translations\FDI\5x5go");
            //CheckFDI(@"H:\translations\Translations\Floppy\fdi\sstriders_01.fdi");
            //CheckFDI(@"H:\translations\Translations\Floppy\d88\04_disk.d88");
            //D88Checker();
            //CheckD88(@"S:\Translations\Patcher\Macross\sr-cp_1.d88");
            //FDIChecker1(fileList2);
            //Newdisk(@"D:\Translations\Tools\Patcher\images\test.hdi");
            file = @"D:\Translations\KOEI\Air Management 1 - Ozora ni Kakeru (J) A.FDI";
            //file = @"D:\Translations\RememberMe\mac1_1.fdi";
            //Tester(file);
            file = @"H:\translations\Translations\StarCruiserX68k\HDM_Disks\Original\disk1.xdf";
            file = @"H:\translations\Translations\StarCruiserX68k\HDM_Disks\Converted\disk1.DIM";
            XdfCheck(file);
            
            //var stream = new SubStream(disk.Content, 0x400, disk.Content.Length - 0x400);
            //var stream = new SubStream(disk.Content, 0x400, disk.Content.Length - 0x400);
            //var fat = new PC98FatFileSystem(stream);
            
        }

        private static void XdfCheck(string fname) {
            //var disk = new DiscUtils.Xdf.Disk(fname);
            var disk = new DiscUtils.Dim.Disk(fname);
            var param = new FileSystemParameters();
            //param.SectorSize = disk.SectorSize;
            param.SectorSize = 1024;
            param.FileNameEncoding = Encoding.GetEncoding("shift-jis");
            var fat = new PC98FatFileSystem(disk.Content, param);
            var files = fat.GetFiles(@"\");
            Array.Sort(files);
            foreach (var file in files) {
                var length = fat.GetFileLength(file) / 1024.0;
                var info = fat.GetFileInfo(file);
                Console.WriteLine(file + $@" ({length:F2} Kb) {info.CreationTime} {info.LastAccessTime} {info.LastWriteTime}");
                var path = Path.Combine(exportPath, new FileInfo(file).Name);
                using (var outfh = File.OpenWrite(path)) {
                    using (var infh = fat.OpenFile(file, FileMode.Open)) {
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
        private static void Tester(string file) {
            var disk = new DiscUtils.Fdi.Disk(file);
            var stream = new SubStream(disk.Content, 0x400, disk.Content.Length - 0x4000);
            var fat = new PC98FatFileSystem(disk.Content);
            walkDir(fat, @"\", 0);
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

        private static void D88Checker() {
            var files = Directory.GetFiles(@"H:\translations\Translations\Floppy\d88");
            foreach (var file in files) {
                Console.WriteLine(file);
                try {
                    CheckD88(file);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static void CheckD88(string file) {
            using (var disk = VirtualDisk.OpenDisk(file, FileAccess.Read)) {
                using (var fs = new PC98FatFileSystem(disk.Content)) {
                    //ShowDir(fs.Root, 6);
                    walkDir(fs, @"\", 0);
                }
            }
        }

        private static void FDIChecker1(string[] files) {
            foreach (var file in files) {
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Console.WriteLine(file);
                CheckFDI(file);
            }
        }

        private static void FDIChecker(string dirname) {
            var files = Directory.GetFiles(dirname);
            var start = false;
            foreach (var file in files) {
                if (!file.ToLower().EndsWith("fdi")) continue;
                if (!start) {
                    if (file == @"D:\Translations\FDI\5x5go\sabnack (j).fdi") {
                        start = true;
                    }
                    continue;
                }
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Console.WriteLine(file);
                try {
                    CheckFDI(file);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static void CheckFDI(string file) {
            var disk = VirtualDisk.OpenDisk(file, FileAccess.Read);
            if (disk == null) {
                Console.WriteLine("Can't read file");
                return;
            }
            var volMgr = new VolumeManager();
            volMgr.AddDisk(disk);
            foreach (var vol in volMgr.GetLogicalVolumes()) {
                var fileSystemInfos = FileSystemManager.DetectDefaultFileSystems(vol);
                if (fileSystemInfos == null || fileSystemInfos.Length == 0) {
                    Console.WriteLine("Can't find filesystems");
                    return;
                }
                foreach (var fsi in fileSystemInfos) {
                    using (DiscFileSystem fs = fsi.Open(vol)) {
                        Console.WriteLine("    {0} Volume Label: {1}", fsi.Name, fs.VolumeLabel);
                        Console.WriteLine("    Files ({0})...", fsi.Name);
                        ShowDir(fs.Root, 6);
                    }
                    Console.WriteLine();
                }
            }
            return;
        }

        private static void ShowDir(DiscDirectoryInfo dirInfo, int indent) {
            Console.WriteLine("{0}{1,-50} [{2}]", new String(' ', indent), dirInfo.FullName, dirInfo.CreationTimeUtc);
            foreach (DiscDirectoryInfo subDir in dirInfo.GetDirectories()) {
                ShowDir(subDir, indent + 0);
            }
            foreach (DiscFileInfo file in dirInfo.GetFiles()) {
                Console.WriteLine("{0}{1,-50} [{2}]", new String(' ', indent), file.FullName, file.CreationTimeUtc);
            }
        }

        private static void FDIInfo(string dirname) {
            var dirs = Directory.GetFiles(dirname);
            var start = false;
            var stat = new Dictionary<string, int>();
            foreach (var file in dirs) {
                if (!file.ToLower().EndsWith("fdi")) continue;
                
                using (var f = File.Open(file, FileMode.Open)) {
                    f.Position = 0x1000 + 3;
                    var data = new byte[8];
                    f.Read(data, 0, 8);
                    var descr = Encoding.ASCII.GetString(data);
                    descr = descr.TrimEnd('\0');
                    if (descr.Contains(';')) {
                        descr = descr.Replace(';', ',');
                    }
                    f.Position = 0x1020;
                    var sizeb = new byte[4];
                    f.Read(sizeb, 0, 4);
                    var size = BitConverter.ToString(sizeb);
                    if (stat.ContainsKey(size)) {
                        stat[size]++;
                    } else {
                        stat.Add(size, 1);
                    }
                    f.Position = 0x1000;
                    data = new byte[0x3e];
                    f.Read(data, 0, 0x3e);
                    var hex = BitConverter.ToString(data);
                    var ascii = Encoding.ASCII.GetString(data).Replace('\r', ' ').Replace('\n', ' ').Replace(';', ' ');
                    Console.WriteLine($@"{new FileInfo(file).Name};{descr};{size};{hex};{ascii}");
                }
                
            }
            Console.WriteLine("=========================");
            foreach (var key in stat.Keys) {
                Console.WriteLine($@"{key};{stat[key]}");
            }
        }

        private static void FDIClear(string dirname) {
            var dirs = Directory.GetFiles(dirname);
            var start = false;
            foreach (var file in dirs) {
                if (!file.ToLower().EndsWith("fdi")) continue;
                if (!start) {
                    if (file.EndsWith("cinema1.fdi")) {
                        start = true;
                    } else {
                        continue;
                    }
                }
                var process = System.Diagnostics.Process.Start(file);
                Thread.Sleep(100);
                var title = process.MainWindowTitle;
                process.CloseMainWindow();
                process.Close();
                Thread.Sleep(500);
                if (string.IsNullOrEmpty(title)) {
                    Console.WriteLine($@"Delete file {file}");
                    File.Delete(file);
                    /*Console.Write($@"Delete file {file}? y/ ");
                    var ans = Console.ReadLine();
                    if (ans.StartsWith("y")) {
                        Console.WriteLine("Deleting...");
                        File.Delete(file);
                        continue;
                    }*/
                } else {
                    Console.WriteLine($@"OK - {title}");
                }
                continue;
                using (var f = File.Open(file, FileMode.Open)) {
                    f.Position = 0x1000 + 3;
                    var data = new byte[8];
                    f.Read(data, 0, 8);
                    var descr = Encoding.ASCII.GetString(data);
                    descr = descr.TrimEnd('\0');
                    f.Position = 0x1020;
                    var sizeb = new byte[4];
                    f.Read(sizeb, 0, 4);
                    Console.WriteLine($@"{new FileInfo(file).Name};{descr};{sizeb[0]:X2}-{sizeb[1]:X2}-{sizeb[2]:X2}-{sizeb[3]:X2}");
                }
            }
        }

        private static void TestPartition(string file) {
            using (var disk = new Disk(file)) {
                PartitionTable.IsPartitioned(disk.Content);
                var pt = PartitionTable.GetPartitionTables(disk);
                var p = pt[0].Partitions[0];
                var fs = new FatFileSystem(p.Open());
                var files = fs.GetFiles(@"\");
                Console.WriteLine(1 + disk.Partitions[0].LastSector - disk.Partitions[0].FirstSector);
                //walkDir(fs, @"\", 0);
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

        private static void DiskInfo(string filename) {
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

        public static void Newdisk(string filename) {
            /*long diskSize = 30 * 1024 * 1024; //30MB
using (Stream vhdStream = File.Create(@"C:\TEMP\mydisk.vhd"))
{
    Disk disk = Disk.InitializeDynamic(vhdStream, diskSize);
    BiosPartitionTable.Initialize(disk, WellKnownPartitionType.WindowsFat);
    using (FatFileSystem fs = FatFileSystem.FormatPartition(disk, 0, null))
    {
        fs.CreateDirectory(@"TestDir\CHILD");
        // do other things with the file system...
                Type: 0, Size: 15713280, SectorSize: 0x100, Sectors: 33, Surfaces: 6, Cylinders: 310
        Bootable: True, PartType: DOS, Active: True, FSType: FAT12, IPL section: 0x00, IPL Head: 0x00, IPL Cyl: 0x0001, Sector: 0x00, Head: 0x00, Cylinder: 0x0001, EndSector: 0x00, EndHead: 0x00, EndCyl: 0x012C, Name: PATCHER
   Partition offset: 0x00D600

    }
}*/
            /*using (var fh = File.Create(filename)) {
                var disk = Disk.InitializeFixed(fh, Ownership.None, HddType.Size15Mb);
                PC98PartitionTable.Initialize(disk, WellKnownPartitionType.PC98Fat);
                using (var fs = PC98FatFileSystem.FormatPartition(disk, 0, null, disk.Header.Sectorsize)) {
                    fs.CreateDirectory(@"\TestDir");
                }
            }*/
            filename = @"D:\Translations\Tools\Patcher\images\newdisk_reformat.hdi";
            using (var disk = new Disk(filename)) {
                PC98PartitionTable.Initialize(disk, WellKnownPartitionType.PC98Fat);
                using (var fs = PC98FatFileSystem.FormatPartition(disk, 0, null, disk.Header.Sectorsize)) {
                    //fs.CreateDirectory(@"\TestDir");
                }
            }
            filename = @"D:\Translations\Tools\Patcher\images\newdisk30.hdi";
            //TestPartition(filename);
            //DiskInfo(filename);
        }
    }
}