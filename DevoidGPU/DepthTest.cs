using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public enum DepthTest
    {
        Disabled,       // Always pass
        Less,           // Default
        LessEqual,
        Equal,
        Greater,
        GreaterEqual,
        NotEqual,
        Always
    }
}
