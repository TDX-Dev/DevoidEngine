using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidGPU;
using System.Numerics;

namespace ElementalEditor.Utils
{
    public class EditorCamera
    {
        public Camera Camera { get; private set; }

        public Vector3 Position = new(0, 2, 5);
        public float Pitch;
        public float Yaw = -90f;

        public float MouseSensitivity = 0.25f;
        public float MoveSpeed = 20f;
        public float Fov = 60f;

        public int Width;
        public int Height;

        bool rotating;
        bool panning;

        public EditorCamera(int width, int height)
        {
            Width = width;
            Height = height;

            Camera = new Camera();
            Camera.RenderTarget = new Framebuffer();

            Camera.RenderTarget.AttachRenderTexture(new Texture2D(new TextureDescription
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA16_Float,
                IsRenderTarget = true,
                GenerateMipmaps = false,
                MipLevels = 1
            }));

            Camera.RenderTarget.AttachDepthTexture(new Texture2D(new TextureDescription
            {
                Width = width,
                Height = height,
                Format = TextureFormat.Depth24_Stencil8,
                IsDepthStencil = true,
                GenerateMipmaps = false,
                MipLevels = 1
            }));

            UpdateProjection();
            UpdateView();
        }

        public void SetViewportSize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return;

            if (width == Width && height == Height)
                return;

            Width = width;
            Height = height;

            Camera.RenderTarget.Resize(width, height);

            Renderer.Resize(width, height);

            UpdateProjection();
        }

        public void StartRotate() => rotating = true;
        public void StopRotate() => rotating = false;

        public void StartPan() => panning = true;
        public void StopPan() => panning = false;

        public void MouseDelta(float dx, float dy)
        {
            bool changed = false;

            if (rotating)
            {
                Yaw += dx * MouseSensitivity;
                Pitch -= dy * MouseSensitivity;
                Pitch = Math.Clamp(Pitch, -89f, 89f);
                changed = true;
            }

            if (panning)
            {
                Vector3 right = GetRight();
                Vector3 up = Vector3.UnitY;

                Position -= right * dx * 0.02f;
                Position += up * dy * 0.02f;
                changed = true;
            }

            if (changed)
                UpdateView();
        }

        public void Scroll(float delta)
        {
            Position += GetForward() * delta;
            UpdateView();
        }

        public void UpdateView()
        {
            Camera.UpdateView(
                Position,
                GetForward(),
                Vector3.UnitY
            );
        }

        void UpdateProjection()
        {
            float aspect = (float)Width / Height;

            Camera.FovY = MathF.PI / 180f * Fov;
            Camera.UpdateProjectionMatrix(aspect);

            Console.WriteLine($"{Width}x{Height} aspect {(float)Width / Height}");
        }

        public Vector3 GetForward()
        {
            float yaw = MathF.PI / 180f * Yaw;
            float pitch = MathF.PI / 180f * Pitch;

            Vector3 forward;

            forward.X = MathF.Cos(yaw) * MathF.Cos(pitch);
            forward.Y = MathF.Sin(pitch);
            forward.Z = MathF.Sin(yaw) * MathF.Cos(pitch);

            return Vector3.Normalize(forward);
        }

        public Vector3 GetRight()
        {
            return Vector3.Normalize(Vector3.Cross(GetForward(), Vector3.UnitY));
        }

        public Vector3 GetUp()
        {
            return Vector3.UnitY;
        }
    }
}