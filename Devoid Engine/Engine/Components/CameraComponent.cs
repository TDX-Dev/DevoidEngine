using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class CameraComponent3D : Component
    {
        public override string Type => nameof(CameraComponent3D);

        public bool IsDefault
        {
            get => isDefault; set
            {
                isDefault = value;
                if (value == true)
                {
                    gameObject.Scene.SetMainCamera(this);
                }
            }
        }

        public Camera Camera { get; private set; }

        private bool isDefault;
        private int width = 800;
        private int height = 600;

        public CameraComponent3D()
        {
            Camera = new Camera();
        }

        public override void OnStart()
        {
            IsDefault = true;
            Camera.RenderTarget = new Framebuffer();

            Camera.RenderTarget.AttachRenderTexture(new Texture2D(new DevoidGPU.TextureDescription()
            {
                Width = (int)Screen.Size.X,
                Height = (int)Screen.Size.Y,
                Format = DevoidGPU.TextureFormat.RGBA16_Float,
                GenerateMipmaps = false,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false,
            }));

            Camera.RenderTarget.AttachDepthTexture(new Texture2D(new DevoidGPU.TextureDescription()
            {
                Width = (int)Screen.Size.X,
                Height = (int)Screen.Size.Y,
                Format = DevoidGPU.TextureFormat.Depth24_Stencil8,
                GenerateMipmaps = false,
                IsDepthStencil = true,
                IsRenderTarget = false,
                IsMutable = false
            }));

            UpdateProjection();


            //gameObject.Scene.AddCamera(this);
            //if (IsDefault) gameObject.Scene.SetMainCamera(this);
        }

        public override void OnLateUpdate(float dt)
        {
            var transform = gameObject.transform;

            Vector3 position = transform.Position;

            Vector3 forward = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitZ, transform.Rotation)
            );

            Vector3 up = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitY, transform.Rotation)
            );

            Camera.UpdateView(position, forward, up);
        }


        public override void OnDestroy()
        {
            gameObject.Scene.RemoveCamera(this);
        }

        // --- API for projection ---
        public float Fov
        {
            get => MathHelper.RadToDeg(Camera.FovY);
            set
            {
                Camera.FovY = MathHelper.DegToRad(Math.Clamp(value, 1f, 179f));
                UpdateProjection();
            }
        }

        public float NearPlane
        {
            get => Camera.NearClip;
            set { Camera.NearClip = value; UpdateProjection(); }
        }

        public float FarPlane
        {
            get => Camera.FarClip;
            set { Camera.FarClip = value; UpdateProjection(); }
        }

        public void SetViewportSize(int newWidth, int newHeight)
        {
            if (newWidth == width && newHeight == height) return;

            width = newWidth;
            height = newHeight;
            Camera.RenderTarget.Resize(width, height);
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            float aspectRatio = (float)width / height;
            Camera.UpdateProjectionMatrix(aspectRatio);
        }
    }
}
