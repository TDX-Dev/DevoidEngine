using DevoidEngine.Core;
using Elemental.Documents;

namespace Elemental
{
    public class EditorContext
    {
        public GameObject? SelectedObject { get; set; }

        public SceneDocument? ActiveDocument { get; set; }

        public Scene? ActiveScene { get; set; }

        public EditorCamera? EditorCamera { get; set; }

        public bool IsSceneViewFocused { get; set; }
    }
}