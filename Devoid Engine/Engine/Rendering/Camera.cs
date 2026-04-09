using Assimp;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering
{

    public class Camera
    {
        public Framebuffer? RenderTarget { get; set; }
        public Frustum? Frustum { get; set; }

        public Vector3 Position { get; private set; } = new Vector3(0, 0, -5);
        public Vector3 Front { get; private set; } = Vector3.UnitZ;
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; } = Vector3.UnitX;

        public float NearClip { get; set; } = 0.01f;
        public float FarClip { get; set; } = 1000f;
        public float FovY { get; set; } = MathF.PI / 3f; // default 60°
        public Vector4 ClearColor { get; private set; } = Vector4.One;

        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 _invProjectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 _invViewMatrix = Matrix4x4.Identity;
        private Matrix4x4 _invViewProjectionMatrix = Matrix4x4.Identity;
       



        // Data for GPU
        public CameraData GetCameraData()
        {
            var renderTexture = RenderTarget!.GetRenderTexture(0);
            return new CameraData
            {
                View = _viewMatrix,
                Projection = _projectionMatrix,
                InverseProjection = _invProjectionMatrix,
                InverseView = _invViewMatrix,
                InverseViewProjection = _invViewProjectionMatrix,
                Position = Position,
                NearClip = NearClip,
                FarClip = FarClip,
                ScreenSize = new Vector2(renderTexture.Width, renderTexture.Height),
            };
        }

        // Mutators
        public void SetClearColor(Vector4 color) => ClearColor = color;

        public void SetProjectionMatrix(Matrix4x4 proj) => _projectionMatrix = proj;
        public Matrix4x4 GetProjectionMatrix() => _projectionMatrix;

        public void SetViewMatrix(Matrix4x4 view) => _viewMatrix = view;
        public Matrix4x4 GetViewMatrix() => _viewMatrix;

        public Matrix4x4 GetInverseProjectionMatrix() => _invProjectionMatrix;
        public Matrix4x4 GetInverseViewMatrix() => _invViewMatrix;
        public Matrix4x4 GetInverseViewProjectionMatrix() => _invViewProjectionMatrix;

        public void UpdateProjectionMatrix(float aspectRatio)
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FovY, aspectRatio, NearClip, FarClip);
            _projectionMatrix = Renderer.GraphicsDevice.AdjustProjectionMatrix(_projectionMatrix);

            Matrix4x4.Invert(_projectionMatrix, out _invProjectionMatrix);

            Frustum = Frustum.FromMatrix(_viewMatrix * _projectionMatrix);
        }

        public void UpdateView(Vector3 position, Vector3 front, Vector3 up)
        {
            Position = position;
            Front = Vector3.Normalize(front);
            Up = Vector3.Normalize(up);
            Right = Vector3.Normalize(Vector3.Cross(Up, Front));

            _viewMatrix = Matrix4x4.CreateLookAt(Position, Position + Front, Up);

            Matrix4x4.Invert(_viewMatrix, out _invViewMatrix);
            Matrix4x4.Invert(_viewMatrix * _projectionMatrix, out _invViewProjectionMatrix);

            //Frustum = Frustum.FromMatrix(_projectionMatrix * _viewMatrix);
            Frustum = Frustum.FromMatrix(_viewMatrix * _projectionMatrix);
        }

        public Vector3 WorldToScreen(Vector3 worldPos, float screenWidth, float screenHeight)
        {
            Matrix4x4 viewProj = _viewMatrix * _projectionMatrix;

            Vector4 clip = Vector4.Transform(new Vector4(worldPos, 1.0f), viewProj);

            float w = clip.W;

            // avoid divide by zero
            float safeW = MathF.Max(MathF.Abs(w), 0.00001f);

            Vector3 ndc = new Vector3(clip.X, clip.Y, clip.Z) / safeW;

            Vector2 screen;
            screen.X = (ndc.X * 0.5f + 0.5f) * screenWidth;
            screen.Y = (1.0f - (ndc.Y * 0.5f + 0.5f)) * screenHeight;

            return new Vector3(screen, w);
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