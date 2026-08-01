using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    [Flags]
    public enum MemoryBarrierFlags
    {
        None = 0,

        ShaderStorage = 1 << 0,

        Texture = 1 << 1,

        Uniform = 1 << 2,

        All = ~0
    }
}
