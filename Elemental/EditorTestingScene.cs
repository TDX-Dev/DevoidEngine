using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.Rendering;

namespace Elemental
{
    public static class EditorTestingScene
    {
        public static void LoadTestScene(EditorContext context)
        {
            context.SceneService.NewScene();

            PackedScene cubeOnPlatform = Asset.Load<PackedScene>("models/flares_test.gltf")!;
            Scene scene = cubeOnPlatform.Instantiate(context.SceneService.SceneDocument?.Scene);
            Engine.Instance.SceneTree.LoadScene(scene);

            scene.SetMode(SceneMode.Play);
            
        }

    }
}
