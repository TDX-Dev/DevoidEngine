using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SH9
    {
        public Vector4 C0;
        public Vector4 C1;
        public Vector4 C2;
        public Vector4 C3;
        public Vector4 C4;
        public Vector4 C5;
        public Vector4 C6;
        public Vector4 C7;
        public Vector4 C8;
    }
}
