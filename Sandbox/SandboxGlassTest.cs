using DevoidEngine.AssetPipeline;
using DevoidEngine.AssetPipeline.Importers;
using DevoidEngine.Assets;
using DevoidEngine.Audio;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Gizmos.DevoidEngine.Gizmos;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidEngine.Serialization;
using DevoidEngine.UI;
using DevoidEngine.UI.Text;
using DevoidEngine.UI.Theme;
using DevoidEngine.UI.Theme.Styleboxes;
using DevoidEngine.UI.UINodes;
using DevoidEngine.Util;
using DevoidGPU;
using ImGuiNET;
using MessagePack;
using OpenTK.Windowing.Common;
using System.ComponentModel;
using System.Numerics;

namespace Sandbox
{

    internal class SandboxGlassTest : Layer
    {

        Scene scene = null!;

        MaterialInstance PBRMaterial = null!;
        MaterialInstance GroundPBRMaterial = null!;

        readonly Random rand = new();

        public override void OnGUIRender()
        {
            if (ImGui.Begin("Hey!"))
            {

            }
            ImGui.End();

            if (ImGui.Begin("H1!"))
            {

            }
            ImGui.End();
        }

        public override void OnAttach()
        {
            BoundsGizmo transformGizmo = new()
            {

            };

            BoundsGizmo transformGizmo1 = new()
            {

            };

            TranslateGizmo translateGizmo = new() { };

            Engine.Instance.SceneTree.RootViewport.GizmoContext.Gizmos.Add(transformGizmo);
            Engine.Instance.SceneTree.RootViewport.GizmoContext.Gizmos.Add(transformGizmo1);
            Engine.Instance.SceneTree.RootViewport.GizmoContext.Gizmos.Add(translateGizmo);


            Application.MainWindow.OnResize += MainWindow_OnResize;
            Engine.Instance.SceneTree.OnSceneChanged += SceneTree_OnSceneChanged;

            Engine.Instance.SceneTree.RootViewport.Resize(Application.MainWindow.Window.ClientSize.X, Application.MainWindow.Window.ClientSize.Y);

            Console.WriteLine("Sandbox has launched.");

            //if (!loadScene)
            //    return;

            scene = Asset.Load<PackedScene>("models/platform.gltf")!.Instantiate();
            //scene = new Scene();
            //scene = new Scene();
            GC.Collect();
            //scene.GameObjects[0].Transform.Position = new Vector3(0, 5, 0);

            Engine.Instance.SceneTree.LoadScene(scene);
            scene.SetMode(SceneMode.Play);

            //Engine.Instance.AssetDatabase.TryGetGuid("HDRIs/puresky.hdr", out Guid puresky);
            //Engine.Instance.AssetDatabase.Reimport(puresky, MessagePackSerializer.Serialize<TextureImportSettings>(new TextureImportSettings()
            //{
            //    Format = TextureFormat.RGBA16_Float,
            //    GenerateMipmaps = false
            //}));

            Engine.Renderer.SkyRenderer.Sky = new HDRISky()
            {
                PanoramaTexture = Asset.Load<Texture>("HDRIs/soil_puresky.hdr")!
            };


            PBRMaterial = new MaterialInstance(Engine.Renderer.DefaultMaterial);
            GroundPBRMaterial = new MaterialInstance(Engine.Renderer.DefaultMaterial);
            PBRMaterial.SetVector4("Albedo", new Vector4(1, 1, 1, 1));
            GroundPBRMaterial.SetVector4("Albedo", new Vector4(1, 1, 1, 1));

            Texture dvsTex = Asset.Load<Texture>("mesh_tex.png")!;
            //Texture dvsTex = Texture.CreateFromImage2D(TextureUtil.LoadImage("Assets/mesh_tex.png"), TextureUsage.ShaderResource);
            Texture terrTex = Asset.Load<Texture>("ground_tex.png")!;

            //GroundPBRMaterial.SetTexture("MAT_AlbedoMap", terrTex);
            GroundPBRMaterial.SetFloat("Roughness", 1f);
            GroundPBRMaterial.SetFloat("Metallic", 0f);


            PBRMaterial.SetTexture("MAT_AlbedoMap", dvsTex);
            PBRMaterial.SetFloat("Roughness", 0f);

            GameObject target = scene.AddGameObject("CameraTarget");
            target.Transform.Position = Vector3.Zero;

            GameObject cameraObject = scene.AddGameObject("EditorCamera");

            OrbitalCameraController orbit =
                cameraObject.AddComponent<OrbitalCameraController>();

            //orbit.Target = target;
            //orbit.Distance = 5.0f;

            // Movement
            Engine.InputSystem.Map.Bind("Forward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.W
            });

            Engine.InputSystem.Map.Bind("Backward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.S
            });

            Engine.InputSystem.Map.Bind("Left", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.A
            });

            Engine.InputSystem.Map.Bind("Right", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.D
            });

            // Mouse Look
            Engine.InputSystem.Map.Bind("LookX", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaX,
                IsClamped = false
            });

            Engine.InputSystem.Map.Bind("LookY", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaY,
                IsClamped = false
            });

            // Jump
            Engine.InputSystem.Map.Bind("Jump", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.Space
            });

            // Sprint
            Engine.InputSystem.Map.Bind("Sprint", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.LeftShift
            });

            // Toggle Cursor
            Engine.InputSystem.Map.Bind("Grab", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.G
            });

            Engine.InputSystem.Map.Bind("Pickup", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.E,
            });

            Engine.InputSystem.Map.Bind("Orbit", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseButton.Middle,
            });

            Engine.InputSystem.Map.Bind("Zoom", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.ScrollY
            });


            SetupUI();
            //GameObject sketchModel = scene.GetGameObject("Sketchfab_model")!;
            //AudioSource3D audio = sketchModel.AddComponent<AudioSource3D>();
            //audio.Audio = Asset.Load<AudioClip>("Sounds/PortalRadio.wav");
            //audio.PlayOnStart = true;
            //audio.Volume = 0.5f;
            //audio.MaxDistance = 20;
            //audio.SetLooping(true);
            //audio.Play();

            //GameObject sublimModel = scene.GetGameObject("Sublim")!;
            //AudioSource3D audio1 = sublimModel.AddComponent<AudioSource3D>();
            //audio1.Audio = Asset.Load<AudioClip>("Sounds/SBH.wav");
            //audio1.PlayOnStart = true;
            //audio1.Volume = 0.5f;
            //audio1.MaxDistance = 20;
            //audio1.SetLooping(true);
            //audio1.Play();

            //GameObject ballDyn = scene.GetGameObject("Ball:Dynamic")!;
            //ballDyn.RemoveComponent(ballDyn.GetComponent<StaticColliderComponent>()!);
            //RigidBodyComponent rbS = ballDyn.AddComponent<RigidBodyComponent>();
            //rbS.Shape = new DevoidEngine.Physics.PhysicsShapeDescription()
            //{
            //    Type = DevoidEngine.Physics.PhysicsShapeType.Box,
            //    Size = ballDyn.Transform.Scale
            //};

            //AudioSource3D as3d = ballDyn.AddComponent<AudioSource3D>();
            //as3d.Audio = Asset.Load<AudioClip>("Sounds/SBH.wav");
            //as3d.PlayOnStart = true;
            //as3d.Volume = 1f;
            //as3d.MaxDistance = 40;
            //as3d.SetLooping(true);
            //as3d.Play();

            //FollowImpulseComponent fic = ballDyn.AddComponent<FollowImpulseComponent>();
            //fic.FollowTarget = go1;

            //GameObject go = gameObject.Scene.AddGameObject("Debug");
            //MeshRenderer mr = sketchModel.AddComponent<MeshRenderer>();
            //mr.Mesh = PrimitiveMeshes.GetCube();
            //go.SetParent(gameObject);
            //go.Transform.LocalPosition = new Vector3(0, -1, 0);

        }

        LabelNode GPUInfo = null!;
        void SetupUI()
        {
            Viewport viewport = Engine.Instance.SceneTree.RootViewport;

            CanvasNode canvas = new()
            {
                Justify = JustifyContent.Start,
                Align = AlignItems.Start,
                Padding = Padding.GetAll(50)
            };

            viewport.UIContext.Canvases.Add(canvas);

            ContainerNode paddedHealthcontainer = new()
            {
                Size = new Vector2(250, 100),
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 0,
                    FlexGrowMain = 0,
                },
                Direction = FlexDirection.Column,
                Padding = Padding.GetAll(10)
            };

            paddedHealthcontainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0, 0, 0, 0.3f),
                BorderRadius = new Vector4(15),
            });

            ContainerNode Healthcontainer = new()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 1,
                },
                Align = AlignItems.Start,
                Justify = JustifyContent.Start,
                Direction = FlexDirection.Row,
                Padding = Padding.GetAll(10),
            };

            Healthcontainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = Colors.Transparent
            });

            LabelNode healthLabel = new()
            {
                Font = Asset.Load<Font>("tahomabd.ttf")!,
                Text = "HEALTH",
                FontSize = 19
            };

            healthLabel.AddColorOverride(StyleKeys.FontColor, new(1, 0.627f, 0, 0.8f));

            ContainerNode healthValueContainer = new()
            {
                Justify = JustifyContent.Center,
                Align = AlignItems.End,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };
            healthValueContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = Colors.Transparent
            });

            LabelNode healthValueLabel = new()
            {
                Font = Asset.Load<Font>("halflife2.ttf")!,
                Text = "100",
                FontSize = 48,
                VerticalAlignment = TextVerticalAlignment.Bottom
            };

            healthValueLabel.AddColorOverride(StyleKeys.FontColor, new(1, 0.627f, 0, 0.8f));

            Healthcontainer.Add(healthLabel);
            healthValueContainer.Add(healthValueLabel);
            Healthcontainer.Add(healthValueContainer);

            paddedHealthcontainer.Add(Healthcontainer);


            ContainerNode paddedGPUcontainer = new()
            {
                //WidthMode = SizeMode.FitContent,
                //HeightMode = SizeMode.FitContent,
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 0,
                    FlexGrowMain = 0
                },
                Direction = FlexDirection.Column,
                Padding = Padding.GetAll(10)
            };

            paddedGPUcontainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0, 0, 0, 0.3f),
                BorderRadius = new Vector4(15),
            });

            GPUInfo = new()
            {
                Font = Asset.Load<Font>("JBM.ttf")!,
                Text = "N/A",
                FontSize = 16,
                VerticalAlignment = TextVerticalAlignment.Bottom
            };

            paddedGPUcontainer.Add(GPUInfo);

            //canvas.Add(paddedHealthcontainer);
            canvas.Add(paddedGPUcontainer);
            //canvas.Add(labelNode);
            //canvas.Add(labelNode1);

            canvas.Initialize();

        } 


        private void SceneTree_OnSceneChanged(Scene obj)
        {
            Console.WriteLine("Scene changed");
            Application.MainWindow.Window.Title = obj.SceneName + " - Devoid Engine";
        }

        private void MainWindow_OnResize(int width, int height)
        {
            Engine.Instance.SceneTree.RootViewport.Resize(width, height);
        }

        public override void OnDetach()
        {
            Console.WriteLine("Sandbox exited successfully");
        }
        float timer = 0;
        float frameTimeTotal = 0;
        int frameCount = 0;
        public override void OnUpdate(float deltaTime)
        {
            if (Engine.InputSystem.GetActionDown("Grab"))
            {
                if (Engine.Cursor.GetCursorState() == CursorState.Grabbed)
                {
                    Engine.Cursor.SetCursorState(CursorState.Normal);
                }
                else
                {
                    Engine.Cursor.SetCursorState(CursorState.Grabbed);
                }
            }
            timer += deltaTime;
            frameTimeTotal += deltaTime;
            frameCount++;

            if (timer > 1)
            {
                float averageFrameTime = frameTimeTotal / frameCount;
                float averageFPS = 1.0f / averageFrameTime;

                timer = 0;
                frameTimeTotal = 0;
                frameCount = 0;

                GPUInfo.Text =
                    $"GPU: {Engine.Instance.GraphicsDeviceInfo.Name}\n" +
                    $"VRAM: {Engine.Instance.GraphicsDeviceInfo.VideoMemoryUsage / (1024f * 1024f):F1} MB / {Engine.Instance.GraphicsDeviceInfo.DedicatedVideoMemory / (1024f * 1024f):F1} MB\n" +
                    $"VRAM Budget: {Engine.Instance.GraphicsDeviceInfo.VideoMemoryBudget / (1024f * 1024f):F1} MB\n" +
                    $"Frame Time: {averageFrameTime * 1000:F2} ms\n" +
                    $"FPS: {averageFPS:F1}";
            }
        }

        public override void OnRender(ICommandList cmd)
        {

            //cmd.SetViewport(0, 0, Application.MainWindow.Window.Size.X, Application.MainWindow.Window.Size.Y);
            ////cmd.SetPipeline(shader.GetPass("Forward").GetPipeline(Engine.GraphicsDevice, Vertex.VertexInfo));
            ////cmd.SetDescriptorSet(1, set);
            ////mesh.Draw(cmd);

        }

        public override void OnPostRender(ICommandList cmd)
        {

            cmd.SetFramebuffer(Application.MainWindow.Framebuffer);
            cmd.SetViewport(0, 0, Application.MainWindow.Window.ClientSize.X, Application.MainWindow.Window.ClientSize.Y);
            Engine.Renderer.API.RenderToScreen(cmd, Engine.Instance.SceneTree.RootViewport.OutputTexture!);
        }
    }
}
