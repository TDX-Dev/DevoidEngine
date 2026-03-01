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
        FileReloader reloader;
        string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/platform_test.fbx";
        public override void OnInit()
        {
            LoadDCC();

            Mesh mesh = new Mesh();
            mesh.SetVertices(Primitives.GetCubeVertex());


            reloader = new FileReloader(levelPath, () =>
            {
                Console.WriteLine("FBX changed. Reloading level...");
                LoadLevel();
            });
            LoadLevel();

        }

        void LoadLevel()
        {
            this.scene = new Scene();
            SceneManager.LoadScene(scene);
            loader.CurrentScene = scene;

            Importer.LoadModel(levelPath);
            scene.Play();
        }


        int mode = 0;
        int mode1 = DebugRenderSystem.AllowDebugDraw ? 1 : 0;

        public override void OnUpdate(float delta)
        {
            reloader?.Consume();

            if (Input.GetKeyDown(Keys.P))
            {
                mode1 = mode1 == 0 ? 1 : 0;
                DebugRenderSystem.AllowDebugDraw = mode1 == 1 ? true : false;
            }

            if (Input.GetKeyDown(Keys.R))
            {
                mode = mode == 0 ? 1 : 0;
                if (mode == 1)
                {
                    ((ForwardRenderTechnique)RenderBase.ActiveRenderTechnique).renderStateOverride = new RenderState()
                    {
                        FillMode = DevoidGPU.FillMode.Solid,
                        BlendMode = DevoidGPU.BlendMode.AlphaBlend
                    };
                }
                else
                {
                    ((ForwardRenderTechnique)RenderBase.ActiveRenderTechnique).renderStateOverride = new RenderState()
                    {
                        FillMode = DevoidGPU.FillMode.Wireframe,
                        BlendMode = DevoidGPU.BlendMode.AlphaBlend,
                    };
                }
            }
        }

        public void LoadDCC()
        {
            LevelSpawnRegistry.Register("Player_Flycam", (assimpNode, assimpScene) =>
            {
                Cursor.SetCursorState(CursorState.Grabbed);

                Console.WriteLine("Added freecam");
                GameObject camera = scene.addGameObject("Camera");

                camera.transform.Position = Importer.GetTransform(assimpNode).Item1;

                camera.AddComponent<FreeCameraComponent>();
                var camComponent = camera.AddComponent<CameraComponent3D>();
                camComponent.IsDefault = true;

            });


            LevelSpawnRegistry.Register("Model", (assimpNode, assimpScene) =>
            {
                var go = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(go, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                go.AddComponent<MeshRenderer>().AddMesh(mesh);
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

                rb.Material = new PhysicsMaterial()
                {
                    Friction = 2f
                };

                Console.WriteLine("Collideable Added");
            });


            LevelSpawnRegistry.Register("Player", (assimpNode, assimpScene) =>
            {


                GameObject player = scene.addGameObject("Player");
                player.transform.Position = Importer.GetTransform(assimpNode).Item1;
                //Importer.ApplyTransform(player, assimpNode);

                //player.transform.Position = new Vector3(0, 1.5f, -6f);

                var playerBody = player.AddComponent<RigidBodyComponent>();
                playerBody.Mass = 1000;
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
                    AngularDamping = 10f,
                    LinearDamping = 1
                };

                FPSController playerController = player.AddComponent<FPSController>();
                playerController.MoveSpeed = 10f;
                playerController.JumpForce = 7f;
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

            LevelSpawnRegistry.Register("Collideable_Dynamic", (assimpNode, assimpScene) =>
            {
                var go = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(go, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                go.AddComponent<MeshRenderer>().AddMesh(mesh);

                var rb = go.AddComponent<RigidBodyComponent>();
                rb.Mass = 100;

                rb.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = Importer.GetTransform(assimpNode).Item3 * 2,
                    //Height = 2,
                    //Radius = 2
                };

                rb.Material = new PhysicsMaterial()
                {
                    Friction = 2f
                };

                Console.WriteLine("Collideable Added");
            });

            LevelSpawnRegistry.Register("Door_Hinged", (assimpNode, assimpScene) =>
            {
                // 1️⃣ Create hinge pivot
                GameObject hinge = scene.addGameObject(assimpNode.Name + "_Hinge");
                Importer.ApplyTransform(hinge, assimpNode);

                Vector3 size = Importer.GetTransform(assimpNode).Item3 * 2;
                float halfWidth = size.Z * 0.25f; // adjust axis if needed

                // 🔥 Move hinge to door edge (important)
                hinge.transform.Position -=
                    Vector3.Transform(new Vector3(0f, 0f, halfWidth), hinge.transform.Rotation);

                // 2️⃣ Create door body
                GameObject door = scene.addGameObject(assimpNode.Name + "_Body");
                door.SetParent(hinge, false);

                // Door sits centered relative to hinge
                door.transform.LocalPosition = new Vector3(0f, 0f, halfWidth);

                // 3️⃣ Collider on door
                var rb = door.AddComponent<RigidBodyComponent>();
                rb.StartKinematic = true;
                rb.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = size
                };

                rb.Material = new PhysicsMaterial()
                {
                    Friction = 1f,
                    Restitution = 0f
                };

                // 4️⃣ Mesh on door
                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                door.AddComponent<MeshRenderer>().AddMesh(mesh);

                // 5️⃣ Rotate hinge only
                hinge.AddComponent<DoorComponent>();
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

                collider.Material = new PhysicsMaterial()
                {
                    Restitution = 0,
                    Friction = 1f
                };


                //var buttonComp = button.AddComponent<PortalButtonComponent>();

                //buttonComp.OnPressed += () =>
                //{
                //    scene.GetComponentsOfType<DoorComponent>()[0].Turn();
                //};

                //buttonComp.OnReleased += () =>
                //{
                //    scene.GetComponentsOfType<DoorComponent>()[0].Turn();
                //};
            });

            LevelSpawnRegistry.Register("Trigger_Button1", (assimpNode, assimpScene) =>
            {
                GameObject button = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(button, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                button.AddComponent<MeshRenderer>().AddMesh(mesh);

                var collider = button.AddComponent<RigidBodyComponent>();
                collider.StartKinematic = true;
                collider.AllowSleep = false;
                collider.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = new Vector3(3, 3, 1)
                };

                collider.Material = new PhysicsMaterial()
                {
                    Restitution = 0,
                    Friction = 1f
                };


                var buttonComp = button.AddComponent<PortalButtonComponent>();

                buttonComp.OnPressed += () =>
                {
                    scene.GetComponentsOfType<DoorComponent>()[0].Turn(true);
                };

                buttonComp.OnReleased += () =>
                {
                    scene.GetComponentsOfType<DoorComponent>()[0].Turn(false);
                };
            });

            LevelSpawnRegistry.Register("Interactable_Cube", (assimpNode, assimpScene) =>
            {
                var go = scene.addGameObject(assimpNode.Name);

                Importer.ApplyTransform(go, assimpNode);

                var mesh = Importer.ConvertMesh(assimpNode, assimpScene);
                go.AddComponent<MeshRenderer>().AddMesh(mesh);

                var rb = go.AddComponent<RigidBodyComponent>();
                rb.Mass = 10f;
                rb.Shape = new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = Importer.GetTransform(assimpNode).Item3 * 2
                };

                rb.Material = new PhysicsMaterial()
                {
                    Friction = 2f,
                    Restitution = 0f
                };

                go.AddComponent<PortalCubeComponent>();
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
