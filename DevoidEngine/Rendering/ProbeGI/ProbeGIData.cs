using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ProbeGIData
    {
        public Vector3 GridMin;
        public float Spacing;

        public uint ResolutionX;
        public uint ResolutionY;
        public uint ResolutionZ;
        public uint ProbeCount;
    }
}
