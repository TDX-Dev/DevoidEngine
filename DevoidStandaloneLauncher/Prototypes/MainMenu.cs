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

        public override void OnInit()
        {
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);



            camera = scene.AddGameObject("Camera");
            CameraComponent3D cameraComponent = camera.AddComponent<CameraComponent3D>();
            cameraComponent.IsDefault = true;

            GameObject light = scene.AddGameObject("Light");
            var lightComp = light.AddComponent<LightComponent>();
            light.Transform.Position = new System.Numerics.Vector3(0, 5, -10);
            lightComp.Intensity = 100;
            lightComp.Radius = 100;

            CanvasComponent canvas = camera.AddComponent<CanvasComponent>();
            camera.AddComponent<ButtonComponent>();

            ContainerNode buttonContainer = new ContainerNode()
            {
                ParticipatesInLayout = false,
                Color = Helper.RGBANormalize(new Vector4(34, 40, 49, 255)),
                Size = new Vector2(300, 400),
                Offset = new Vector2(50, 275),
                Padding = new Padding()
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10,
                }
            };

            ContainerNode innterButtonContainer = new ContainerNode()
            {
                Color = Helper.RGBANormalize(new Vector4(57, 62, 70, 255)),
                Offset = new Vector2(50, 275),
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            buttonContainer.Add(innterButtonContainer);
            canvas.Canvas.Add(buttonContainer);

            scene.Play(true);
        }


    }
}