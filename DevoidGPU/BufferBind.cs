using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    [Flags]
    public enum BufferBind
    {
        None = 0,

        Vertex = 1 << 0,
        Index = 1 << 1,
        Uniform = 1 << 2,
        Storage = 1 << 3,

        ShaderResource = 1 << 4,
        UnorderedAccess = 1 << 5,

        Indirect = 1 << 6,
        TransferSrc = 1 << 7,
        TransferDst = 1 << 8
    }
}
