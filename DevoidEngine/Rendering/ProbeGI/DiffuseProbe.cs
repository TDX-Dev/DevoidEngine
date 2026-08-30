using DevoidEngine.Util;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering.ProbeGI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DiffuseProbe
    {
        public float Weight;
        public Vector3 Padding;
        public SH9 Coefficients;
    }
}
