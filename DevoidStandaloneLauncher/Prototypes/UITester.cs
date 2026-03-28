using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Scripts;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class UITester : Prototype
    {
        Scene scene;
        GameObject camera;
        GameObject cubeObject;

        Mesh testRender;

        public override void OnInit()
        {
            Console.WriteLine("Initialized");
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.CurrentScene = scene;

            testRender = new Mesh();
            testRender.SetVertices(Primitives.GetSphereVertices(128, 128, 0.75f));



            camera = scene.AddGameObject("Camera");
            CameraComponent3D cameraComponent = camera.AddComponent<CameraComponent3D>();
            cameraComponent.IsDefault = true;
            camera.Transform.Position = new System.Numerics.Vector3(0, 0, -10);
            //camera.Transform.EulerAngles = new System.Numerics.Vector3(32, -47f, 0);
            //camera.Transform.EulerAngles = new System.Numerics.Vector3(-45, 0, 0);

            //GameObject light = scene.AddGameObject("Light");
            //var lightComp = light.AddComponent<LightComponent>();
            //light.Transform.Position = new System.Numerics.Vector3(0, 10, 0);
            //lightComp.Intensity = 100;
            //lightComp.Radius = 100;

            scene.Play(true);


            int grid = 5;
            float spacing = 2f;

            float half = (grid - 1) / 2f;

            for (int i = 0; i < grid; i++)
            {
                for (int j = 0; j < grid; j++)
                {
                    GameObject sphereObject = scene.AddGameObject($"Sphere_{i}_{j}");

                    sphereObject.Transform.Position = new Vector3(
                        (j - half) * spacing,
                        (half - i) * spacing,
                        0
                    );

                    MeshRenderer mr = sphereObject.AddComponent<MeshRenderer>();

                    float metallic = j / (float)(grid - 1);
                    float roughness = i / (float)(grid - 1);

                    mr.AddMesh(testRender);

                    mr.material.SetFloat("Metallic", metallic);
                    mr.material.SetFloat("Roughness", roughness);
                }
            }


            //cubeObject = scene.AddGameObject("Cube1");
            //cubeObject.AddComponent<MeshRenderer>().AddMesh(testRender);
            //cubeObject.Transform.EulerAngles = new System.Numerics.Vector3(0, 45, 0);
            ////var phyInterp1 = cubeObject.AddComponent<PhysicsInterpolationTest>();
            ////phyInterp1.isPhysicsMovement = false;

            //GameObject cubeObject1 = scene.AddGameObject("Cube2");
            //cubeObject1.Transform.Position = new System.Numerics.Vector3(0, 0, -1.454f);
            //cubeObject1.AddComponent<MeshRenderer>().AddMesh(testRender);
            //cubeObject1.AddComponent<PhysicsInterpolationTest>().isPhysicsMovement = true;

        }

        //public override void OnUpdate(float delta)
        //{
        //    var rot = cubeObject.Transform.EulerAngles;
        //    rot.Y += delta * 15;
        //    cubeObject.Transform.EulerAngles = rot;
        //}

    }
}