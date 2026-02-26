using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Core
{

    public class Camera
    {
        public Framebuffer RenderTarget { get; set; }
        public Frustum Frustum { get; set; }

        public Vector3 Position { get; private set; } = new Vector3(0, 0, -5);
        public Vector3 Front { get; private set; } = Vector3.UnitZ;
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; } = Vector3.UnitX;

        public float NearClip { get; set; } = 0.1f;
        public float FarClip { get; set; } = 1000f;
        public float FovY { get; set; } = MathF.PI / 3f; // default 60°
        public Vector4 ClearColor { get; private set; } = Vector4.One;

        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;



        // --- Data for GPU ---
        public CameraData GetCameraData()
        {
            Matrix4x4.Invert(_projectionMatrix, out Matrix4x4 invProjection);

            return new CameraData
            {
                View = _viewMatrix,
                Projection = _projectionMatrix,
                InverseProjection = invProjection,
                Position = Position,
                NearClip = NearClip,
                FarClip = FarClip,
                ScreenSize = new Vector2(Renderer.Width, Renderer.Height)
            };
        }

        // --- Mutators ---
        public void SetClearColor(Vector4 color) => ClearColor = color;

        public void SetProjectionMatrix(Matrix4x4 proj) => _projectionMatrix = proj;
        public Matrix4x4 GetProjectionMatrix() => _projectionMatrix;

        public void SetViewMatrix(Matrix4x4 view) => _viewMatrix = view;
        public Matrix4x4 GetViewMatrix() => _viewMatrix;

        public void UpdateProjectionMatrix(float aspectRatio)
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FovY, aspectRatio, NearClip, FarClip);
            _projectionMatrix = Renderer.graphicsDevice.AdjustProjectionMatrix(_projectionMatrix);
            Frustum = Frustum.FromMatrix(_viewMatrix * _projectionMatrix);
        }

        public void UpdateView(Vector3 position, Vector3 front, Vector3 up)
        {
            Position = position;
            Front = Vector3.Normalize(front);
            Up = Vector3.Normalize(up);
            Right = Vector3.Normalize(Vector3.Cross(Front, Up));

            _viewMatrix = Matrix4x4.CreateLookAt(Position, Position + Front, Up);

            Frustum = Frustum.FromMatrix(_viewMatrix * _projectionMatrix);
        }
    }
}
