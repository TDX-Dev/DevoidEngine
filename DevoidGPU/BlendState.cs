using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct BlendState
    {
        public bool Enable;

        public BlendFactor SrcColor;
        public BlendFactor DstColor;
        public BlendOp ColorOp;

        public BlendFactor SrcAlpha;
        public BlendFactor DstAlpha;
        public BlendOp AlphaOp;

        public ColorMask WriteMask;
    }
}
