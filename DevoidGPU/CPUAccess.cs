using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    [Flags]
    public enum CpuAccess
    {
        None = 0,
        Read = 1 << 0,
        Write = 1 << 1
    }
}
