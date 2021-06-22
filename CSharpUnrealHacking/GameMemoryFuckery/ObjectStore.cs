using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUnrealHacking.GameMemoryFuckery
{
    class ObjectStore
    {
        private IntPtr g_GobjectPtr;
        public ObjectStore(IntPtr GobjectPtr)
        {
            g_GobjectPtr = GobjectPtr;
        }
    }
}
