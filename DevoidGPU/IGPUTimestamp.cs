using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface IGPUTimestamp : IDisposable
    {
        bool TryGetTimestamp(out ulong timestamp);
    }
}
