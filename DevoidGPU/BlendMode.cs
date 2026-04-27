using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public enum BlendMode
    {
        Opaque,         // No blending
        AlphaBlend,     // SrcAlpha, 1-SrcAlpha
        Additive,       // SrcAlpha, One
        Multiply,        // DstColor, Zero,
        PremultipliedAlpha
    }


}
