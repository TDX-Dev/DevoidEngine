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
                Align = AlignItems.Stretch,
                Justify = JustifyContent.Start,
                Gap = 10,
                Padding = Padding.GetAll(10),
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1,
                    FlexGrowCross = 1
                }
            };

            FlexboxNode leftInnerContainer = new FlexboxNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.2f,
                }
            };

            FlexboxNode rightInnerContainer = new FlexboxNode()
            {
                Direction = FlexDirection.Column,
                Gap = 10,
                Padding = Padding.GetAll(10),
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.8f,
                }
            };

            FlexboxNode rightInnerInnerContainer1 = new FlexboxNode()
            {
                Justify = JustifyContent.SpaceEvenly,
                Align = AlignItems.Center,
                Gap = 10,
                Padding = Padding.GetAll(10),
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.3f
                }
            };

            FlexboxNode rightInnerInnerContainer2 = new FlexboxNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.7f
                }
            };

            AddContainerList(rightInnerInnerContainer1);

            rightInnerContainer.Add(rightInnerInnerContainer1);
            rightInnerContainer.Add(rightInnerInnerContainer2);

            bodyContainer.Add(leftInnerContainer);
            bodyContainer.Add(rightInnerContainer);

            canvas.Canvas.Add(bodyContainer);
        }


        void AddContainerList(UINode node)
        {
            for (int i = 0; i < 5; i++)
            {
                node.Add(new ContainerNode()
                {
                    Layout = new LayoutOptions()
                    {
                        FlexGrowMain = 1,
                        FlexGrowCross = 1
                    }
                });
            }
        }
    }
}