using DevoidEngine.Rendering;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DevoidEngine.Core
{
    public class Camera
    {
        public RenderTarget? RenderTarget { get; set; }
        public Frustum? Frustum { get; private set; }

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
                ScreenSize = screenSize,
            };
        }

        public void UpdateProjectionMatrix(float aspectRatio)
        {
            if (aspectRatio == prev_aspectratio && !view_dirty)
                return;
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov_radians, aspectRatio, Near, Far);
            Matrix4x4.Invert(Projection, out InverseProjection);
            Frustum = Frustum.FromMatrix(View * Projection);
            prev_aspectratio = aspectRatio;
            view_dirty = false;
        }

        public void UpdateView(Vector3 position, Vector3 front, Vector3 up)
        {
            Position = position;
            Front = Vector3.Normalize(front);
            Up = Vector3.Normalize(up);
            Right = Vector3.Normalize(Vector3.Cross(Up, Front));

            View = Matrix4x4.CreateLookAt(Position, Position + Front, Up);

            Matrix4x4.Invert(View, out InverseView);
            Matrix4x4.Invert(View * Projection, out InverseViewProjection);

            Frustum = Frustum.FromMatrix(View * Projection);
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

            return new (screen, w);
        }
    }
}
