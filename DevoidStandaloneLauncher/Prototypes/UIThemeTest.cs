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
    internal class UIThemeTest : Prototype
    {
        Scene scene;
        GameObject camera;
        GameObject cubeObject;

        public override void OnInit()
        {
            Console.WriteLine("Initialized");
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);
            scene.Play(true);

            camera = scene.AddGameObject("Camera");
            var camComponent = camera.AddComponent<CameraComponent3D>();
            camComponent.IsDefault = true;


            CanvasComponent canvas = camera.AddComponent<CanvasComponent>();
            //canvas.Canvas.Align = AlignItems.Stretch;
            //canvas.Canvas.Justify = JustifyContent.Start;


            FlexboxNode bodyContainer = new FlexboxNode()
            {
                Size = Screen.Size,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1,
                    FlexGrowCross = 1,
                }
            };

            FlexboxNode leftInnerContainer = new FlexboxNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1,
                    FlexGrowCross = 1,
                }
            };

            FlexboxNode rightInnerContainer = new FlexboxNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1,
                    FlexGrowCross = 1,
                }
            };
            bodyContainer.Add(leftInnerContainer);
            bodyContainer.Add(rightInnerContainer);

            canvas.Canvas.Add(bodyContainer);
        }

    }
}