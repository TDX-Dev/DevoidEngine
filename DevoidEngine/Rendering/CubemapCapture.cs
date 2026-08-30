using DevoidEngine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public sealed class CubemapCapture
    {
        public static readonly Vector3[] Directions =
        [
            Vector3.UnitX,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ
        ];

        public static readonly Vector3[] Ups =
        [
            Vector3.UnitY,
            Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ,
            Vector3.UnitY,
            Vector3.UnitY
        ];

        public CameraData CameraData;

        public CubemapCapture(int resolution, float nearClip = 0.1f, float farClip = 1000.0f)
        {
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(MathF.PI * 0.5f, 1.0f, nearClip, farClip);

            CameraData = new()
            {
                Projection = projection,
                View = Matrix4x4.Identity,
                ScreenSize = new Vector2(resolution),
                FarClip = farClip,
                NearClip = nearClip,
                CameraPosition = Vector3.Zero
            };
        }

        public Matrix4x4 GetView(int face, Vector3 position)
        {
            return Matrix4x4.CreateLookAtLeftHanded(
                position,
                position + Directions[face],
                Ups[face]);
        }
    }
}