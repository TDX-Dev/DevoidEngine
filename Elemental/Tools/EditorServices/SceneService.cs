using DevoidEngine.Core;
using DevoidEngine.Logging;
using DevoidEngine.Nodes;
using Elemental.Tools.Documents;

namespace Elemental.Tools.EditorServices
{
    public class SceneService : IEditorService
    {
        public event Action<Scene>? OnSceneChanged;

        public SceneDocument? SceneDocument { get; private set; }

        public SceneService()
        {

        }

        public void NewScene()
        {
            if (SceneDocument != null)
            {
                if (SceneDocument.HasFile)
                {
                    DevoidLog.Warning(LogCategory.Editor, "Scene has no file, save work?");
                }
                else if (SceneDocument.HasUnsavedChanges)
                {
                    DevoidLog.Warning(LogCategory.Editor, "Scene had unsaved changes, moving to a new scene will erase work.");
                }
                return;
            }

            SceneDocument = new SceneDocument(new Scene());
            OnSceneChanged?.Invoke(SceneDocument.Scene);
        }

        public void LoadScene(EditorContext context, Scene scene)
        {
            SceneDocument = new SceneDocument(scene);
            OnSceneChanged?.Invoke(SceneDocument.Scene);
        }

        public void SaveScene()
        {
            // for starters lets try saving to the root directory of the project.

            Scene testScene = new();
            Engine.Instance.SceneTree.LoadScene(testScene);

            testScene.CreateNode<Camera3D>("A");
            testScene.CreateNode<Node3D>("B");
            testScene.CreateNode<Node3D>("C");


            ResourceFormatText.Save("scene.dscn", testScene);

        }

    }
}
