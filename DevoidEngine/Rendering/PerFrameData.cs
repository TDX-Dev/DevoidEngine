using DevoidEngine.Rendering.ProbeGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameData
    {
        public int FrameIndex;
        public Vector3 _padding;
        public ProbeGISettings ProbeGISettings;
    }
}
