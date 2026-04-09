using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor
{
    public class EditorContext
    {
        public Scene Scene;
        public GameObject SelectedObject;

        public CameraComponent3D EditorCamera;

        public Texture2D SceneViewportTarget;
    }
}
