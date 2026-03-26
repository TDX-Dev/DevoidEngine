using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class UITester : Prototype
    {
        Scene scene;
        GameObject canvas;
        GameObject camera;

        Mesh testRender;
        Material testRenderMat;
        MaterialInstance testRenderMatInstance;

        public override void OnInit()
        {
            Console.WriteLine("Initialized");
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.CurrentScene = scene;
            scene.Play(true);

            testRender = new Mesh();
            testRender.SetVertices(Primitives.GetCubeVertex());

            //UIButton button = new UIButton();
            //button.Setup();


            camera = scene.AddGameObject("Camera");
            CameraComponent3D cameraComponent = camera.AddComponent<CameraComponent3D>();
            camera.Transform.Position = new System.Numerics.Vector3(0, 0, 0);

            GameObject cubeObject = scene.AddGameObject("Cube1");
            cubeObject.AddComponent<MeshRenderer>().AddMesh(testRender);

        }

    }
}