using DevoidEngine.Core;
using DevoidEngine.Logging;
using DevoidEngine.Nodes;

namespace DevoidSandbox
{
    public class SandboxLayer : Layer
    {

        public override void OnAttach()
        {
            DevoidLog.Info(LogCategory.External, "Sandbox layer added.");

            DevoidLog.Info(LogCategory.External, "Creating sandbox scene.");


            Scene scene = Engine.Instance.SceneTree.NewScene(true);

            scene.SetMode(SceneMode.Play);

            scene.CreateNode<TickTestNode>();
        }

        public override void OnDetach()
        {

        }
    }
}
