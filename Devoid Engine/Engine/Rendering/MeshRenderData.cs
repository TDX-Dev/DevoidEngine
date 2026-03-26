using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    struct MeshRenderData
    {
        public Matrix4x4 ModelMatrix;
        public Matrix4x4 ModelMatrixInv;
    }
}
