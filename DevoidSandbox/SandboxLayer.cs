using DevoidEngine.Core;
using DevoidEngine.Logging;
using DevoidEngine.Nodes;
using DevoidEngine.Util;

namespace DevoidSandbox
{
    public class SandboxLayer : Layer
    {

        public override void OnAttach()
        {
            DevoidLog.Info(LogCategory.External, "Sandbox layer added.");

            DevoidLog.Info(LogCategory.External, "Creating sandbox scene.");


            Scene scene = Engine.Instance.SceneTree.LoadScene(new Scene(Engine.Instance.SceneTree));

            scene.SetMode(SceneMode.Play);

            scene.CreateNode<Camera3D>();



            MeshInstance3D meshInstanceNode = scene.CreateNode<MeshInstance3D>();
            meshInstanceNode.Mesh = PrimitiveMeshes.CreateCube();

            Application.MainWindow.OnResize += OnMainWindowResize;
        }

        public override void OnDetach()
        {

        }

        public void OnMainWindowResize(int width, int height)
        {
            Engine.Instance.SceneTree.RootViewport.Resize(width, height);
        }
    }
}
