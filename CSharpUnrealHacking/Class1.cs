using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpUnrealHacking.GameMemoryFuckery;
using NativeManager.MemoryInteraction;
using NativeManager.WinApi;
using NativeManager.WinApi.Enums;

public static class Plugin
{
    public static void PluginStartMethod()
    {
        CSharpUnrealHacking.Class1.Main();
    }
}

namespace CSharpUnrealHacking
{
    public class Class1
    {
        private static SigScanningBits sigScan;
        private static MemoryManager hMemory;
        /// <summary>
        /// 
        /// </summary>
        public static void Main()
        {
            MessageBox.Show("WaitTillMainMenu");
            //Thread.Sleep(3000);
            //SigScanTest();
            InitSigScan();
            SigScanNew();
        }

        public static void InitSigScan()
        {
            hMemory = new MemoryManager(Process.GetCurrentProcess().ProcessName);
        }
        public static void SigScanNew()
        {
            FindGengine();
            FindGnameHard();
            //MessageBox.Show(BitConverter.ToString(BitConverter.GetBytes(returnvalue.ToInt64())));
            //hMemory.GetPatternManager().FindPattern()
        }

        private static void FindGnameHard()
        {
            ulong found = 0;
            var pattens = hMemory.GetPatternManager();
            var mypages = new PageManager(hMemory);
            var allpages = mypages.GetAllPages().Where(Result =>
                Result.RegionSize == 0x40000 && Result.Protect == (int) AllocationProtect.PAGE_READWRITE).ToList();
            var ByteChecked = allpages.Where(delegate(MEMORY_BASIC_INFORMATION Result)
            {
                var bytes = hMemory.ReadBytes((IntPtr) Result.BaseAddress, 32);
                return pattens.FindPatternBuff(bytes, PatternManager.ParsePattern("4E 6F 6E 65 00").ToArray()) != 0;
            }).ToList();
            if (ByteChecked.Count == 1)
            {
                found = ByteChecked.First().BaseAddress;
            }

            var realBase = pattens.FindPatternCBase((IntPtr) found, PatternManager.ParsePattern("4E 6F 6E 65 00").ToArray());
            GNameParsing(found);

        }
        private static IntPtr FindGengine()
        {
            var pattens = hMemory.GetPatternManager();
            var mybase = hMemory.ProcessMemory.MainModule.BaseAddress;
            var mypages = new PageManager(hMemory);
            //get gengne
            var returnvalue = pattens.FindPattern(mybase,
                "?? ?? ?? ?? 48 8B 88 ?? ?? 00 00 48 85 C9 74 ?? 48 8B 49 ?? 48 85 C9");
            //next portion finds the correct mov instruct
            var subbed = returnvalue + 3000;
            var specialreturn = pattens.FindPatternCBase(subbed, PatternManager.ParsePattern("48 8B 05").ToArray());
            //extract mov bytes
            var secondOFfset = BitConverter.ToInt32(hMemory.ReadBytes(specialreturn, 7).Skip(3).ToArray(),0);
            //get address stored for gengine jump
            var nextPosition = specialreturn + secondOFfset + 7;
            var MegaJump = hMemory.Read<IntPtr>(nextPosition);
            return MegaJump;
        }

        public static void SigScanTest()
        {
            var test = Process.GetCurrentProcess();

            MessageBox.Show("test");
            sigScan = new SigScanningBits(test);
            var Found = ProcessPatterns();
            foreach (var keyValuePair in Found)
            {
                var mybytes = BitConverter.GetBytes(keyValuePair.Value);
                Array.Reverse(mybytes);
                MessageBox.Show(BitConverter.ToString(mybytes).Replace("-", ""), keyValuePair.Key);
            }
            //GNameParsing(Found);
        }

        public static void GetAnswerFromObjectDump()
        {
            var drect = Directory.GetCurrentDirectory();
            var objectsfile =Directory.GetFiles(drect).First(Result => Result.Contains("ObjectsDump"));
            var MylineAddress = File.ReadLines(objectsfile).First(Result => Result.Contains("Package CoreUObject"))
                .Split(' ').First(Result => Result.Length > 10 && Result.StartsWith("0")).Replace("0x","");
            var address = Convert.ToUInt64(MylineAddress, 16);
        }
        public static void GNameParsing(ulong found)
        {
            var mynames = new NameStore(found,hMemory);
            var FIleDump = Path.Combine(Directory.GetCurrentDirectory(), "Gnamedump.txt");
            var myfile =File.CreateText(FIleDump);
            foreach (var fNameEntity in mynames.Gnames)
            {
                var OutString = $"[{fNameEntity.Index}] {fNameEntity.AnsiName}";
                myfile.WriteLine(OutString);
            }
            myfile.Flush();
            myfile.Close();
        }

        public static Dictionary<string, ulong> ProcessPatterns()
        {
            //var Select = sigScan.sigScan.ReadEntireProcess(sigScan.sigScan.ParsePatternString("4E 6F 6E 65 00 00 00 00 00"));
            //    MessageBox.Show(BitConverter.ToString(BitConverter.GetBytes(Select).Reverse().ToArray()).Replace("-",""));

            //    MessageBox.Show("done");
            //return new Dictionary<string, ulong>();
            sigScan.addPatternAndChecker("GameVersionP1", "C7 05 ?? ?? ?? ??", "04 00 ?? 00 66 89 ?? ?? ?? ?? ?? 89 05");
            sigScan.addPatternAndChecker("GameVersionP2", "C7 05 ?? ?? ?? ??", "04 00 ?? 00 66 89 ?? ?? ?? ?? ?? C7 05");
            sigScan.addPatternAndChecker("GameVersionP3", "C7 05 ?? ?? ?? ??", "04 00 ?? 00 66 89 ?? ?? ?? ?? ?? 89");
            sigScan.addPatternAndChecker("GameVersionP4", "41 C7 ??", "04 00 ?? 00 B9 01 00 00 00");
            sigScan.addPatternAndChecker("GameVersionP5", "41 C7 ??", "04 00 18 00 66 41 89 ?? 04");
            sigScan.addPatternAndChecker("GENGINE", "48 8B 05", "?? ?? ?? ?? 48 8B 88 ?? ?? 00 00 48 85 C9 74 ?? 48 8B 49 ?? 48 85 C9");
            sigScan.addPatternAndChecker("OBJECTSSTOREP1", "48 8D 0D", "?? ?? ?? ?? C6 05 ?? ?? ?? ?? 01 E8 ?? ?? ?? ?? C6 05 ?? ?? ?? ?? 01 C6 05 ?? ?? ?? ?? 00 80 3D ?? ?? ?? ??", -3);
            sigScan.addPatternAndChecker("NAMESSTOREP1", "48 8D 15", "?? ?? ?? ?? EB 16 48 8D 0D ?? ?? ?? ?? E8",0);
            sigScan.addPatternAndChecker("NAMESSTOREP2", "E8 ?? ?? ?? ?? 48 8B C3 48 89 1D", "?? ?? ?? ?? 48 8B 5C 24",0);
            sigScan.addPatternAndChecker("NAMESSTOREP3", "E8 ?? ?? ?? ?? 48 89 D8 48 89 1D", "?? ?? ?? ?? 48 8B 5C 24 20 48 83 C4 28 C3 31 DB 48 89 1D",0);
            sigScan.addPatternAndChecker("NAMESSTOREP4", "E8 ?? ?? ?? ?? 48 89 D8 48 89 1D", "?? ?? ?? ?? 48 8B 5C 24 20 48 83 C4 28 C3 48 8B 5C",0);
            sigScan.addPatternAndChecker("NAMESSTOREP5", "4E 6F 6E 65 00 00 00 00 00", "4E 6F 6E 65 00 00 00 00 00", 0);
            return sigScan.HandlePatterns();
        }
    }
}
