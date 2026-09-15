using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Tools.Panels
{
    public class SceneViewPanel : ViewportPanel
    {
        public EditorCamera EditorViewCamera;

        public SceneViewPanel() : base("Scene View")
        {
            EditorViewCamera = new EditorCamera();

            this.Viewport.CameraOverride = EditorViewCamera.Camera;
        }
    }
}
