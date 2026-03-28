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
    public struct CameraData
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Matrix4x4 InverseProjection;
        public Matrix4x4 InverseView;
        public Matrix4x4 InverseViewProjection;
        public Vector3 Position;
        public float NearClip;
        public float FarClip;
        public Vector2 ScreenSize;
        private float _padding0;
    }
}
