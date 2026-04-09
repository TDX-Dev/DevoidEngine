using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Serialization;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
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
                    gameObject.Scene.SetMainCamera3D(this);
                }
            }
        }

        public Camera Camera { get; private set; }

        public bool isDefault;
        private int width;
        private int height;

        public CameraComponent3D()
        {
            Camera = new Camera();

            Camera.RenderTarget = new Framebuffer();

            width = (int)Screen.Size.X;
            height = (int)Screen.Size.Y;
            CreateRenderTarget(width, height);

            UpdateProjection();
        }

        private void CreateRenderTarget(int w, int h)
        {
            Camera.RenderTarget = new Framebuffer();

            Camera.RenderTarget.AttachRenderTexture(new Texture2D(new TextureDescription
            {
                Width = w,
                Height = h,
                Format = TextureFormat.RGBA16_Float,
                IsRenderTarget = true,
                GenerateMipmaps = false,
                MipLevels = 1
            }));

            Camera.RenderTarget.AttachDepthTexture(new Texture2D(new TextureDescription
            {
                Width = w,
                Height = h,
                Format = TextureFormat.Depth24_Stencil8,
                IsDepthStencil = true,
                GenerateMipmaps = false,
                MipLevels = 1
            }));

            Console.WriteLine("Created Render Target with dimensions: " + new Vector2(w, h));
        }

        public override void OnStart()
        {

            UpdateProjection();
            gameObject.Scene.AddCamera3D(this);

            if (isDefault)
                gameObject.Scene.SetMainCamera3D(this);

            //if (IsDefault) gameObject.Scene.SetMainCamera(this);
        }

        public override void OnUpdate(float dt)
        {

            var transform = gameObject.Transform;
            if (!transform.hasMoved && IsInitialized)
                return;

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
            gameObject.Scene.RemoveCamera3D(this);
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
            if (newWidth == width && newHeight == height)
                return;

            width = newWidth;
            height = newHeight;

            Camera.RenderTarget!.Resize(width, height);

            UpdateProjection();

            Console.WriteLine("Camera Resize Call: " + new Vector2(width, height));
        }

        private void UpdateProjection()
        {
            float aspectRatio = (float)width / height;
            Camera.UpdateProjectionMatrix(aspectRatio);
        }
    }
}