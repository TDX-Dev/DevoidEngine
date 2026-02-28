using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.PrototypeSystems;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class PortalTest : Prototype
    {
        Scene scene;
        GameObject camera;
        CanvasComponent Canvas;
        GameObject player;
        GameObject Monitor;
        FPSController playerController;
        FontInternal font;

        GameObject portalCamera;
        GameObject cameraFollower;

        Mesh mesh;

        public override void OnInit()
        {

            this.scene = new Scene();
            SceneManager.LoadScene(scene);
            loader.CurrentScene = scene;

            mesh = new Mesh();
            //mesh.SetVertices(Primitives.GetIndexedCube());
            //mesh.SetIndices(Primitives.GetCubeIndices());
            mesh.SetVertices(Primitives.GetCubeVertex());

            // ===============================
            // PLAYER ROOT (Capsule Physics)
            // ===============================

            player = scene.addGameObject("Player");

            // Ground top = 0.5 (height 1 centered at 0)
            // Capsule half total height = 1.5
            // So center should be 2.0
            player.transform.Position = new Vector3(0, 1.5f, -6f);

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

            // ===============================
            // FPS CONTROLLER
            // ===============================

            playerController = player.AddComponent<FPSController>();
            playerController.MoveSpeed = 20f;
            playerController.JumpForce = 5;
            playerController.MouseSensitivity = 0.15f;

            playerController.OnDeath += () =>
            {
                Console.WriteLine("Died");
                scene = CreateGameOverScene();
                loader.CurrentScene = scene;
                SceneManager.LoadScene(scene);
                scene.Play();

                Cursor.SetCursorState(CursorState.Normal);
            };

            Cursor.SetCursorState(CursorState.Grabbed);
            // ===============================
            // CAMERA PIVOT (Pitch Only)
            // ===============================

            //GameObject torch = scene.addGameObject("Torch");
            //LightComponent torchLight = torch.AddComponent<LightComponent>();
            //torchLight.Intensity = 10;
            //torchLight.Radius = 10;
            //torchLight.Color = new Vector4(1, 1, 1, 1);

            //torch.SetParent(player, false);

            GameObject cameraPivot = scene.addGameObject("CameraPivot");
            cameraPivot.AddComponent<MeshRenderer>().AddMesh(mesh);
            cameraPivot.SetParent(player, false);
            playerController.SetCameraPivot(cameraPivot.transform);

            cameraPivot.transform.LocalPosition = new Vector3(0, 1.4f, 0);



            GameObject light = scene.addGameObject("RoomLight");
            light.transform.Position = new Vector3(0, 7f, 0);

            var lightComp = light.AddComponent<LightComponent>();
            lightComp.Intensity = 30;
            lightComp.Radius = 30;
            lightComp.Color = new Vector4(1, 1, 1, 1);

            // ===============================
            // CAMERA
            // ===============================

            camera = scene.addGameObject("Camera");
            camera.SetParent(cameraPivot, false);
            //camera.transform.LocalPosition = new Vector3(0, 2, -20);

            //camera.AddComponent<HitMarkerComponent>();
            var camComponent = camera.AddComponent<CameraComponent3D>();
            camComponent.IsDefault = true;


            // ===============================
            // GROUND
            // ===============================

            //GameObject ground = scene.addGameObject("Ground");
            //ground.transform.Position = new Vector3(0, 0, 0);
            //ground.transform.Scale = new Vector3(1000, 1, 1000);

            //var groundRenderer = ground.AddComponent<MeshRenderer>();
            //groundRenderer.AddMesh(mesh);

            //var groundCollider = ground.AddComponent<StaticCollider>();
            //groundCollider.Shape = new PhysicsShapeDescription()
            //{
            //    Type = PhysicsShapeType.Box,
            //    Size = new Vector3(1000, 1, 1000)
            //};

            //groundCollider.Material = new PhysicsMaterial()
            //{
            //    Friction = 1f
            //};

            GameObject canvasObject = scene.addGameObject("Canvas");
            canvasObject.SetParent(Monitor, false);
            canvasObject.transform.LocalPosition = new Vector3(0, 0, 1);
            Canvas = canvasObject.AddComponent<CanvasComponent>();
            Canvas.CameraConstraint = camComponent;
            Canvas.RenderMode = CanvasRenderMode.ScreenSpace;
            Canvas.PixelsPerUnit = 300;


            portalCamera = scene.addGameObject("Portal Camera");
            portalCamera.transform.Position = new Vector3(-3, 3, 0);
            portalCamera.transform.EulerAngles = new Vector3(0, 90, 0);
            CameraComponent3D portalCameraComponent = portalCamera.AddComponent<CameraComponent3D>();


            GameObject portalViewObject = scene.addGameObject("Portal View");
            portalViewObject.transform.Position = new Vector3(5.998f, 3, 0);
            portalViewObject.transform.Scale = new Vector3(1, 3, 3);

            StaticCollider staticCollider = portalViewObject.AddComponent<StaticCollider>();
            staticCollider.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = new Vector3(1, 3, 3)
            };

            MeshRenderer portalMeshRenderer = portalViewObject.AddComponent<MeshRenderer>();

            RenderThread.Enqueue(() =>
            {
                portalMeshRenderer.material = new MaterialInstance(new Material(new Shader("Engine/Content/Shaders/Testing/portal_render")));
                portalMeshRenderer.material.SetTexture("MAT_TEX_OVERRIDE", portalCameraComponent.Camera.RenderTarget.GetRenderTexture(0));
                //portalMeshRenderer.material.SetVector4("Albedo", new Vector4(0, 1, 0, 0));
            });

            portalMeshRenderer.AddMesh(mesh);


            //cameraFollower = scene.addGameObject("Camera Follow Cube");
            //cameraFollower.AddComponent<MeshRenderer>().AddMesh(mesh);

            PChamber1.CreateRoom(scene, new Vector3(0, 4, 0), new Vector3(12, 8, 18));

            SetupDoor();


            SetupUI();
            scene.Play();
        }

        int score = 0;

        LabelNode renderModeLabel;
        LabelNode ammoLabel;
        LabelNode healthLabel;
        LabelNode scoreLabel;

        void SetupDoor()
        {
            GameObject hinge = scene.addGameObject("DoorHinge");

            // Position hinge at the hinge edge (left side of door)
            hinge.transform.Position = new Vector3(-0.5f, 1.5f, 8);
            // -0.5 because your door width is 1

            // Add kinematic rigidbody to hinge (NOT the mesh)
            RigidBodyComponent hingeBody = hinge.AddComponent<RigidBodyComponent>();
            hingeBody.StartKinematic = true;
            hingeBody.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = new Vector3(1, 3, 0.3f)
            };

            hingeBody.Material = new PhysicsMaterial()
            {
                Friction = 1f,
                Restitution = 0f
            };

            // Now create the visible door
            GameObject doorMesh = scene.addGameObject("DoorMesh");
            doorMesh.SetParent(hinge, false);

            // Offset door mesh so its left edge aligns with hinge
            doorMesh.transform.LocalPosition = new Vector3(0.5f, 0f, 0f);

            doorMesh.transform.Scale = new Vector3(1, 3, 0.3f);

            doorMesh.AddComponent<MeshRenderer>().AddMesh(mesh);

            // Add your DoorComponent to hinge (not mesh)
            hinge.AddComponent<DoorComponent>();
        }

        void SetupUI()
        {
            font = FontLibrary.LoadFont("Engine/Content/Fonts/JetBrainsMono-Regular.ttf", 32);
            //font = FontLibrary.LoadFont("C:/Windows/Fonts/HARLOWSI.ttf", 32);

            FlexboxNode headerContainer = new FlexboxNode()
            {
                Size = new Vector2(50, 50),

                Direction = FlexDirection.Column,
                Align = AlignItems.Center,
                Justify = JustifyContent.Center,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };


            //BoxNode headerBox = new BoxNode()
            //{
            //    Size = new Vector2(50, 50),
            //    Layout = new LayoutOptions()
            //    {
            //        FlexGrowMain = 1
            //    }
            //};


            renderModeLabel = new LabelNode("RenderMode: Solid", font, 20f)
            {
                Layout = new LayoutOptions() { FlexGrowMain = 0 }
            };

            ammoLabel = new LabelNode("Ammo: 10/10", font, 30f)
            {
                Layout = new LayoutOptions() { FlexGrowMain = 0 }
            };

            healthLabel = new LabelNode("Health: 100/100", font, 30f)
            {
                Layout = new LayoutOptions() { FlexGrowMain = 0 }
            };

            scoreLabel = new LabelNode($"Score: {score}", font, 30f)
            {
                Layout = new LayoutOptions() { FlexGrowMain = 0 }
            };

            //FlexboxNode innerFlex = new FlexboxNode()
            //{
            //    Direction = FlexDirection.Column,
            //    Align = AlignItems.End,
            //    Justify = JustifyContent.End,
            //    Layout = new LayoutOptions()
            //    {
            //        FlexGrowMain = 0
            //    }
            //};

            FlexboxNode mainContainer = new FlexboxNode()
            {
                Size = new Vector2(200, 300),

                Offset = new Vector2(10, 10),
                ParticipatesInLayout = false,

                Direction = FlexDirection.Column,
                Align = AlignItems.Stretch,
                Justify = JustifyContent.Start,
                Gap = 10
            };

            mainContainer.Add(renderModeLabel);
            mainContainer.Add(ammoLabel);
            mainContainer.Add(healthLabel);
            mainContainer.Add(scoreLabel);

            //FlexboxNode crossHair = new FlexboxNode()
            //{

            //};

            //crossHair.Add(new BoxNode()
            //{ 
            //    Size = new Vector2(30, 30),
            //    Texture = Helper.loadImageAsTex("Engine/Content/Textures/crosshair.png", DevoidGPU.TextureFilter.Linear)
            //});

            //Canvas.Canvas.Add(crossHair);
            Canvas.Canvas.Add(mainContainer);
        }

        int mode = 0;
        float pos = 0;
        public override void OnUpdate(float delta)
        {
            //cameraFollower.transform.Position = camera.transform.Position + camera.transform.Forward * 2f;

            //Monitor.transform.Position = new Vector3(0, pos, 0);
            portalCamera.transform.EulerAngles = new Vector3(0, pos * 5, 0);
            pos += delta;

            healthLabel.Text = "Health: " + Math.Round(playerController.Health) + "/" + playerController.MaxHealth;

            if (playerController.isReloading)
            {
                ammoLabel.Text = "Reloading";
            }
            else
            {
                ammoLabel.Text = $"Ammo: {playerController.currentAmmo}/{playerController.MaxAmmo}";
            }

            if (Input.GetKeyDown(Keys.R))
            {
                renderModeLabel.Text = mode == 0 ? "RenderMode: Solid" : "RenderMode: Wireframe";
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

            // Nothing needed here now.
            // FPSController handles input + movement.
        }

        public Scene CreateGameOverScene()
        {
            Scene scene = new Scene();
            CameraComponent3D camera = scene.addGameObject("Camera").AddComponent<CameraComponent3D>();
            camera.IsDefault = true;

            CanvasComponent canvas = scene.addGameObject("GameOverObject").AddComponent<CanvasComponent>();

            LabelNode label = new LabelNode("Game Over!", font, 64)
            {

            };

            canvas.Canvas.Add(label);

            return scene;
        }
    }
}
