using DevoidEngine.Core;
using DevoidEngine.Rendering;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class Camera3D : Component
    {
        public override string Type => nameof(Camera3D);

        public bool IsCurrent
        {
            get => is_current_camera;
            set
            {
                is_current_camera = value;

            }
        }

        public float Fov
        {
            get => fov;
            set
            {
                fov = Math.Clamp(value, 1f, 179f);
                camera.FOV = fov;
            }
        }

        public float NearPlane
        {
            get => nearPlane;
            set
            {
                nearPlane = value;
                camera.Near = nearPlane;
            }
        }

        public float FarPlane
        {
            get => farPlane;
            set
            {
                farPlane = value;
                camera.Far = farPlane;
            }
        }



        private bool is_current_camera;
        private readonly Camera camera;

        internal float fov = 60f;
        internal float nearPlane = 0.1f;
        internal float farPlane = 1000f;

        public Camera3D()
        {
            camera = new Camera();
        }

        public override void OnAttach()
        {
            Viewport viewport = GetTree().RootViewport;

            bool firstCamera = viewport.AddCamera3D(this);
            if (firstCamera || is_current_camera)
                viewport.SetCamera3D(this);
        }

        public override void OnUpdate(float dt)
        {
            var transform = gameObject.Transform;
            //if (!transform.hasMoved && IsInitialized && !dirty)
            //    return;


            Vector3 position = transform.Position;

            Vector3 forward = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitZ, transform.Rotation)
            );

            Vector3 up = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitY, transform.Rotation)
            );

            camera.UpdateView(position, forward, up);
        }

        public Camera GetCamera()
        {
            return camera;
        }
    }
}
