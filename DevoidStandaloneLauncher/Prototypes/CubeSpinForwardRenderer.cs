using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class CubeSpinForwardRenderer : Prototype
    {
        Scene scene;
        GameObject camera;
        CanvasComponent Canvas;

        public override void OnInit(Scene main)
        {
            this.scene = main;
            Mesh mesh = new Mesh();
            mesh.SetVertices(Primitives.GetCubeVertex());


            // ===============================
            // PLAYER ROOT (Capsule Physics)
            // ===============================

            GameObject player = scene.addGameObject("Player");

            // Ground top = 0.5 (height 1 centered at 0)
            // Capsule half total height = 1.5
            // So center should be 2.0
            player.transform.Position = new Vector3(0, 2.0f, 0);

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

            var fps = player.AddComponent<FPSController>();
            fps.MoveSpeed = 20f;
            fps.JumpForce = 5;
            fps.MouseSensitivity = 0.15f;

            // ===============================
            // CAMERA PIVOT (Pitch Only)
            // ===============================

            GameObject cameraPivot = scene.addGameObject("CameraPivot");
            cameraPivot.AddComponent<MeshRenderer>().AddMesh(mesh);
            cameraPivot.SetParent(player, false);

            cameraPivot.transform.LocalPosition = new Vector3(0, 1.4f, 0);

            // ===============================
            // CAMERA
            // ===============================

            camera = scene.addGameObject("Camera");
            camera.SetParent(cameraPivot, false);
            //camera.transform.LocalPosition = new Vector3(0, 2, -20);

            var camComponent = camera.AddComponent<CameraComponent3D>();
            camComponent.IsDefault = true;

            // ===============================
            // LIGHT
            // ===============================

            GameObject light1 = scene.addGameObject("Light");
            var lightComponent1 = light1.AddComponent<LightComponent>();
            lightComponent1.Intensity = 400;
            lightComponent1.Color = new Vector4(1, 1, 1, 1);
            lightComponent1.Radius = 100;
            light1.transform.Position = new Vector3(20, 20, 20);

            GameObject light2 = scene.addGameObject("Light");
            var lightComponent2 = light2.AddComponent<LightComponent>();
            lightComponent2.Intensity = 400;
            lightComponent2.Color = new Vector4(1, 1, 1, 1);
            light2.transform.Position = new Vector3(20, 20, -20);

            GameObject light3 = scene.addGameObject("Light");
            var lightComponent3 = light3.AddComponent<LightComponent>();
            lightComponent3.Intensity = 400;
            lightComponent3.Color = new Vector4(1, 1, 1, 1);
            light3.transform.Position = new Vector3(-20, 20, 20);

            GameObject light4 = scene.addGameObject("Light");
            var lightComponent4 = light4.AddComponent<LightComponent>();
            lightComponent4.Intensity = 400;
            lightComponent4.Color = new Vector4(1, 1, 1, 1);
            light4.transform.Position = new Vector3(-20, 20, -20);


            // ===============================
            // GROUND
            // ===============================

            GameObject ground = scene.addGameObject("Ground");
            ground.transform.Position = new Vector3(0, 0, 0);
            ground.transform.Scale = new Vector3(100, 1, 100);

            var groundRenderer = ground.AddComponent<MeshRenderer>();
            groundRenderer.AddMesh(mesh);

            var groundCollider = ground.AddComponent<StaticCollider>();
            groundCollider.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = new Vector3(100, 1, 100)
            };

            groundCollider.Material = new PhysicsMaterial()
            {
                Friction = 1f
            };

            //GameObject camera = scene.addGameObject("Camera");
            //camera.transform.LocalPosition = new Vector3(0, 10, -20);
            //var camComponent = camera.AddComponent<CameraComponent3D>();
            //camComponent.IsDefault = true;

            //camera.AddComponent<FreeCameraComponent>();

            GameObject spawner = scene.addGameObject("EnemySpawner");
            EnemySpawner spawnerComp = spawner.AddComponent<EnemySpawner>();



            Vector3 enemyScale = new Vector3(1, 4, 1);

            GameObject enemy = scene.addGameObject("Enemy");
            enemy.transform.Position = new Vector3(0, 5, 10);
            enemy.transform.Scale = enemyScale;

            enemy.AddComponent<MeshRenderer>().AddMesh(mesh);

            RigidBodyComponent rb = enemy.AddComponent<RigidBodyComponent>();

            rb.FreezeRotationX = true;
            rb.FreezeRotationZ = true;

            rb.Shape =
                new PhysicsShapeDescription()
                {
                    Type = PhysicsShapeType.Box,
                    Size = enemyScale
                };

            rb.Mass = 100;
            rb.Material = new PhysicsMaterial()
            {
                Friction = 2f,
                Restitution = 0.1f
            };

            Enemy enemyComp = enemy.AddComponent<Enemy>();
            spawnerComp.OnDeath += () =>
            {
                scoreLabel.Text = "Score: " + ++score;
            };

            GameObject canvasObject = scene.addGameObject("Canvas");
            Canvas = canvasObject.AddComponent<CanvasComponent>();

            SetupUI();
        }

        int score = 0;

        LabelNode renderModeLabel;
        LabelNode ammoLabel;
        LabelNode healthLabel;
        LabelNode scoreLabel;

        void SetupUI()
        {
            FontInternal font = FontLibrary.LoadFont("Engine/Content/Fonts/JetBrainsMono-Regular.ttf", 32);
            //FontInternal font = FontLibrary.LoadFont("C:/Windows/Fonts/HARLOWSI.ttf", 32);

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

            FlexboxNode crossHair = new FlexboxNode()
            {
                
            };

            crossHair.Add(new BoxNode()
            {
                Size = new Vector2(30, 30),
                Texture = Helper.loadImageAsTex("Engine/Content/Textures/crosshair.png", DevoidGPU.TextureFilter.Linear)
            });

            Canvas.Canvas.Add(crossHair);
            Canvas.Canvas.Add(mainContainer);
        }

        int mode = 0;
        public override void OnUpdate(float delta)
        {
            if (Input.GetKeyDown(Keys.R))
            {
                renderModeLabel.Text = mode == 0 ? "RenderMode: Solid" : "RenderMode: Wireframe";
                mode = mode == 0 ? 1 : 0;
                if (mode == 1)
                {
                    ((ForwardRenderTechnique)RenderBase.ActiveRenderTechnique).renderStateOverride = new RenderState()
                    {
                        FillMode = DevoidGPU.FillMode.Solid
                    };
                } else
                {
                    ((ForwardRenderTechnique)RenderBase.ActiveRenderTechnique).renderStateOverride = new RenderState()
                    {
                        FillMode = DevoidGPU.FillMode.Wireframe
                    };
                }
            }

            // Nothing needed here now.
            // FPSController handles input + movement.
        }
    }
}
