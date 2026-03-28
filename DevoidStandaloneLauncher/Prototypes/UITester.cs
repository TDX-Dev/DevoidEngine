using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Scripts;
using DevoidStandaloneLauncher.Utils;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class UITester : Prototype
    {
        Scene scene;
        GameObject camera;
        GameObject cubeObject;

        Mesh testRender;

        string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/crt.fbx";

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

            GameObject light = scene.AddGameObject("Light");
            var lightComp = light.AddComponent<LightComponent>();
            light.Transform.Position = new System.Numerics.Vector3(0, 5, -10);
            lightComp.Intensity = 100;
            lightComp.Radius = 100;

            scene.Play(true);


            if (levelPath != "")
            {
                LoadDCC();
                Importer.LoadModel(levelPath);
            }
        }

        void LoadDCC()
        {
            LevelSpawnRegistry.RegisterFallBack((assimpNode, assimpScene) =>
            {
                Console.WriteLine("Loading");
                if (!assimpNode.HasMeshes)
                    return;

                var go = scene.AddGameObject(assimpNode.Name);

                Importer.ApplyTransform(go, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);

                var mr = go.AddComponent<MeshRenderer>();
                mr.AddMesh(mesh);

                var material = Importer.ConvertMaterial(assimpNode, assimpScene, levelPath);
                mr.material = material;
            });



            LevelSpawnRegistry.RegisterLight((assimpNode, assimpLight) =>
            {
                GameObject lightGO = scene.AddGameObject(assimpNode.Name);

                Importer.ApplyTransform(lightGO, assimpNode);

                var lightComponent = lightGO.AddComponent<LightComponent>();

                switch (assimpLight.LightType)
                {
                    case Assimp.LightSourceType.Point:
                        lightComponent.LightType = LightType.PointLight;
                        break;
                    case Assimp.LightSourceType.Directional:
                        lightComponent.LightType = LightType.DirectionalLight;
                        break;
                    case Assimp.LightSourceType.Spot:
                        lightComponent.LightType = LightType.SpotLight;
                        break;
                }

                Vector3 diffuse = new Vector3(assimpLight.ColorDiffuse.X, assimpLight.ColorDiffuse.Y, assimpLight.ColorDiffuse.Z);
                float intensity = MathF.Max(diffuse.X, MathF.Max(diffuse.Y, diffuse.Z));
                Vector3 color = intensity > 0.0f ? diffuse / intensity : Vector3.Zero;

                //lightComponent.Color = new Vector4(assimpLight.ColorDiffuse.R, assimpLight.ColorDiffuse.G, assimpLight.ColorDiffuse.B, 1f);
                lightComponent.Color = new Vector4(color, 1f) * 5;

                lightComponent.Radius = 200f;
                lightComponent.Intensity = intensity * 15; // your scale


                //Console.WriteLine(lightComponent.Intensity);
                //Console.WriteLine(color);

                ////lightComponent.Color = new Vector4(assimpLight.ColorDiffuse.R, assimpLight.ColorDiffuse.G, assimpLight.ColorDiffuse.B, 1f);
                //lightComponent.Color = new Vector4(assimpLight.ColorDiffuse, 1f);

                //lightComponent.Radius = 200f;
                //lightComponent.Intensity = 150f; // your scale



            });
        }


    }
}