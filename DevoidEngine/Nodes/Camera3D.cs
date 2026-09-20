using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Nodes
{
    public class Camera3D : Node3D
    {
        public bool IsCurrent
        {
            get => is_current_camera;
            set
            {
                is_current_camera = value;
                if (is_current_camera && Scene != null)
                {
                    Scene.SetMainCamera(this);
                }
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
        internal bool isPostprocessSet = false;

        public Camera3D()
        {
            camera = new Camera();
        }
        protected override void OnAttach()
        {
            if (Scene == null)
                return;
            Scene.RegisterCamera(this);
            Scene.OnNodeAdded += Camera3D_OnNodeAdded;
            Scene.OnNodeRemoved += Camera3D_OnNodeRemoved;

            RegisterPostprocessSettings();
        }

        private void Camera3D_OnNodeRemoved(Node obj)
        {
            Scene.World.RemovePostProcessSettings(camera);
        }

        private void Camera3D_OnNodeAdded(Node obj)
        {
            RegisterPostprocessSettings();
        }
        public void RegisterPostprocessSettings()
        {
            WorldEnvironmentNode? node = Scene!.GetNode<WorldEnvironmentNode>();
            if (node == null)
            {
                Scene.World.RemovePostProcessSettings(camera);
                isPostprocessSet = false;
                Console.WriteLine("DIDNT FIND POSTPROCESS");
                return;
            }
            if (isPostprocessSet)
                return;
            Scene.World.AddPostprocessSettings(camera, node.PostProcessSettings);
            isPostprocessSet = true;
            Console.WriteLine("found postprocess");
        }
        protected override void OnDestroy()
        {
            Scene?.UnregisterCamera(this);
        }

        internal void SetIsCurrentInternal(bool isCurrent)
        {
            is_current_camera = isCurrent;
        }
        protected override void OnStart()
        {
            UpdateCameraView(Transform.WorldMatrix);
        }
        protected override void OnRender()
        {

            Matrix4x4 world =
                Engine.Instance.UseInterpolation
                ? Transform.GetGlobalTransformInterpolated(
                    Engine.Instance.FrameCount,
                    Engine.Instance.InterpolationAlpha)
                : Transform.WorldMatrix;

            UpdateCameraView(world);
        }

        private void UpdateCameraView(in Matrix4x4 world)
        {
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

        public void SetAspectRatio(float aspectRatio)
        {
            camera.UpdateProjectionMatrix(aspectRatio);
        }


    }
}
