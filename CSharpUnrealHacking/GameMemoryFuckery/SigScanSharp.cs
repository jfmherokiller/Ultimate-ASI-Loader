/*
 * Name: SigScanSharp
 * Author: Striekcarl/GENESIS @ Unknowncheats
 * Date: 14/05/2017
 * Purpose: Find memory patterns, both individually or simultaneously, as fast as possible
 * 
 * Example:
 *  Init:
 *      Process TargetProcess = Process.GetProcessesByName("TslGame")[0];
 *      SigScanSharp Sigscan = new SigScanSharp(TargetProcess.Handle);
 *      Sigscan.SelectModule(procBattlegrounds.MainModule);
 * 
 *  Find Patterns (Simultaneously):
 *      Sigscan.AddPattern("Pattern1", "48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6");
 *      Sigscan.AddPattern("Pattern2", "E8 0A EC ? ? FF");
 *      
 *      long lTime;
 *      var result = Sigscan.FindPatterns(out lTime);
 *      var offset = result["Pattern1"];
 *      
 *  Find Patterns (Individual):
 *      long lTime;
 *      var offset = Sigscan.FindPattern("48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6", out lTime);
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NativeManager.WinApi;

namespace CSharpUnrealHacking.GameMemoryFuckery
{
    public class SigScanSharp
    {
        private IntPtr g_hProcess { get; set; }
        private byte[] g_arrModuleBuffer { get; set; }
        private ulong g_lpModuleBase { get; set; }

        private Dictionary<string, string> g_dictStringPatterns { get; }
        public Dictionary<string, string> g_CheckerPatterns;

        public SigScanSharp(IntPtr hProc)
        {
            g_hProcess = hProc;
            g_dictStringPatterns = new Dictionary<string, string>();
            g_CheckerPatterns = new Dictionary<string, string>();
        }

        public bool SelectModule(ProcessModule targetModule)
        {
            g_lpModuleBase = (ulong)targetModule.BaseAddress;
            g_arrModuleBuffer = new byte[targetModule.ModuleMemorySize];

            g_dictStringPatterns.Clear();

            return Win32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer, targetModule.ModuleMemorySize);
        }

        public void AddPattern(string szPatternName, string szPattern)
        {
            g_dictStringPatterns.Add(szPatternName, szPattern);
        }

        public bool PatternCheck(int nOffset, byte[] arrPattern)
        {
            for (var i = 0; i < arrPattern.Length; i++)
            {
                var realoffset = nOffset + i;
                if (arrPattern[i] == 0x0)
                    continue;
                if (realoffset >= this.g_arrModuleBuffer.Length)
                    return false;
                if (arrPattern[i] != this.g_arrModuleBuffer[nOffset + i])
                    return false;
            }

            return true;
        }
        public bool PatternCheck(int nOffset,byte[] InternalBuffer, byte[] arrPattern)
        {
            for (var i = 0; i < arrPattern.Length; i++)
            {
                var realoffset = nOffset + i;
                if (arrPattern[i] == 0x0)
                    continue;
                if (realoffset >= InternalBuffer.Length)
                    return false;
                if (arrPattern[i] != InternalBuffer[realoffset])
                    return false;
            }

            return true;
        }
        public bool PatternCheck(byte[] orginalBytes, byte[] arrPattern)
        {
            for (var i = 0; i < arrPattern.Length; i++)
            {
                if (arrPattern[i] == 0x0)
                    continue;

                if (arrPattern[i] != orginalBytes[i])
                    return false;
            }

            return true;
        }
        public int FindPatternModIndex(string szPattern, out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var stopwatch = Stopwatch.StartNew();

            var arrPattern = ParsePatternString(szPattern);

            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                    continue;

                if (PatternCheck(nModuleIndex, arrPattern))
                {
                    lTime = stopwatch.ElapsedMilliseconds;
                    return nModuleIndex;
                }
            }

            lTime = stopwatch.ElapsedMilliseconds;
            return 0;
        }
        public List<ulong> FindPatternA(string szPattern)
        {
            var Results = new List<ulong>();
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var arrPattern = ParsePatternString(szPattern);

            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                    continue;

                if (PatternCheck(nModuleIndex, arrPattern))
                {
                    Results.Add(g_lpModuleBase + (ulong)nModuleIndex);
                }
            }

            return Results;
        }
        public List<ulong> FindPatternA(byte[] szPattern)
        {
            var Results = new List<ulong>();
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                if (this.g_arrModuleBuffer[nModuleIndex] != szPattern[0])
                    continue;

                if (PatternCheck(nModuleIndex, szPattern))
                {
                    Results.Add(g_lpModuleBase + (ulong)nModuleIndex);
                }
            }

            return Results;
        }
        public ulong FindPattern(string szPattern, out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var stopwatch = Stopwatch.StartNew();

            var arrPattern = ParsePatternString(szPattern);

            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                    continue;

                if (PatternCheck(nModuleIndex, arrPattern))
                {
                    lTime = stopwatch.ElapsedMilliseconds;
                    return g_lpModuleBase + (ulong)nModuleIndex;
                }
            }

            lTime = stopwatch.ElapsedMilliseconds;
            return 0;
        }

        //public ulong ReadEntireProcess(byte[] SpecialPattern)
        //{
        //    var sys_info = new Kernel32.SYSTEM_INFO();
        //    Kernel32.GetSystemInfo(out sys_info);

        //    var proc_min_address = sys_info.minimumApplicationAddress;
        //    var proc_max_address = sys_info.maximumApplicationAddress;
        //    return ReadEntireProcess(proc_min_address, proc_max_address, SpecialPattern);
        //}
        //public ulong ReadEntireProcess(UIntPtr start,UIntPtr end,byte[] SpecialPattern)
        //{
        //    // REQUIRED CONSTS
        //    const int MEM_COMMIT = 0x00001000;
        //    const int PAGE_READWRITE = 0x04;
        //    // getting minimum & maximum address

        //    var InnerStart = start;
        //    // saving the values as long ints so I won't have to do a lot of casts later
        //    var proc_min_address_l = InnerStart.ToUInt64();
        //    var proc_max_address_l = (ulong)end;
        //    // this will store any information we get from VirtualQueryEx()
        //    Kernel32.MEMORY_BASIC_INFORMATION mem_basic_info = new Kernel32.MEMORY_BASIC_INFORMATION();

        //    IntPtr bytesRead = new IntPtr(0);  // number of bytes read with ReadProcessMemory

        //    while (proc_min_address_l < proc_max_address_l)
        //    {
        //        // 28 = sizeof(MEMORY_BASIC_INFORMATION)
        //        Kernel32.VirtualQueryEx(g_hProcess, start, out mem_basic_info, 28);

        //        // if this memory chunk is accessible
        //        if (mem_basic_info.Protect == PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
        //        {
        //            var buffer = new byte[mem_basic_info.RegionSize];
        //            var baseAddress = mem_basic_info.BaseAddress;
        //            // read everything in the buffer above
        //            Kernel32.ReadProcessMemory(g_hProcess,baseAddress, buffer, mem_basic_info.RegionSize, bytesRead);

        //            // then output this in the file
        //            for (var i = 0; i < mem_basic_info.RegionSize;)
        //            {
        //                if (PatternCheck(i, buffer, SpecialPattern))
        //                {
        //                    return (ulong) (baseAddress + i);
        //                }

        //                i += SpecialPattern.Length;
        //            }
        //        }

        //        // move to the next memory chunk
        //        proc_min_address_l += (ulong)mem_basic_info.RegionSize;
        //        InnerStart = new UIntPtr(proc_min_address_l);
        //    }

        //    return 0;
        //}
        public ulong FindPattern(ulong startAddress,string szPattern, out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var stopwatch = Stopwatch.StartNew();

            var arrPattern = ParsePatternString(szPattern);

            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                    continue;

                if (PatternCheck(nModuleIndex, arrPattern))
                {
                    lTime = stopwatch.ElapsedMilliseconds;
                    return g_lpModuleBase + (ulong)nModuleIndex;
                }
            }

            lTime = stopwatch.ElapsedMilliseconds;
            return 0;
        }
        public Dictionary<string, List<ulong>> FindPatternsA()
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var arrBytePatterns = new byte[g_dictStringPatterns.Count][];
            var arrResult = new List<ulong>[g_dictStringPatterns.Count];

            // PARSE PATTERNS
            for (var nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
                arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);

            // SCAN FOR PATTERNS

            for (var nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
            {
                var results = FindPatternA(arrBytePatterns[nPatternIndex]);

                arrResult[nPatternIndex] = results;
            }

            var dictResultFormatted = new Dictionary<string, List<ulong>>();

            // FORMAT PATTERNS
            for (var nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

            return dictResultFormatted;
        }
        public Dictionary<string, ulong> FindPatterns(out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var stopwatch = Stopwatch.StartNew();

            var arrBytePatterns = new byte[g_dictStringPatterns.Count][];
            var arrResult = new ulong[g_dictStringPatterns.Count];
        
            // PARSE PATTERNS
            for (var nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
                arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);
        
            // SCAN FOR PATTERNS
            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                for (var nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                {
                    if (arrResult[nPatternIndex] != 0)
                        continue;

                    if (PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
                        arrResult[nPatternIndex] = g_lpModuleBase + (ulong)nModuleIndex;
                }
            }

            var dictResultFormatted = new Dictionary<string, ulong>();

            // FORMAT PATTERNS
            for (var nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

            lTime = stopwatch.ElapsedMilliseconds;
            return dictResultFormatted;
        }
        public Dictionary<string, int> FindPatternsIndex(out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            var stopwatch = Stopwatch.StartNew();

            var arrBytePatterns = new byte[g_dictStringPatterns.Count][];
            var arrResult = new int[g_dictStringPatterns.Count];

            // PARSE PATTERNS
            for (var nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
                arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);

            // SCAN FOR PATTERNS
            for (var nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                for (var nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                {
                    if (arrResult[nPatternIndex] != 0)
                        continue;

                    if (PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
                        arrResult[nPatternIndex] = nModuleIndex;
                }
            }

            var dictResultFormatted = new Dictionary<string, int>();

            // FORMAT PATTERNS
            for (var nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

            lTime = stopwatch.ElapsedMilliseconds;
            return dictResultFormatted;
        }
        public void ClearPatterns()
        {
            g_dictStringPatterns.Clear();
        }

        public ulong FixAddress(int address)
        {
            return g_lpModuleBase + (ulong) address;
        }
        public byte[] ParsePatternString(string szPattern)
        {
            var patternbytes = new List<byte>();

            foreach (var szByte in szPattern.Split(' '))
            {
                if (szByte == "??")
                {
                    patternbytes.Add(0x0);
                }
                else
                {
                    patternbytes.Add(Convert.ToByte(szByte, 16));
                }
            }
            return patternbytes.ToArray();
        }

        public byte[] ReadBytes(ulong Address, int Sizeme)
        {
            var Mybuff = new byte[Sizeme];
            Win32.ReadProcessMemory(g_hProcess, Address, Mybuff, Sizeme);
            return Mybuff;
        }

        public ulong ReadAddress(ulong address)
        {
            return BitConverter.ToUInt64(ReadBytes(address, sizeof(ulong)),0);
        }


        private static class Win32
        {
            [DllImport("kernel32.dll")]
            public static extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, int lpNumberOfBytesRead = 0);
        }
    }
}