using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SceneData
    {
        public uint pointLightCount;
        public uint spotLightCount;
        public uint directionalLightCount;
        public int _padding;
    }
}
