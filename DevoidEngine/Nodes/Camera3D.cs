using DevoidEngine.Core;
using DevoidEngine.Logging;
using DevoidEngine.Rendering;
using System.Numerics;

namespace DevoidEngine.Nodes
{
    public class Camera3D : Node3D
    {
        public float Fov
        {
            get => fov;
            set
            {
                fov = Math.Clamp(value, 1f, 179f);
                camera.Fov = fov;
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


        public bool isCurrent = false;
        public readonly Camera camera;

        internal float fov = 60f;
        internal float nearPlane = 0.1f;
        internal float farPlane = 1000f;

        public Camera3D()
        {
            camera = new Camera();
        }

        private Viewport? _viewport;

        protected override void OnTreeEntered()
        {
            base.OnTreeEntered();

            _viewport = GetViewport();
            _viewport?.RegisterCamera(camera);

            DevoidLog.Info(LogCategory.Scene, "Camera entered scene");
        }

        protected override void OnTreeExited()
        {
            _viewport?.UnregisterCamera(camera);
            _viewport = null;

            base.OnTreeExited();
        }

        public void MakeCurrent()
        {
            if (_viewport == null)
                return;

            DevoidLog.Info(LogCategory.Scene, $"Made '{Name}' as current camera.");
            _viewport.SetActiveCamera(camera);
        }

        protected override void OnStart()
        {
            UpdateCameraView(Transform.WorldMatrix);
        }
        protected override void OnRender()
        {

            Matrix4x4 world = Engine.Instance.UseInterpolation ? Transform.GetGlobalTransformInterpolated(
                Engine.Instance.FrameCount,
                Engine.Instance.InterpolationAlpha
            )
            : Transform.WorldMatrix;
            UpdateCameraView(world);
        }

        private void UpdateCameraView(in Matrix4x4 world)
        {
            Vector3 position = world.Translation;

            Vector3 forward = Vector3.Normalize(new Vector3(world.M31, world.M32, world.M33));
            Vector3 up = Vector3.Normalize(new Vector3(world.M21, world.M22, world.M23));

            camera.UpdateView(position, forward, up);
        }


    }
}
