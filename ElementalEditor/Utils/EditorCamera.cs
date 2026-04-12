using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidGPU;
using System.Numerics;

namespace ElementalEditor.Utils
{
    public class EditorCamera
    {
        public Camera Camera { get; private set; }

        Vector3 position = new(0, 2, 5);
        float pitch;
        float yaw = -90f;

        float fov = 60f;

        public float MouseSensitivity { get; set; } = 0.25f;
        public float MoveSpeed { get; set; } = 20f;

        public int Width { get; private set; }
        public int Height { get; private set; }

        bool rotating;
        bool panning;

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                UpdateView();
            }
        }

        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = Math.Clamp(value, -89f, 89f);
                UpdateView();
            }
        }

        public float Yaw
        {
            get => yaw;
            set
            {
                yaw = value;
                UpdateView();
            }
        }

        public float Fov
        {
            get => fov;
            set
            {
                fov = value;
                UpdateProjection();
            }
        }

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

            Camera.ClearColor = new Vector4(new Vector3(0.1f), 1);

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
            if (rotating)
            {
                Yaw += dx * MouseSensitivity;
                Pitch -= dy * MouseSensitivity;
            }

            if (panning)
            {
                Vector3 right = GetRight();
                Vector3 up = Vector3.UnitY;

                position -= right * dx * 0.02f;
                position += up * dy * 0.02f;

                UpdateView();
            }
        }

        public void Scroll(float delta)
        {
            position += GetForward() * delta;
            UpdateView();
        }

        internal void UpdateView()
        {
            Camera.UpdateView(
                position,
                GetForward(),
                Vector3.UnitY
            );
        }

        void UpdateProjection()
        {
            float aspect = (float)Width / Height;

            Camera.FovY = MathF.PI / 180f * fov;
            Camera.UpdateProjectionMatrix(aspect);
        }

        public Vector3 GetForward()
        {
            float yawRad = MathF.PI / 180f * yaw;
            float pitchRad = MathF.PI / 180f * pitch;

            Vector3 forward;

            forward.X = MathF.Cos(yawRad) * MathF.Cos(pitchRad);
            forward.Y = MathF.Sin(pitchRad);
            forward.Z = MathF.Sin(yawRad) * MathF.Cos(pitchRad);

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