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

            Engine.Renderer.SkyRenderer.Sky = new HDRISky()
            {
                PanoramaTexture = Asset.Load<Texture>("HDRIs/soil_puresky.hdr")!
            };


            PackedScene cubeOnPlatform = Asset.Load<PackedScene>("models/p2_wall.gltf")!;
            Scene scene = cubeOnPlatform.Instantiate(context.SceneService.SceneDocument?.Scene);
            Engine.Instance.SceneTree.LoadScene(scene);

            scene.SetMode(SceneMode.Play);
            
        }

    }
}
