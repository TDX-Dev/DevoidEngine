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


        public override void OnStart()
        {
            Matrix4x4 world = gameObject.Transform.WorldMatrix;

            Vector3 position = world.Translation;

            Vector3 forward = Vector3.Normalize(new Vector3(
                world.M31,
                world.M32,
                world.M33));

            Vector3 up = Vector3.Normalize(new Vector3(
                world.M21,
                world.M22,
                world.M23));

            camera.UpdateView(position, forward, up);
        }

        public override void OnRender()
        {
            if (Engine.Instance.FrameCount == 0)
                return;
            Transform3D transform = gameObject.Transform;

            Matrix4x4 world =
                Engine.Instance.UseInterpolation
                ? transform.GetGlobalTransformInterpolated(
                    Engine.Instance.FrameCount,
                    Engine.Instance.InterpolationAlpha)
                : transform.WorldMatrix;

            Vector3 position = world.Translation;

            Vector3 forward = Vector3.Normalize(new Vector3(
                world.M31,
                world.M32,
                world.M33));

            Vector3 up = Vector3.Normalize(new Vector3(
                world.M21,
                world.M22,
                world.M23));

            camera.UpdateView(position, forward, up);
        }

        public Camera GetCamera()
        {
            return camera;
        }
    }
}
