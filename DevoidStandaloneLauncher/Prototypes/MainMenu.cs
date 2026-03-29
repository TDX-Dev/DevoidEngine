using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Scripts;
using DevoidStandaloneLauncher.Utils;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class MainMenu : Prototype
    {
        Scene scene;
        GameObject camera;
        GameObject cubeObject;

        string levelPath = "D:/Programming/Devoid Engine/DevoidStandaloneLauncher/LauncherContents/main_menu.fbx";

        void ConfigureInput()
        {
            Input.Map.Bind("PosX", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.X,
                isClamped = false
            });

            Input.Map.Bind("PosY", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.Y,
                isClamped = false
            });

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
            ConfigureInput();
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);



            //camera = scene.AddGameObject("Camera");
            //CameraComponent3D cameraComponent = camera.AddComponent<CameraComponent3D>();
            //cameraComponent.IsDefault = true;

            GameObject light = scene.AddGameObject("Light");
            var lightComp = light.AddComponent<LightComponent>();
            light.Transform.Position = new System.Numerics.Vector3(0, 5, -10);
            lightComp.Intensity = 100;
            lightComp.Radius = 100;

            GameObject canvasObject = scene.AddGameObject("Canvas Object");
            CanvasComponent canvas = canvasObject.AddComponent<CanvasComponent>();

            GameObject ContainerObject = scene.AddGameObject("Container Object");
            ContainerObject.SetParent(canvasObject);

            var ContainerComponent = ContainerObject.AddComponent<ContainerComponent>();
            ContainerComponent.Padding = Padding.GetAll(15);
            ContainerComponent.Color = new Vector4(0, 0, 0, 0.3f);
            ContainerComponent.Align = AlignItems.Stretch;

            List<string> buttonNames = ["Play", "Options", "Credits", "Quit"];
            List<Action> buttonActions = [
                () => {loader.SwitchPrototype(new UITester()); },
                () => {},
                () => {},
                () => {loader.Application.Quit(); }
            ];

            for (int i = 0; i < 4; i++)
            {
                GameObject buttonObject = scene.AddGameObject("Button Object");
                buttonObject.SetParent(ContainerObject);
                var buttonComponent = buttonObject.AddComponent<ButtonComponent>();
                buttonComponent.Text = buttonNames[i];
                buttonComponent.OnClick += buttonActions[i];
            }


            //ContainerComponent.Align = AlignItems.Start;


            //camera.AddComponent<ButtonComponent>();

            //ContainerNode buttonContainer = new ContainerNode()
            //{
            //    ParticipatesInLayout = false,
            //    Color = Helper.RGBANormalize(new Vector4(34, 40, 49, 255)),
            //    Size = new Vector2(300, 400),
            //    Offset = new Vector2(50, 275),
            //    Padding = new Padding()
            //    {
            //        Top = 10,
            //        Bottom = 10,
            //        Left = 10,
            //        Right = 10,
            //    }
            //};

            //ContainerNode innterButtonContainer = new ContainerNode()
            //{
            //    Color = Helper.RGBANormalize(new Vector4(57, 62, 70, 255)),
            //    Offset = new Vector2(50, 275),
            //    Layout = new LayoutOptions()
            //    {
            //        FlexGrowMain = 1
            //    }
            //};

            //buttonContainer.Add(innterButtonContainer);
            //canvas.Canvas.Add(buttonContainer);

            scene.Play(true);
            
            
            if (levelPath != "")
            {
                LoadDCC();
                Importer.LoadModel(levelPath);
            }
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

            LevelSpawnRegistry.Register("Player_PanCam", (assimpNode, assimpScene) =>
            {
                //Cursor.SetCursorState(CursorState.Grabbed);

                camera = scene.AddGameObject("Camera");

                camera.Transform.Position = Importer.GetTransform(assimpNode).Item1;
                camera.Transform.Rotation = Importer.GetTransform(assimpNode).Item2;

                camera.AddComponent<MenuCameraPanComponent>();
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