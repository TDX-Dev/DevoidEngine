using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public abstract class GPUResource : IDisposable
    {
        // Size of GPU Resource in bytes
        public ulong Size { get; protected set; } = 0;

        public string DebugName { get; set; } = "";

        public abstract void Dispose();
    }
}
