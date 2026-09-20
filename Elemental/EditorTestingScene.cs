using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.Nodes;
using DevoidEngine.Rendering;

namespace Elemental
{
    public static class EditorTestingScene
    {
        public static void LoadTestScene(EditorContext context)
        {
            context.SceneService.NewScene();

            Engine.Renderer.SkyRenderer.Sky = new HDRISky()
            {
                PanoramaTexture = Asset.Load<Texture>("HDRIs/soil_puresky.hdr")!
            };


            PackedScene cubeOnPlatform = Asset.Load<PackedScene>("models/backrooms_light_flare_test.gltf")!;
            Scene scene = cubeOnPlatform.Instantiate(context.SceneService.SceneDocument?.Scene);

            WorldEnvironmentNode wen = scene.CreateNode<WorldEnvironmentNode>();
            Camera3D cameraNode = scene.CreateNode<Camera3D>();

            cameraNode.SetParent(wen);

            Engine.Instance.SceneTree.LoadScene(scene);

            scene.SetMode(SceneMode.Play);
            
        }

    }
}
