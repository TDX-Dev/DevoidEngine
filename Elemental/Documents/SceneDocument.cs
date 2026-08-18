using DevoidEngine.Components;
using DevoidEngine.Core;

namespace Elemental.Documents
{
    public sealed class SceneDocument : EditorDocument
    {
        public Scene Scene { get; }
        public override string DisplayName => HasFile ? Path.GetFileNameWithoutExtension(FilePath!) : Scene.SceneName ?? "Untitled";

        public SceneDocument(Scene scene, string? filePath = null) : base(filePath)
        {
            Scene = scene;
        }
    }
}