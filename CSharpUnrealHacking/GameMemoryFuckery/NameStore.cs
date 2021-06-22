using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeManager.MemoryInteraction;

namespace CSharpUnrealHacking.GameMemoryFuckery
{
    public struct FNameEntity
    {
        public ulong Index;
        public string AnsiName;
    }
    class NameStore
    {
        private ulong G_GnamePtr;
        private MemoryManager SigScanInstance;
        private int chunkCount = 16384;
        public List<FNameEntity> Gnames;
        public NameStore(ulong GnamePtr,MemoryManager myBits)
        {
            G_GnamePtr = GnamePtr;
            SigScanInstance = myBits;
            Gnames = new List<FNameEntity>();
            ReadGnameArray(GnamePtr);

        }

        public bool ReadGnameArray(ulong address)
        {
            var ptrSize = IntPtr.Size;



            // Calc AnsiName offset
            

                var none_sig = this.SigScanInstance.GetPatternManager().FindPatternCBase((IntPtr) G_GnamePtr, PatternManager.ParsePattern("4E 6F 6E 65 00").ToArray());
                var byte_sig = this.SigScanInstance.GetPatternManager().FindPatternCBase((IntPtr) G_GnamePtr,
                    PatternManager.ParsePattern("42 79 74 65 50 72 6F 70 65 72 74 79 00").ToArray());
                var NameOffset = (byte_sig.ToInt64() - none_sig.ToInt64()) - 4;
                var GchunkAddress = new PageManager(SigScanInstance)[none_sig - (int)NameOffset].AllocationBase;
                // Get GNames Chunks
            //std::vector<uintptr_t> gChunks;
            var gNamesChunks = new IntPtr[0].ToList();
            for (var iAIndex = 0; iAIndex < 30; ++iAIndex)
            {
                var offset = ptrSize * iAIndex;
                var addr = SigScanInstance.Read<IntPtr>((IntPtr)(GchunkAddress + (ulong)offset));

                //addr = Utils::MemoryObj->ReadAddress(address + offset);

                //if (!IsValidAddress(addr)) break;
                if(addr.ToInt64() == 0) break;

                gNamesChunks.Add(addr);
            }

            // Dump GNames
            var i = 0;
            foreach (var chunkAddress in gNamesChunks)
            {
                for (var j = 0; j < chunkCount; ++j)
                {
                    var tmp = new FNameEntity();
                    var offset = ptrSize * j;
                    var fNameAddress = SigScanInstance.Read<IntPtr>(chunkAddress + offset);
                    //if (!IsValidAddress(fNameAddress))
                    //{
                    //    // Push Empty, if i just skip will case a problems, so just add empty item
                    //    tmp.Index = (ulong) (i + 1); // FNameEntity Index look like that 0 .. 2 .. 4 .. 6
                    //    tmp.AnsiName = this.SigScanInstance.ReadString(f)
                    //
                    //    Gnames.Add(tmp);
                    //    ++i;
                    //    continue;
                    //}
                    tmp.Index = (ulong)(i + 1);
                    tmp.AnsiName = this.SigScanInstance.ReadString(fNameAddress+(int) NameOffset);
                    // Read FName
                    //if (!tmp.ReadData(fNameAddress, nameOffset)) return false;

                    Gnames.Add(tmp);
                    ++i;
                }
            }

            return true;
		}
    }
}
