﻿//
// Copyright (c) 2008-2009, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using DiscUtils;
using DiscUtils.Common;
using DiscUtils.Vhd;

namespace VHDDump
{
    class Program
    {
        private static CommandLineParameter _vhdFile;
        private static CommandLineSwitch _helpSwitch;
        private static CommandLineSwitch _quietSwitch;

        static void Main(string[] args)
        {
            _vhdFile = new CommandLineParameter("vhd_file", "Path to the VHD file to inspect.", false);
            _helpSwitch = new CommandLineSwitch(new string[] { "h", "?" }, "help", null, "Show this help.");
            _quietSwitch = new CommandLineSwitch("q", "quiet", null, "Run quietly.");

            CommandLineParser parser = new CommandLineParser("VHDDump");
            parser.AddParameter(_vhdFile);
            parser.AddSwitch(_helpSwitch);
            parser.AddSwitch(_quietSwitch);

            bool parseResult = parser.Parse(args);

            if (!_quietSwitch.IsPresent)
            {
                ShowHeader();
            }

            if (_helpSwitch.IsPresent || !parseResult)
            {
                parser.DisplayHelp();
                return;
            }

            using (Stream fileStream = File.OpenRead(_vhdFile.Value))
            {
                using (DiskImageFile vhdFile = new DiskImageFile(fileStream, Ownership.None))
                {
                    DiskImageFileInfo info = vhdFile.Information;

                    FileInfo fileInfo = new FileInfo(_vhdFile.Value);

                    Console.WriteLine("File Info");
                    Console.WriteLine("---------");
                    Console.WriteLine("           File Name: {0}", fileInfo.FullName);
                    Console.WriteLine("           File Size: {0} bytes", fileInfo.Length);
                    Console.WriteLine("  File Creation Time: {0} (UTC)", fileInfo.CreationTimeUtc);
                    Console.WriteLine("     File Write Time: {0} (UTC)", fileInfo.LastWriteTimeUtc);
                    Console.WriteLine();

                    Console.WriteLine("Common Disk Info");
                    Console.WriteLine("-----------------");
                    Console.WriteLine("              Cookie: {0:x8}", info.Cookie);
                    Console.WriteLine("            Features: {0:x8}", info.Features);
                    Console.WriteLine(" File Format Version: {0}.{1}", ((info.FileFormatVersion >> 16) & 0xFFFF), (info.FileFormatVersion & 0xFFFF) );
                    Console.WriteLine("       Creation Time: {0} (UTC)", info.CreationTimestamp);
                    Console.WriteLine("         Creator App: {0:x8}", info.CreatorApp);
                    Console.WriteLine("     Creator Version: {0}.{1}", ((info.CreatorVersion >> 16) & 0xFFFF), (info.CreatorVersion & 0xFFFF));
                    Console.WriteLine("     Creator Host OS: {0}", info.CreatorHostOS);
                    Console.WriteLine("       Original Size: {0} bytes", info.OriginalSize);
                    Console.WriteLine("        Current Size: {0} bytes", info.CurrentSize);
                    Console.WriteLine("    Geometry (C/H/S): {0}", info.Geometry);
                    Console.WriteLine("           Disk Type: {0}", info.DiskType);
                    Console.WriteLine("            Checksum: {0:x8}", info.FooterChecksum);
                    Console.WriteLine("           Unique Id: {0}", info.UniqueId);
                    Console.WriteLine("         Saved State: {0}", info.SavedState);
                    Console.WriteLine();

                    if (info.DiskType == FileType.Differencing || info.DiskType == FileType.Dynamic)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Dynamic Disk Info");
                        Console.WriteLine("-----------------");
                        Console.WriteLine("              Cookie: {0}", info.DynamicCookie);
                        Console.WriteLine("      Header Version: {0}.{1}", ((info.DynamicHeaderVersion >> 16) & 0xFFFF), (info.DynamicHeaderVersion & 0xFFFF));
                        Console.WriteLine("         Block Count: {0}", info.DynamicBlockCount);
                        Console.WriteLine("          Block Size: {0} bytes", info.DynamicBlockSize);
                        Console.WriteLine("            Checksum: {0:x8}", info.DynamicChecksum);
                        Console.WriteLine("    Parent Unique Id: {0}", info.DynamicParentUniqueId);
                        Console.WriteLine("   Parent Write Time: {0} (UTC)", info.DynamicParentTimestamp);
                        Console.WriteLine("         Parent Name: {0}", info.DynamicParentUnicodeName);
                        Console.Write("    Parent Locations: ");
                        foreach (string parentLocation in info.DynamicParentLocators)
                        {
                            Console.Write("{0}\n                      ", parentLocation);
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        private static void ShowHeader()
        {
            Console.WriteLine("VHDDump v{0}, available from http://codeplex.com/DiscUtils", GetVersion());
            Console.WriteLine("Copyright (c) Kenneth Bell, 2008-2009");
            Console.WriteLine("Free software issued under the MIT License, see LICENSE.TXT for details.");
            Console.WriteLine();
        }

        private static string GetVersion()
        {
            return typeof(Program).Assembly.GetName().Version.ToString(3);
        }
    }
}