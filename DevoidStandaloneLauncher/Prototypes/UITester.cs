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
        GameObject dirLight;

        Mesh testRender;

        //string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/crt.fbx";
        //string levelPath = "C:\\Users\\maari\\Downloads\\service_pistol_2k.gltf\\service_pistol_2k.fbx";
        //string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/service_pistol_2k.fbx";
        string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/devoid_test_level2.fbx";
        
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

        LightComponent spotComp;

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

            //GameObject light = scene.AddGameObject("Light");
            //var lightComp = light.AddComponent<LightComponent>();
            //light.Transform.Position = new System.Numerics.Vector3(0, 5, -10);
            //lightComp.Intensity = 100;
            //lightComp.Radius = 100;

            GameObject skybox = scene.AddGameObject("Skybox");
            //var skyboxComp = skybox.AddComponent<SkyboxComponent>();

            GameObject spotLight = scene.AddGameObject("SpotLight1");
            spotComp = spotLight.AddComponent<LightComponent>();
            spotComp.LightType = LightType.SpotLight;
            spotComp.Intensity = 1000;
            spotComp.InnerCutoff = 60;
            spotComp.OuterCutoff = 40;
            spotLight.Transform.Position = new Vector3(0, 3, 0);
            spotLight.Transform.Rotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI * 0.5f);


            scene.Play(true);

            //PBRSpheres.SpawnSphereGrid(scene, new Vector3(0, 5, 5));

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

            FlexboxNode leftContainer = new FlexboxNode()
            {
                Direction = FlexDirection.Column,
                Gap = 10
            };

            ContainerNode buttonContainer = new ContainerNode()
            {
                //ParticipatesInLayout = false,
                Offset = new Vector2(50, 50),
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Gap = 7,
            };

            inspectorContainer = new ContainerNode()
            {
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Gap = 7,
            };

            buttonContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.7f),
                BorderRadius = new Vector4(5)
            });

            inspectorContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
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

            int val = 0;
            buttonContainer.Add(new DragIntNode()
            {
                Value = val
            });

            //AddDirLightControls(buttonContainer);

            FlexboxNode mainWindowSize = new FlexboxNode()
            {
                BlockInput = false,
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
            DrawSceneObjects(rightContainer);

            rightContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
            {
                BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.7f),
                BorderRadius = new Vector4(5)
            });

            leftContainer.Add(buttonContainer);
            leftContainer.Add(inspectorContainer);
            canvas.Canvas.Add(leftContainer);
            canvas.Canvas.Add(mainWindowSize);
            canvas.Canvas.Add(rightContainer);

            //canvas.RenderMode = CanvasRenderMode.WorldSpace;
            //canvas.CanvasSize = new Vector2(1920, 1080);
            //canvasObject.Transform.EulerAngles = new Vector3(0, 180, 0);
        }

        ContainerNode inspectorContainer;

        void DrawSceneObjects(UINode node)
        {
            ScrollNode scrollNode = new ScrollNode()
            {
                Direction = FlexDirection.Column
            };

            for (int i = 0; i < scene.GameObjects.Count; i++)
            {
                GameObject obj = scene.GameObjects[i];

                var btn = new ButtonNode()
                {
                    Text = obj.Name,
                };

                scrollNode.Add(btn);

                btn.OnPressed = () =>
                {
                    DrawInspector(obj);
                };
            }

            node.Add(scrollNode);
        }

        void DrawInspector(GameObject obj)
        {
            inspectorContainer.Clear();

            // Object name
            LabelNode nameLabel = new LabelNode(obj.Name);
            inspectorContainer.Add(nameLabel);

            Transform3D transform = obj.GetComponent<Transform3D>();

            if (transform == null)
                return;

            AddVector3Drag("Position", transform.Position, v =>
            {
                transform.Position = v;
            });

            AddVector3Drag("Rotation", transform.EulerAngles, v =>
            {
                transform.EulerAngles = v;
            });

            var light = obj.GetComponent<LightComponent>();
            if (light != null && light.LightType == LightType.SpotLight)
            {
                var innerCutoffDrag = new DragFloatNode()
                {
                    Value = light.InnerCutoff
                };

                var outerCutoffDrag = new DragFloatNode()
                {
                    Value = light.OuterCutoff
                };

                innerCutoffDrag.OnValueChanged = (float e) => light.InnerCutoff = e;
                outerCutoffDrag.OnValueChanged = (float e) => light.OuterCutoff = e;

                var intensityDrag = new DragFloatNode()
                {
                    Value = light.Intensity
                };

                var radiusDrag = new DragFloatNode()
                {
                    Value = light.Radius
                };

                intensityDrag.OnValueChanged = (float e) => light.Intensity = e;
                radiusDrag.OnValueChanged = (float e) => light.Radius = e;

                inspectorContainer.Add(new LabelNode("Spotlight Cutoff"));
                inspectorContainer.Add(innerCutoffDrag);
                inspectorContainer.Add(outerCutoffDrag);

                inspectorContainer.Add(new LabelNode("Intensity & Radius"));
                inspectorContainer.Add(intensityDrag);
                inspectorContainer.Add(radiusDrag);
            }
        }

        void AddVector3Drag(string label, Vector3 value, Action<Vector3> onChanged)
        {
            LabelNode title = new LabelNode(label);
            inspectorContainer.Add(title);

            Vector3 current = value;

            FlexboxNode row = new FlexboxNode()
            {
                Direction = FlexDirection.Row,
                Gap = 6
            };

            DragFloatNode x = new DragFloatNode() { Value = value.X };
            DragFloatNode y = new DragFloatNode() { Value = value.Y };
            DragFloatNode z = new DragFloatNode() { Value = value.Z };

            x.OnValueChanged = v =>
            {
                current.X = v;
                onChanged(current);
            };

            y.OnValueChanged = v =>
            {
                current.Y = v;
                onChanged(current);
            };

            z.OnValueChanged = v =>
            {
                current.Z = v;
                onChanged(current);
            };

            row.Add(x);
            row.Add(y);
            row.Add(z);

            inspectorContainer.Add(row);
        }

        Vector3 dirLightEuler = Vector3.Zero;
        void AddDirLightControls(UINode node)
        {
            LabelNode bloomSliderLabel = new LabelNode("Dir Light Controls: ");
            SliderNode XSlider = new SliderNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            SliderNode YSlider = new SliderNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            SliderNode IntensitySlider = new SliderNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1 
                }
            };

            FlexboxNode bloomSliderContainer = new FlexboxNode()
            {
                Gap = 10,
                Align = AlignItems.Stretch,
                Direction = FlexDirection.Column
            };

            XSlider.OnValueChanged = (float e) =>
            {
                //if (dirLight == null)
                //    return;

                //dirLightEuler.X = (e * 360f) - 180f;
                //dirLight.Transform.EulerAngles = dirLightEuler;

                spotComp.InnerCutoff = (e * 60);
            };

            YSlider.OnValueChanged = (float e) =>
            {
                //if (dirLight == null)
                //    return;

                //dirLightEuler.Y = (e * 360f) - 180f;
                //dirLight.Transform.EulerAngles = dirLightEuler;
                spotComp.OuterCutoff = (e * 60);
            };

            IntensitySlider.OnValueChanged = (float e) =>
            {
                if (dirLight == null)
                    return;

                dirLight.GetComponent<LightComponent>().Intensity = e * 100;
            };

            bloomSliderContainer.Add(bloomSliderLabel);
            bloomSliderContainer.Add(XSlider);
            bloomSliderContainer.Add(YSlider);
            bloomSliderContainer.Add(IntensitySlider);

            node.Add(bloomSliderContainer);
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

            //FlexboxNode thumbnailParentContainer = new FlexboxNode()
            //{
            //    Align = AlignItems.Start
            //};

            //ContainerNode thumbnailContainer = new ContainerNode()
            //{
            //    MinSize = new Vector2(128, 128),
            //    MaxSize = new Vector2(128, 128)
            //};

            //thumbnailContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxTexture()
            //{
            //    Texture = Helper.LoadImageAsTex("D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/textures/cube_thumbnail_place.png", DevoidGPU.TextureFilter.Linear)
            //});

            //thumbnailParentContainer.Add(thumbnailContainer);
            //parent.Add(thumbnailParentContainer);
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

                GameObject flash = scene.AddGameObject("flashlight");
                var lightComp = flash.AddComponent<LightComponent>();
                lightComp.LightType = LightType.SpotLight;

                flash.SetParent(camera, false);
                flash.Transform.LocalPosition = new Vector3(0, 0, 2);

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
                //if (assimpLight.LightType == Assimp.LightSourceType.Directional || assimpLight.LightType == Assimp.LightSourceType.Spot)
                //    return;
                GameObject lightGO = scene.AddGameObject(assimpNode.Name);

                Importer.ApplyTransform(lightGO, assimpNode);

                var eul = lightGO.Transform.EulerAngles;
                eul.X = 90;
                lightGO.Transform.EulerAngles = eul;

                var lightComponent = lightGO.AddComponent<LightComponent>();

                float multiplier = 1f;

                switch (assimpLight.LightType)
                {
                    case Assimp.LightSourceType.Point:
                        lightComponent.LightType = LightType.PointLight;
                        break;
                    case Assimp.LightSourceType.Directional:
                        lightComponent.LightType = LightType.DirectionalLight;
                        dirLight = lightGO;
                        break;
                    case Assimp.LightSourceType.Spot:
                        lightComponent.LightType = LightType.SpotLight;
                        break;
                }

                Vector3 diffuse = new Vector3(assimpLight.ColorDiffuse.X, assimpLight.ColorDiffuse.Y, assimpLight.ColorDiffuse.Z);
                float intensity = MathF.Max(diffuse.X, MathF.Max(diffuse.Y, diffuse.Z));

                Vector3 color = intensity > 0.0f
                    ? diffuse / intensity
                    : Vector3.Zero;

                //lightComponent.Color = new Vector4(assimpLight.ColorDiffuse.R, assimpLight.ColorDiffuse.G, assimpLight.ColorDiffuse.B, 1f);
                lightComponent.Color = new Vector4(color, 1f) * 5;

                lightComponent.Radius = 20;
                lightComponent.Intensity = intensity * 0.01f; // your scale

                lightComponent.InnerCutoff = MathHelper.RadToDeg(assimpLight.AngleInnerCone);
                lightComponent.OuterCutoff = MathHelper.RadToDeg(assimpLight.AngleOuterCone);


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