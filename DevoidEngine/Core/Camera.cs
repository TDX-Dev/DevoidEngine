using DevoidEngine.Rendering;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Core
{
    public class Camera
    {
        public RenderTarget? RenderTarget { get; set; }
        public Frustum Frustum { get; private set; }

        public float FOV { get => MathHelper.RadToDeg(fov_radians); set => fov_radians = MathHelper.DegToRad(value); }
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 1000f;
        public Vector3 Position { get; private set; } = Vector3.Zero;

        public Vector3 Front { get; private set; } = Vector3.UnitZ;
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; } = Vector3.UnitX;


        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Matrix4x4 InverseProjection;
        public Matrix4x4 InverseView;
        public Matrix4x4 InverseViewProjection;

        internal float fov_radians = MathF.PI / 3.0f;
        internal float prev_aspectratio = 0;
        internal bool view_dirty = true;

        public Camera()
        {
            Frustum = new Frustum();
        }

        public CameraData GetCameraData(Vector2 screenSize)
        {
            return new CameraData
            {
                View = View,
                Projection = Projection,
                InverseProjection = InverseProjection,
                InverseView = InverseView,
                InverseViewProjection = InverseViewProjection,
                CameraPosition = Position,
                NearClip = Near,
                FarClip = Far,
                ScreenSize = screenSize
            };
        }

        public void UpdateProjectionMatrix(float aspectRatio)
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                fov_radians,
                aspectRatio,
                Near,
                Far);

            Matrix4x4.Invert(Projection, out InverseProjection);

            Frustum.Update(View * Projection);
            Matrix4x4.Invert(View * Projection, out InverseViewProjection);

            prev_aspectratio = aspectRatio;
            view_dirty = false;
        }

        public void UpdateView(Vector3 position, Vector3 front, Vector3 up)
        {
            Position = position;
            Front = Vector3.Normalize(front);
            Up = Vector3.Normalize(up);
            Right = Vector3.Normalize(Vector3.Cross(Front, Up));

            View = Matrix4x4.CreateLookAtLeftHanded(
                Position,
                Position + Front,
                Up);

            Matrix4x4.Invert(View, out InverseView);
            Matrix4x4.Invert(View * Projection, out InverseViewProjection);

            Frustum.Update(View * Projection);

            view_dirty = true;
        }

        public Vector3 WorldToScreen(Vector3 worldPos, float screenWidth, float screenHeight)
        {
            Matrix4x4 viewProj = View * Projection;

            Vector4 clip = Vector4.Transform(new Vector4(worldPos, 1.0f), viewProj);

            float w = clip.W;

            float safeW = MathF.Max(MathF.Abs(w), 0.00001f);

            Vector3 ndc = new Vector3(clip.X, clip.Y, clip.Z) / safeW;

            Vector2 screen;
            screen.X = (ndc.X * 0.5f + 0.5f) * screenWidth;
            screen.Y = (1.0f - (ndc.Y * 0.5f + 0.5f)) * screenHeight;

            return new(screen, w);
        }

        public Ray ScreenToWorldRay(Vector2 screenPosition, float screenWidth, float screenHeight)
        {
            float x = (screenPosition.X / screenWidth) * 2.0f - 1.0f;
            float y = 1.0f - (screenPosition.Y / screenHeight) * 2.0f;

            Vector3 nearNdc = new(x, y, 0.0f);
            Vector3 farNdc = new(x, y, 1.0f);

            Matrix4x4 inverseViewProjection =
                Matrix4x4.Invert(
                    View * Projection,
                    out Matrix4x4 inverse)
                    ? inverse
                    : Matrix4x4.Identity;

            Vector4 nearClip = new(nearNdc, 1.0f);
            Vector4 farClip = new(farNdc, 1.0f);

            Vector4 nearWorld =
                Vector4.Transform(nearClip, inverseViewProjection);

            Vector4 farWorld =
                Vector4.Transform(farClip, inverseViewProjection);

            nearWorld /= nearWorld.W;
            farWorld /= farWorld.W;

            Vector3 origin = nearWorld.AsVector3();
            Vector3 direction =
                Vector3.Normalize(farWorld.AsVector3() - origin);

            return new Ray(origin, direction);
        }

        public bool IntersectsAABB(Vector3 min, Vector3 max)
        {
            if (Frustum == null)
                return false;
            var planes = Frustum.Planes;

            for (int i = 0; i < 6; i++)
            {
                var plane = planes[i];

                Vector3 normal = plane.Normal;

                Vector3 positive;

                positive.X = normal.X >= 0 ? max.X : min.X;
                positive.Y = normal.Y >= 0 ? max.Y : min.Y;
                positive.Z = normal.Z >= 0 ? max.Z : min.Z;

                float distance =
                    normal.X * positive.X +
                    normal.Y * positive.Y +
                    normal.Z * positive.Z +
                    plane.D;

                if (distance < 0)
                    return false;
            }

            return true;
        }
    }
}
