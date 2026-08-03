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

            scene = Asset.Load<PackedScene>("models/spheres_pbr_test.gltf")!.Instantiate();
            //scene.GameObjects[0].Transform.Position = new Vector3(0, 5, 0);

            Engine.Instance.SceneTree.LoadScene(scene);
            scene.Play();

            Engine.Renderer.SkyRenderer.Sky = new HDRISky()
            {
                PanoramaTexture = Asset.Load<Texture>("HDRIs/ferndale_studio.hdr")!
            };

            //Engine.Instance.AssetDatabase.TryGetGuid("models/Tiles133A_2K-JPG_Color.jpg", out Guid fernDale);

            //Engine.Instance.AssetDatabase.Reimport(fernDale, MessagePackSerializer.Serialize<TextureImportSettings>(new TextureImportSettings()
            //{
            //    Format = TextureFormat.RGBA8_UNorm
            //}));


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

            go = scene.AddGameObject("Hello World");

            cam = go.AddComponent<Camera3D>();

            //meshGo = scene.AddGameObject("Hello Mesh Object");
            //MeshRenderer meshRenderer = meshGo.AddComponent<MeshRenderer>();
            //meshRenderer.Mesh = PrimitiveMeshes.GetCube();
            //meshRenderer.Material = GroundPBRMaterial;
            //meshGo.Transform.Scale = new Vector3(20, 1f, 20);
            //StaticColliderComponent sb = meshGo.AddComponent<StaticColliderComponent>();
            //sb.Shape = new DevoidEngine.Physics.PhysicsShapeDescription()
            //{
            //    Type = DevoidEngine.Physics.PhysicsShapeType.Box,
            //    Size = new Vector3(20, 1f, 20),

            //};

            //meshGo1 = scene.AddGameObject("Hello Mesh 2 Object");
            //MeshRenderer meshRenderer1 = meshGo1.AddComponent<MeshRenderer>();
            //meshRenderer1.Mesh = PrimitiveMeshes.GetCube();
            //meshRenderer1.Material = PBRMaterial;
            //meshGo1.Transform.Position = new Vector3(0, 10, 0);
            //RigidBodyComponent rb = meshGo1.AddComponent<RigidBodyComponent>();

            //GameObject childObject = scene.AddGameObject("PhyChildObj");
            //MeshRenderer meshRenderer2 = childObject.AddComponent<MeshRenderer>();
            //meshRenderer2.Mesh = PrimitiveMeshes.GetUVSphere();
            //meshRenderer2.Material = GroundPBRMaterial;
            //childObject.SetParent(meshGo1);
            //childObject.Transform.LocalPosition = new Vector3(0, 2, -2);

            //rb.Shape = new DevoidEngine.Physics.PhysicsShapeDescription()
            //{
            //    Radius = 1,
            //    Type = DevoidEngine.Physics.PhysicsShapeType.Sphere
            //};


            lightGo = scene.AddGameObject("Light Object");
            LightComponent light = lightGo.AddComponent<LightComponent>();
            light.LightType = LightType.DirectionalLight;
            light.Intensity = 0;
            light.Radius = 100;
            light.InnerCutoff = 30;
            light.OuterCutoff = 35;
            light.Color = new Vector4(1, 1, 1, 1);
            lightGo.Transform.EulerAngles = new Vector3(45, 10, 0);
            lightGo.Transform.Position = new Vector3(5, 10, -7);
            AudioSource3D audio = lightGo.AddComponent<AudioSource3D>();

            //AudioClip clip = Asset.Load<AudioClip>("tone.mp3")!;
            //audio.Audio = clip;
            //audio.Play();

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

            Engine.InputSystem.Map.Bind("Grab", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.G
            });

            Engine.InputSystem.Map.Bind("SpawnLight", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.L
            });

            //Engine.InputSystem.Map.Bind("Impulse", new InputBinding()
            //{
            //    DeviceType = InputDeviceType.Keyboard,
            //    Control = (ushort)Keys.P
            //});

            //SetupUI();
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

        GameObject go = null!;
        readonly GameObject meshGo = null!;
        readonly GameObject meshGo1 = null!;
        GameObject lightGo = null!;

        Camera3D cam = null!;

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
            if (Engine.InputSystem.GetActionDown("Impulse"))
            {
                Console.WriteLine("Impulse Added");
                meshGo1.GetComponent<RigidBodyComponent>()!.AddForce(new Vector3(0, 750, 0));
            }

            if (Engine.InputSystem.GetActionDown("SpawnLight"))
            {
                GameObject lightGo = scene.AddGameObject("Light Object");
                LightComponent light = lightGo.AddComponent<LightComponent>();
                light.LightType = LightType.PointLight;
                light.Intensity = 10;
                light.Radius = 100;
                light.Color = new Vector4(1, 1, 1, 1);
                lightGo.Transform.EulerAngles = new Vector3(45, 0, 0);

                float posX = (float)rand.NextDouble() * 20;
                float posY = (float)rand.NextDouble() * 20;

                lightGo.Transform.Position = new Vector3(posX, 10, posY);
            }

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

            if (Engine.Cursor.GetCursorState() != CursorState.Grabbed)
                return;

            var transform = go.Transform;

            float forward =
                Engine.InputSystem.GetAction("Forward") -
                Engine.InputSystem.GetAction("Backward");

            float right =
                Engine.InputSystem.GetAction("Left") -
                Engine.InputSystem.GetAction("Right");

            Vector3 moveDirection = Vector3.Zero;

            moveDirection += transform.Forward * forward;
            moveDirection += transform.Right * right;

            if (moveDirection.LengthSquared() > 0.0f)
            {
                moveDirection = Vector3.Normalize(moveDirection);
            }


            const float moveSpeed = 5.0f;

            transform.Position +=
                moveDirection *
                moveSpeed *
                deltaTime;

            float lookX = Engine.InputSystem.GetAction("LookX");
            float lookY = Engine.InputSystem.GetAction("LookY");

            const float sensitivity = 0.0025f;

            yaw -= lookX * sensitivity;
            pitch += lookY * sensitivity;

            pitch = Math.Clamp(
                pitch,
                -MathF.PI * 0.49f,
                 MathF.PI * 0.49f);

            Quaternion rotation =
                Quaternion.CreateFromYawPitchRoll(
                    yaw,
                    pitch,
                    0.0f);

            go.Transform.Rotation = rotation;


        }

        private float yaw;
        private float pitch;

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
