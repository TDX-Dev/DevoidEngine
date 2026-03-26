using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Scripts;

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

            testRender = new Mesh();
            testRender.SetVertices(Primitives.GetCubeVertex());

            //UIButton button = new UIButton();
            //button.Setup();


            camera = scene.AddGameObject("Camera");
            CameraComponent3D cameraComponent = camera.AddComponent<CameraComponent3D>();
            cameraComponent.IsDefault = true;
            camera.Transform.Position = new System.Numerics.Vector3(4.131f, 3.537f, -4.218f);
            camera.Transform.EulerAngles = new System.Numerics.Vector3(32, -47f, 0);

            GameObject cubeObject = scene.AddGameObject("Cube1");
            cubeObject.AddComponent<MeshRenderer>().AddMesh(testRender);
            var phyInterp1 = cubeObject.AddComponent<PhysicsInterpolationTest>();
            phyInterp1.isPhysicsMovement = false;

            GameObject cubeObject1 = scene.AddGameObject("Cube2");
            cubeObject1.Transform.Position = new System.Numerics.Vector3(0, 0, -1.454f);
            cubeObject1.AddComponent<MeshRenderer>().AddMesh(testRender);
            cubeObject1.AddComponent<PhysicsInterpolationTest>().isPhysicsMovement = true;

            scene.Play(true);

        }

    }
}