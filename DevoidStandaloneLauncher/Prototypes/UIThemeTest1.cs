using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Scripts;
using DevoidStandaloneLauncher.Utils;
using System.Numerics;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class UIThemeTest1 : Prototype
    {
        Scene scene;
        GameObject camera;
        GameObject cubeObject;

        public override void OnInit()
        {
            DefaultInput.ConfigureInput();

            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            Console.WriteLine("Initialized");
            this.scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);

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

            ContainerNode leftInnerContainer = new ContainerNode()
            {
                Gap = 10,
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Align = AlignItems.Start,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.2f,
                }
            };

            SliderNode slider = new SliderNode()
            {
                MinSize = new Vector2(1, 30),
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1
                }
            };

            SliderNode sliderInner = new SliderNode()
            {
                MinSize = new Vector2(1, 30),
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1
                }
            };

            FlexboxNode checkboxLabelContainer = new FlexboxNode()
            {
                Align = AlignItems.Center,
                Gap = 10,
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1
                }
            };

            LabelNode checkboxLabel = new LabelNode("Radio Button", font, 16);

            CheckboxNode checkbox = new CheckboxNode()
            {
                Size = new Vector2(35),
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 0
                }
            };

            DropdownNode dropdown = new DropdownNode();

            dropdown.SetItems(["Item1", "Item2", "Item3", "Item4"]);

            ButtonNode button = new ButtonNode()
            {
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1
                }
            };

            checkboxLabelContainer.Add(checkbox);
            checkboxLabelContainer.Add(checkboxLabel);

            leftInnerContainer.Add(slider);
            leftInnerContainer.Add(sliderInner);
            leftInnerContainer.Add(checkboxLabelContainer);

            leftInnerContainer.Add(button);
            leftInnerContainer.Add(dropdown);
            AddThumbnails(leftInnerContainer);

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

            ContainerNode rightInnerInnerContainer1 = new ContainerNode()
            {
                Padding = Padding.GetAll(10),
                Gap = 10,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.3f
                }
            };

            ContainerNode rightInnerInnerContainer2 = new ContainerNode()
            {
                Padding = Padding.GetAll(10),
                Gap = 10,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.7f
                }
            };

            SplitterNode splitter1 = new SplitterNode();
            splitter1.Target = rightInnerInnerContainer1;
            splitter1.Vertical = true;
            splitter1.MinSize = new Vector2(0, 6);
            splitter1.MaxSize = new Vector2(float.PositiveInfinity, 6);

            rightInnerContainer.Add(rightInnerInnerContainer1);
            rightInnerContainer.Add(splitter1);
            rightInnerContainer.Add(rightInnerInnerContainer2);

            dropdown.OnSelectionChanged = (int e) =>
            {
                var children = rightInnerInnerContainer1.Children.ToArray();
                for (int i = 0; i < children.Length; i++)
                {
                    rightInnerInnerContainer1.Remove(children[i]);
                }
                AddFlexboxList(rightInnerInnerContainer1, e + 1);
            };

            slider.OnValueChanged = (float e) =>
            {
                rightInnerInnerContainer1.Layout.FlexGrowMain = e;
                rightInnerInnerContainer2.Layout.FlexGrowMain = 1 - e;
            };

            checkbox.OnValueChanged = (bool e) =>
            {
                if (e)
                {
                    AddFlexboxList(rightInnerInnerContainer2, 2);
                } else
                {
                    var children = rightInnerInnerContainer2.Children.ToArray();
                    for (int i = 0; i <  children.Length; i++)
                    {
                        rightInnerInnerContainer2.Remove(children[i]);
                    }
                }
            };

            sliderInner.OnValueChanged = (float e) =>
            {
                if (rightInnerInnerContainer2.Children.Count == 0) return;

                var flex1 = rightInnerInnerContainer2.Children[0];
                var flex2 = rightInnerInnerContainer2.Children[1];

                flex1.Layout.FlexGrowMain = e;
                flex2.Layout.FlexGrowMain = 1 - e;
            };

            SplitterNode splitter = new SplitterNode();
            splitter.Target = leftInnerContainer;
            splitter.MinSize = new Vector2(6, 0);
            splitter.MaxSize = new Vector2(6, float.PositiveInfinity);

            bodyContainer.Add(leftInnerContainer);
            bodyContainer.Add(splitter);
            bodyContainer.Add(rightInnerContainer);

            canvas.Canvas.Add(bodyContainer);

            scene.Play(true);



            //camera.AddComponent<FreeCameraComponent>();
            //canvas.Canvas.Add(slider);
        }

        public override void OnRender()
        {
            //DebugRenderSystem.DrawCube(Vector3.One, Vector3.Zero, Matrix4x4.Identity);
        }

        void AddThumbnails(UINode node)
        {
            FlexboxNode mainContainer = new FlexboxNode()
            {
                Wrap = FlexWrap.Wrap,
                Gap = 10,
                Padding = Padding.GetAll(10),
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 1
                }
            };

            for (int i = 0; i < 5; i++)
            {
                ContainerNode baseContainer = new ContainerNode()
                {
                    MinSize = new Vector2(100, 100),
                    Padding = Padding.GetAll(5),
                    Gap = 10,
                    Direction = FlexDirection.Column
                };

                baseContainer.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(1, 1, 1, 0.2f),
                    BorderRadius = new Vector4(5)
                });

                ContainerNode thumbnail = new ContainerNode()
                {
                    MinSize = new Vector2(100, 100),
                    Layout = new LayoutOptions()
                    {
                        FlexGrowCross = 1,
                        FlexGrowMain = 1
                    }
                };

                thumbnail.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxFlat()
                {
                    BackgroundColor = MutedColors.GetRandom(),
                    BorderRadius = new Vector4(5)
                });

                LabelNode thumbnailLabel = new LabelNode("Thumbnail");

                baseContainer.Add(thumbnail);
                baseContainer.Add(thumbnailLabel);
                mainContainer.Add(baseContainer);
            }

            node.Add(mainContainer);
        }

        void AddFlexboxList(UINode node, int n = 4)
        {
            for (int i = 0; i < n; i++)
            {
                FlexboxNode rightInnerInnerContainer2 = new FlexboxNode()
                {
                    Padding = Padding.GetAll(10),
                    Gap = 10,
                    Direction = FlexDirection.Column,
                    Layout = new LayoutOptions()
                    {
                        FlexGrowMain = 0.7f
                    }
                };

                node.Add(rightInnerInnerContainer2);
            }
        }
    }
}