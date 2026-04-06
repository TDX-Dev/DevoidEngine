using System.Numerics;

namespace DevoidEngine.Engine.Rendering.Shadows
{
    public struct ShadowData
    {
        public Matrix4x4 LightViewProj;
        public Vector2 AtlasOffset;
        public Vector2 AtlasScale;
        public Vector2 Padding;
    }
}