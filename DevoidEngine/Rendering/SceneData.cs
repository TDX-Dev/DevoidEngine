using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering
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
