using DevoidEngine.Core;

namespace Elemental
{
    public class EditorContext
    {
        public GameObject? SelectedObject { get; set; } = null!;
        public Scene ActiveScene { get; set; } = null!;
        public bool SceneDirty { get; set; }
        public EditorCamera? EditorCamera { get; set; }
    }
}
