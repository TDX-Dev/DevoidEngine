using DevoidEngine.Core;
using DevoidEngine.Logging;

namespace DevoidEngine.Nodes
{
    public class Camera3D : Node3D
    {

        public bool isCurrent = false;
        public readonly Camera camera;

        public Camera3D()
        {
            camera = new Camera();
        }

        protected override void OnTreeEntered()
        {
            DevoidLog.Info(LogCategory.Scene, "Camera entered scene");
        }

        public void MakeCurrent()
        {
            DevoidLog.Info(LogCategory.Scene, $"Made '{Name}' as current camera.");
        }

    }
}
