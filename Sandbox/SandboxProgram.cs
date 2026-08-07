using DevoidEngine.AssetPipeline;
using DevoidEngine.AssetPipeline.Importers;
using DevoidEngine.Assets;
using DevoidEngine.Audio;
using DevoidEngine.Components;
using DevoidEngine.Core;
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
using MessagePack;
using OpenTK.Windowing.Common;
using System.Numerics;

namespace Sandbox
{

    internal class SandboxProgram : Layer
    {

        Scene scene = null!;

        MaterialInstance PBRMaterial = null!;
        MaterialInstance GroundPBRMaterial = null!;

        readonly Random rand = new();

        public override void OnAttach()
        {
            Application.MainWindow.OnResize += MainWindow_OnResize;
            Engine.Instance.SceneTree.OnSceneChanged += SceneTree_OnSceneChanged;

            Engine.Instance.SceneTree.RootViewport.Resize(Application.MainWindow.Window.ClientSize.X, Application.MainWindow.Window.ClientSize.Y);

            Console.WriteLine("Sandbox has launched.");

            //scene = Asset.Load<PackedScene>("models/spheres_pbr_test.gltf")!.Instantiate();
            scene = Asset.Load<PackedScene>("models/sh.gltf")!.Instantiate();
            //scene = new Scene();
            GC.Collect();
            //scene.GameObjects[0].Transform.Position = new Vector3(0, 5, 0);

            Engine.Instance.SceneTree.LoadScene(scene);
            scene.Play();

            //Engine.Instance.AssetDatabase.TryGetGuid("HDRIs/puresky.hdr", out Guid puresky);
            //Engine.Instance.AssetDatabase.Reimport(puresky, MessagePackSerializer.Serialize<TextureImportSettings>(new TextureImportSettings()
            //{
            //    Format = TextureFormat.RGBA16_Float,
            //    GenerateMipmaps = false
            //}));

            Engine.Renderer.SkyRenderer.Sky = new HDRISky()
            {
                PanoramaTexture = Asset.Load<Texture>("HDRIs/ferndale_studio.hdr")!
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

            GameObject go1 = scene.AddGameObject("Player");
            go1.AddComponent<FirstPersonController>();

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

            //SetupUI();
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

            GameObject ballDyn = scene.GetGameObject("Ball:Dynamic")!;
            ballDyn.RemoveComponent(ballDyn.GetComponent<StaticColliderComponent>()!);
            RigidBodyComponent rbS = ballDyn.AddComponent<RigidBodyComponent>();
            rbS.Shape = new DevoidEngine.Physics.PhysicsShapeDescription()
            {
                Type = DevoidEngine.Physics.PhysicsShapeType.Sphere,
                Radius = ballDyn.Transform.Scale.X
            };

            FollowImpulseComponent fic = ballDyn.AddComponent<FollowImpulseComponent>();
            fic.FollowTarget = go1;

            //GameObject go = gameObject.Scene.AddGameObject("Debug");
            //MeshRenderer mr = sketchModel.AddComponent<MeshRenderer>();
            //mr.Mesh = PrimitiveMeshes.GetCube();
            //go.SetParent(gameObject);
            //go.Transform.LocalPosition = new Vector3(0, -1, 0);
        }

        void SetupUI()
        {
            Viewport viewport = Engine.Instance.SceneTree.RootViewport;

            CanvasNode canvas = new()
            {
                Justify = JustifyContent.Start,
            };

            viewport.UIContext.Canvases.Add(canvas);

            ContainerNode container = new()
            {
                Size = new Vector2(300, 0),
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 0,
                },
                Direction = FlexDirection.Column,
            };

            container.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0, 0, 0f, 0.5f)
            });

            ContainerNode headerBar = new()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 0,
                },
                Padding = Padding.GetAll(10),
                Gap = 10,
            };

            ContainerNode innerContainer = new()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 1,
                },
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Gap = 10
            };

            innerContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = Vector4.Zero,

            });

            //container.Add(subContainer2);

            Font valvepulp = Asset.Load<Font>("valvepulp-bold.ttf")!;

            Font valveoracle = Asset.Load<Font>("valveoracle-semibold.ttf")!;

            LabelNode headerLabelNode1 = new()
            {
                Font = valveoracle,
                FontSize = 32,
                Overflow = TextOverflow.Clip,
                Text = "Tools",
            };

            headerBar.Add(headerLabelNode1);
            container.Add(headerBar);


            // Buttons

            ContainerNode buttonPreviewContainer = GetPreviewBox();

            LabelNode buttonPreviewLabel = new()
            {
                Font = valvepulp,
                FontSize = 16,
                Overflow = TextOverflow.Wrap,
                Text = "Scene Tools",
                VerticalAlignment = TextVerticalAlignment.Center,
                HorizontalAlignment = TextHorizontalAlignment.Center,
            };


            ButtonNode buttonPreview = new()
            {
                Text = "Save Scene To Disk",
                Layout = new()
                {
                    FlexGrowCross = 0,
                    FlexGrowMain = 0
                },
                OnPressed = () =>
                {
                    MessagePackSerializer.Serialize(SceneSerializer.Serialize(scene));
                    Console.WriteLine("Saved Scene to disk");
                }
            };

            InputFieldNode inputField = new()
            {
                HintText = "Save Path",
            };

            buttonPreviewContainer.Add(buttonPreviewLabel);
            buttonPreviewContainer.Add(inputField);
            buttonPreviewContainer.Add(buttonPreview);


            innerContainer.Add(buttonPreviewContainer);


            container.Add(innerContainer);

            canvas.Add(container);
            //canvas.Add(labelNode);
            //canvas.Add(labelNode1);

            canvas.Initialize();

        }

        ContainerNode GetPreviewBox()
        {
            ContainerNode container = new()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 0,
                    FlexGrowMain = 0
                },
                Direction = FlexDirection.Column,
                Padding = Padding.GetAll(10),
                Gap = 10
            };

            container.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0, 0, 0, 0.3f)
            });
            return container;
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
            Engine.Renderer.API.RenderToScreen(cmd, Engine.Instance.SceneTree.RootViewport.OutputTexture!);
        }
    }
}
