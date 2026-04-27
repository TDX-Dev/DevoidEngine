using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct BlendStateDescription
    {
        public BlendState[] BlendStates;
        public bool AlphaToCoverage;
        public bool IndependentBlend;
    }
}
