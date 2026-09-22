using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface IGPUTimestampDisjoint : IDisposable
    {
        bool TryGetResult(out long frequency, out bool disjoint);
    }
}
