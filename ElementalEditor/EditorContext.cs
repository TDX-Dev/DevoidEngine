using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using ElementalEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor
{
    public enum ScenePlayState
    {
        Edit,
        Play,
        Pause
    }

    public class EditorContext
    {
        public Scene Scene;
        public GameObject SelectedObject;

        public EditorCamera EditorCamera = new();

        public Texture2D SceneViewportTarget;

        public ScenePlayState PlayState = ScenePlayState.Edit;
    }
}
