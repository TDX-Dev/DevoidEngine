using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraData
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Matrix4x4 InverseProjection;
        public Matrix4x4 InverseView;
        public Matrix4x4 InverseViewProjection;
        public Vector3 CameraPosition;
        public float NearClip;
        public float FarClip;
        public Vector2 ScreenSize;
        public int FrameIndex;
    }
}
