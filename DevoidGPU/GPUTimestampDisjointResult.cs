using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public readonly struct GpuTimestampDisjointResult
    {
        public readonly bool Disjoint;
        public readonly ulong Frequency;

        public GpuTimestampDisjointResult(bool disjoint, ulong frequency)
        {
            Disjoint = disjoint;
            Frequency = frequency;
        }
    }
}
