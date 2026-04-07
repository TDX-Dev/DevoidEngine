using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering.Shadows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShadowData
    {
        public Matrix4x4 LightViewProj;
        public Vector2 AtlasOffset;
        public Vector2 AtlasScale;
        public Vector4 Padding;
    }
}