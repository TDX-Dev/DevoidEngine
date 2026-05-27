using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct BufferDescription
    {
        public ulong Size;

        public BufferBind Bind;
        public ResourceUsage Usage;

        public CpuAccess CpuAccess;

        public IntPtr InitialData;
    }
}
