using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Utils;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class MeshLoaderPrototype : Prototype
    {
        Scene scene;

        public override void OnInit()
        {

            this.scene = new Scene();
            SceneManager.LoadScene(scene);
            loader.CurrentScene = scene;

            Mesh mesh = new Mesh();
            mesh.SetVertices(Primitives.GetCubeVertex());

            LoadDCC();
            Importer.LoadModel("LauncherContents/platform_test.fbx");


            scene.Play();
        }

        public override void OnUpdate(float delta)
        {
            
        }

        public void LoadDCC()
        {
            LevelSpawnRegistry.Register("Player", (assimpNode, assimpScene) =>
            {


                GameObject player = scene.addGameObject("Player");
                player.transform.Position = Importer.GetTransform(assimpNode).Item1;
                //Importer.ApplyTransform(player, assimpNode);

                //player.transform.Position = new Vector3(0, 1.5f, -6f);

                var playerBody = player.AddComponent<RigidBodyComponent>();
                playerBody.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Capsule,
                    Height = 2f,
                    Radius = 0.5f
                };

                playerBody.Material = new PhysicsMaterial()
                {
                    Friction = 0.8f,
                    Restitution = 0f,
                    AngularDamping = 10f
                };

                FPSController playerController = player.AddComponent<FPSController>();
                playerController.MoveSpeed = 20f;
                playerController.JumpForce = 11f;
                playerController.MouseSensitivity = 0.15f;

                Cursor.SetCursorState(CursorState.Grabbed);

                GameObject cameraPivot = scene.addGameObject("CameraPivot");
                cameraPivot.SetParent(player, false);
                playerController.SetCameraPivot(cameraPivot.transform);

                cameraPivot.transform.LocalPosition = new Vector3(0, 1.4f, 0);

                GameObject camera = scene.addGameObject("Camera");
                camera.SetParent(cameraPivot, false);
                var camComponent = camera.AddComponent<CameraComponent3D>();
                camComponent.IsDefault = true;

            });

            LevelSpawnRegistry.Register("Collideable_Static", (assimpNode, assimpScene) =>
            {
                var go = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(go, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                go.AddComponent<MeshRenderer>().AddMesh(mesh);

                var rb = go.AddComponent<StaticCollider>();

                rb.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = Importer.GetTransform(assimpNode).Item3 * 2
                };

                Console.WriteLine("Collideable Added");
            });

            LevelSpawnRegistry.Register("Door_Hinged", (assimpNode, assimpScene) =>
            {
                // 1. Create hinge root
                GameObject hinge = scene.addGameObject(assimpNode.Name + "_Hinge");

                // Apply DCC transform to hinge
                Importer.ApplyTransform(hinge, assimpNode);

                // 2. Add kinematic rigidbody to hinge
                var hingeBody = hinge.AddComponent<RigidBodyComponent>();
                hingeBody.StartKinematic = true;

                hingeBody.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = Importer.GetTransform(assimpNode).Item3 * 2
                };

                hingeBody.Material = new PhysicsMaterial()
                {
                    Friction = 1f,
                    Restitution = 0f
                };

                // 3. Convert imported mesh
                var doorMeshAsset = Importer.ConvertMesh(assimpNode, assimpScene);

                // 4. Create visible door mesh object
                GameObject doorMeshGO = scene.addGameObject(assimpNode.Name + "_Mesh");
                doorMeshGO.SetParent(hinge, false);

                doorMeshGO.transform.LocalPosition = new Vector3(0f, 0f, 0.5f);

                doorMeshGO.AddComponent<MeshRenderer>().AddMesh(doorMeshAsset);

                // 5. Add door behavior to hinge
                hinge.AddComponent<DoorComponent>();
            });

            LevelSpawnRegistry.Register("Model", (assimpNode, assimpScene) =>
            {
                Console.WriteLine("Text");
                var go = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(go, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                go.AddComponent<MeshRenderer>().AddMesh(mesh);
            });

            LevelSpawnRegistry.Register("Trigger_Button", (assimpNode, assimpScene) =>
            {
                GameObject button = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(button, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                button.AddComponent<MeshRenderer>().AddMesh(mesh);

                var collider = button.AddComponent<StaticCollider>();
                collider.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = Importer.GetTransform(assimpNode).Item3 * 2
                };


                var buttonComp = button.AddComponent<PortalButtonComponent>();

                buttonComp.OnPressed += () =>
                {
                    Console.WriteLine("Button Pressed");
                };

                buttonComp.OnReleased += () =>
                {
                    Console.WriteLine("Button Released");
                };
            });

            LevelSpawnRegistry.RegisterLight((assimpNode, assimpLight) =>
            {
                GameObject lightGO = scene.addGameObject(assimpNode.Name);

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

                lightComponent.Color = new Vector4(
                    assimpLight.ColorDiffuse.R,
                    assimpLight.ColorDiffuse.G,
                    assimpLight.ColorDiffuse.B,
                    1f);

                lightComponent.Radius = 200f;
                lightComponent.Intensity = 20f; // your scale
            });
        }
    }
}
