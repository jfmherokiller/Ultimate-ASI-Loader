using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace CSharpUnrealHacking.GameMemoryFuckery
{
    class SigScanningBits
    {
        public SigScanSharp sigScan;
        
        private Dictionary<string, int> CheckerOffset;
        private Dictionary<string, int> BytesToExtract;
        private Dictionary<string, ulong> SingularPatterns;
        public SigScanningBits(Process ModuleHandle)
        {
            BytesToExtract = new Dictionary<string, int>();
            CheckerOffset = new Dictionary<string, int>();
            sigScan = new SigScanSharp(ModuleHandle.Handle);
            sigScan.SelectModule(ModuleHandle.MainModule);
        }

        public void addPatternAndChecker(string Name, string Checkpattern, string RealPattern,int offset = -6,int bytesToExtract =4)
        {
            CheckerOffset.Add(Name, offset);
            sigScan.g_CheckerPatterns.Add(Name,Checkpattern);
            sigScan.AddPattern(Name,RealPattern);
            BytesToExtract.Add(Name,bytesToExtract);
        }
        public void addPatternAndCheckerTwo(string Name, string Checkpattern, string RealPattern, int offset = -6, int bytesToExtract = 4)
        {
            CheckerOffset.Add(Name, offset);
            sigScan.g_CheckerPatterns.Add(Name, Checkpattern);
            SingularPatterns.Add(Name,sigScan.FindPattern(RealPattern,out _));
            //sigScan.AddPattern(Name, RealPattern);
            BytesToExtract.Add(Name, bytesToExtract);
        }
        public Dictionary<string, ulong> SearchForPatternsA()
        {
            var Results = sigScan.FindPatternsA();
            sigScan.ClearPatterns();
            //FilterZeros
            Results = Results.Where(Result => Result.Value.Count != 0).ToDictionary(x => x.Key, x => x.Value);
            var Subset = CheckPatternsA(Results);
            foreach (var keyValuePair in Subset)
            {
                var mybytes = BitConverter.GetBytes(keyValuePair.Value);
                Array.Reverse(mybytes);
                MessageBox.Show(BitConverter.ToString(mybytes).Replace("-", ""), keyValuePair.Key);
            }
            return Subset;
        }
        public Dictionary<string, ulong> SearchForPatterns()
        {
            var Results = sigScan.FindPatterns(out _);
            sigScan.ClearPatterns();
            //FilterZeros
            Results = Results.Where(Result => Result.Value != 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var keyValuePair in Results)
            {
                var mybytes = BitConverter.GetBytes(keyValuePair.Value);
                Array.Reverse(mybytes);
                MessageBox.Show(BitConverter.ToString(mybytes).Replace("-",""), keyValuePair.Key);
            }
            return CheckPatterns(Results);
        }
        public Dictionary<string, ulong> CheckPatternsA(Dictionary<string, List<ulong>> results)
        {
            var filteredresults = new Dictionary<string,List<ulong>>();
                foreach (var myres in results)
                {
                    var kv = FilteredBitsA(myres);
                    filteredresults.Add(kv.Key,kv.Value);
                }

                var Simplifed = filteredresults.ToDictionary(Resultt => Resultt.Key,
                    Resultt => Resultt.Value.FirstOrDefault());
                return Simplifed.Where(FilteredBits).ToDictionary(x => x.Key, x => x.Value + (ulong)CheckerOffset[x.Key] + (ulong)sigScan.ParsePatternString(sigScan.g_CheckerPatterns[x.Key]).Length);
        }
        public Dictionary<string, ulong> CheckPatterns(Dictionary<string, ulong> results)
        {

            return results.Where(FilteredBits).ToDictionary(x => x.Key, x => x.Value + (ulong)CheckerOffset[x.Key]+ (ulong)sigScan.ParsePatternString(sigScan.g_CheckerPatterns[x.Key]).Length);
        }
        private KeyValuePair<string, List<ulong>> FilteredBitsA(KeyValuePair<string, List<ulong>> Result)
        {
            var CheckerPattern = sigScan.ParsePatternString(sigScan.g_CheckerPatterns[Result.Key]);
            var innerresult = Result.Value;
            innerresult = innerresult.FindAll(ResultT => sigScan.PatternCheck(sigScan.ReadBytes(ResultT + (ulong)CheckerOffset[Result.Key], CheckerPattern.Length), CheckerPattern));
            return new KeyValuePair<string, List<ulong>>(Result.Key, innerresult);
        }
        private bool FilteredBits(KeyValuePair<string, ulong> Result)
        {
            var CheckerPattern = sigScan.ParsePatternString(sigScan.g_CheckerPatterns[Result.Key]);
            var Address = Result.Value + (ulong) CheckerOffset[Result.Key];
            return sigScan.PatternCheck(sigScan.ReadBytes(Address, CheckerPattern.Length),CheckerPattern );
        }

        public Dictionary<string, ulong> HandlePatterns()
        {
            return SearchForPatterns();
        }
    }
}
