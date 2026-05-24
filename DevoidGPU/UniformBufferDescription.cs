using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct UniformBufferDescription
    {
        public ulong Size;
        public BufferUsage Usage;
        public IntPtr InitialData;
    }
}
