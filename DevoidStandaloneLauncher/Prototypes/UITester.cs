using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Rendering.PostProcessing;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
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

        //string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/crt.fbx";
        //string levelPath = "C:\\Users\\maari\\Downloads\\service_pistol_2k.gltf\\service_pistol_2k.fbx";
        string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/service_pistol_2k.fbx";
        
        void LoadInput()
        {
            Input.Map.Bind("LookX", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaX,
                isClamped = false
            });

            Input.Map.Bind("LookY", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaY,
                isClamped = false
            });

            Input.Map.Bind("Forward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.W
            });

            Input.Map.Bind("Backward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.S
            });

            Input.Map.Bind("Left", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.A
            });

            Input.Map.Bind("Right", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.D
            });

            Input.Map.Bind("Capture", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.G
            });

            Input.Map.Bind("Up", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.Space
            });

            Input.Map.Bind("Down", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.LeftShift
            });
        }
        public override void OnInit()
        {
            LoadInput();

            Console.WriteLine("Initialized");
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);

            testRender = new Mesh();
            testRender.SetVertices(Primitives.GetSphereVertices(128, 128, 0.75f));



            //camera = scene.AddGameObject("Camera");
            //CameraComponent3D cameraComponent = camera.AddComponent<CameraComponent3D>();
            //cameraComponent.IsDefault = true;
            //camera.Transform.Position = new System.Numerics.Vector3(0, 0, -10);
            //camera.Transform.EulerAngles = new System.Numerics.Vector3(32, -47f, 0);
            //camera.Transform.EulerAngles = new System.Numerics.Vector3(-45, 0, 0);

            GameObject light = scene.AddGameObject("Light");
            var lightComp = light.AddComponent<LightComponent>();
            light.Transform.Position = new System.Numerics.Vector3(0, 5, -10);
            lightComp.Intensity = 100;
            lightComp.Radius = 100;

            GameObject skybox = scene.AddGameObject("Skybox");
            var skyboxComp = skybox.AddComponent<SkyboxComponent>();

            scene.Play(true);

            PBRSpheres.SpawnSphereGrid(scene, new Vector3(0, 5, 5));

            if (levelPath != "")
            {
                LoadDCC();
                Importer.LoadModel(levelPath);
            }


            canvasObject = scene.AddGameObject("CanvasObject");
            CanvasComponent canvas = canvasObject.AddComponent<CanvasComponent>();

            canvas.Canvas.Align = AlignItems.Start;
            canvas.Canvas.Justify = JustifyContent.Start;
            canvas.Canvas.Padding = Padding.GetAll(50);

            ContainerNode buttonContainer = new ContainerNode()
            {
                //ParticipatesInLayout = false,
                Offset = new Vector2(50, 50),
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Gap = 7,
            };

            buttonContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.7f),
                BorderRadius = new Vector4(5)
            });

            fpsLabel = new LabelNode("");
            objectLabel = new LabelNode("");
            fixedUpdateLabel = new LabelNode("");

            ButtonNode button = new ButtonNode("Add Physics Object");
            
            buttonContainer.Add(fpsLabel);
            buttonContainer.Add(fixedUpdateLabel);
            buttonContainer.Add(objectLabel);
            AddSliders(buttonContainer);
            buttonContainer.Add(button);

            FlexboxNode mainWindowSize = new FlexboxNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            ContainerNode rightContainer = new ContainerNode()
            {
                //ParticipatesInLayout = false,
                Offset = new Vector2(50, 50),
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Gap = 7,
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 0
                }
            };

            LabelNode sceneInfoText = new LabelNode("Scene Loaded: PBR Testing");

            rightContainer.Add(sceneInfoText);

            rightContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.7f),
                BorderRadius = new Vector4(5)
            });

            canvas.Canvas.Add(buttonContainer);
            canvas.Canvas.Add(mainWindowSize);
            canvas.Canvas.Add(rightContainer);

            //canvas.RenderMode = CanvasRenderMode.WorldSpace;
            //canvas.CanvasSize = new Vector2(1920, 1080);
            //canvasObject.Transform.EulerAngles = new Vector3(0, 180, 0);
        }

        void AddSliders(UINode parent)
        {
            LabelNode bloomSliderLabel = new LabelNode("Bloom Intensity: ");
            SliderNode bloomSlider = new SliderNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            FlexboxNode bloomSliderContainer = new FlexboxNode()
            {
                Gap = 10,
                Align = AlignItems.Stretch
            };

            bloomSlider.OnValueChanged = (float e) =>
            {
                Renderer.PostProcessor.GetPass<TonemapPass>().BloomIntensity = e;
            };

            bloomSliderContainer.Add(bloomSliderLabel);
            bloomSliderContainer.Add(bloomSlider);

            LabelNode exposureSliderLabel = new LabelNode("Exposure: ");
            SliderNode exposureSlider = new SliderNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            FlexboxNode exposureSliderContainer = new FlexboxNode()
            {
                Gap = 10,
                Align = AlignItems.Stretch
            };

            exposureSlider.OnValueChanged = (float e) =>
            {
                Renderer.PostProcessor.GetPass<TonemapPass>().Exposure = e;
            };

            exposureSliderContainer.Add(exposureSliderLabel);
            exposureSliderContainer.Add(exposureSlider);

            parent.Add(bloomSliderContainer);
            parent.Add(exposureSliderContainer);

            FlexboxNode thumbnailParentContainer = new FlexboxNode()
            {
                Align = AlignItems.Start
            };

            ContainerNode thumbnailContainer = new ContainerNode()
            {
                MinSize = new Vector2(128, 128),
                MaxSize = new Vector2(128, 128)
            };

            thumbnailContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxTexture()
            {
                Texture = Helper.LoadImageAsTex("D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/textures/cube_thumbnail_place.png", DevoidGPU.TextureFilter.Linear)
            });

            thumbnailParentContainer.Add(thumbnailContainer);
            parent.Add(thumbnailParentContainer);
        }

        GameObject canvasObject;
        LabelNode fpsLabel;
        LabelNode objectLabel;
        LabelNode fixedUpdateLabel;

        float smoothedFPS = 60f;
        float smoothing = 0.1f; // lower = smoother
        float timer = 0;

        public override void OnUpdate(float delta)
        {
            float currentFPS = 1f / delta;
            smoothedFPS = smoothedFPS + (currentFPS - smoothedFPS) * smoothing;

            fpsLabel.Text = $"Update Hz: {(int)smoothedFPS}";
            objectLabel.Text = $"GameObjects in scene: {scene.GameObjects.Count}";

            //timer += delta;
            //canvasObject.Transform.EulerAngles = new Vector3(0, timer * 10, 0);
        }

        float smoothedFixedFPS = 60f;
        float smoothingFixed = 0.1f; // lower = smoother
        public override void OnFixedUpdate(float delta)
        {
            float currentFPS = 1f / delta;
            smoothedFixedFPS = smoothedFixedFPS + (currentFPS - smoothedFixedFPS) * smoothingFixed;

            fixedUpdateLabel.Text = $"Physics Hz: {(int)smoothedFixedFPS}";
        }

        void LoadDCC()
        {
            LevelSpawnRegistry.Register("Player_Flycam", (assimpNode, assimpScene) =>
            {
                //Cursor.SetCursorState(CursorState.Grabbed);

                camera = scene.AddGameObject("Camera");

                camera.Transform.Position = Importer.GetTransform(assimpNode).Item1;
                camera.Transform.Rotation = Importer.GetTransform(assimpNode).Item2;

                camera.AddComponent<FreeCameraComponent>();
                var camComponent = camera.AddComponent<CameraComponent3D>();
                camComponent.IsDefault = true;

            });

            LevelSpawnRegistry.RegisterFallBack((assimpNode, assimpScene) =>
            {
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