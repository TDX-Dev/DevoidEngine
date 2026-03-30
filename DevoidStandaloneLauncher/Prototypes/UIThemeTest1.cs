using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
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
                Gap = 10,
                Padding = Padding.GetAll(10),
                Direction = FlexDirection.Column,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.2f,
                }
            };

            AddAlterBoxes(leftInnerContainer, false, 2);

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
                Wrap = FlexWrap.Wrap,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.3f
                }
            };

            FlexboxNode rightInnerInnerContainer2 = new FlexboxNode()
            {
                Padding = Padding.GetAll(10),
                Gap = 10,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 0.7f
                }
            };

            AddContainerList(rightInnerInnerContainer1, 5);
            AddFlexboxList(rightInnerInnerContainer2);

            rightInnerContainer.Add(rightInnerInnerContainer1);
            rightInnerContainer.Add(rightInnerInnerContainer2);

            bodyContainer.Add(leftInnerContainer);
            bodyContainer.Add(rightInnerContainer);

            canvas.Canvas.Add(bodyContainer);
        }

        // Dk what to call this.
        void AddAlterBoxes(UINode node, bool inv, int depth = 0)
        {
            FlexboxNode rightInnerInnerContainer1 = new FlexboxNode()
            {
                Padding = Padding.GetAll(10),
                Gap = 10,
                Direction = FlexDirection.Column,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = inv ? 0.7f : 0.3f
                }
            };

            FlexboxNode rightInnerInnerContainer2 = new FlexboxNode()
            {
                Padding = Padding.GetAll(10),
                Gap = 10,
                Direction = FlexDirection.Column,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = inv ? 0.3f : 0.7f
                }
            };

            node.Add(rightInnerInnerContainer1);
            node.Add(rightInnerInnerContainer2);

            if (depth == 0)
                return;

            AddAlterBoxes(rightInnerInnerContainer1, !inv, depth - 1);
            AddAlterBoxes(rightInnerInnerContainer2, inv, depth - 1);
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

                AddAlterBoxes(rightInnerInnerContainer2, i % 2 == 0, 2);

                node.Add(rightInnerInnerContainer2);
            }
        }


        void AddContainerList(UINode node, int n = 5, float grow = 1)
        {
            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            int i = 0;

            foreach (var kv in DebugMutedColors)
            {
                if (i++ >= n)
                    break;

                var container = new ContainerNode()
                {
                    Padding = Padding.GetAll(20),
                    Layout = new LayoutOptions()
                    {
                        FlexGrowMain = grow,
                    }
                };

                container.AddColorOverride(StyleKeys.Background, kv.Value);
                container.AddConstantOverride(StyleKeys.BorderRadius, new Vector4(20));

                var label = new LabelNode(kv.Key, font, 20)
                {
                    LayoutOptions = new TextLayoutOptions()
                    {
                        Overflow = TextOverflow.Overflow
                    }
                };

                label.AddColorOverride(StyleKeys.FontColor, GetReadableTextColor(kv.Value));

                container.Add(label);

                node.Add(container);
            }
        }

        public static Vector4 GetReadableTextColor(Vector4 background)
        {
            float luminance =
                (0.299f * background.X) +
                (0.587f * background.Y) +
                (0.114f * background.Z);

            if (luminance > 0.5f)
                return new Vector4(0f, 0f, 0f, 1f);
            else
                return new Vector4(1f, 1f, 1f, 1f);
        }

        public static readonly Dictionary<string, Vector4> DebugMutedColors = new()
        {
            { "MutedRed",        new Vector4(0.75f, 0.35f, 0.35f, 1f) },
            { "MutedGreen",      new Vector4(0.35f, 0.65f, 0.35f, 1f) },
            { "MutedBlue",       new Vector4(0.35f, 0.45f, 0.75f, 1f) },
            { "MutedYellow",     new Vector4(0.75f, 0.75f, 0.35f, 1f) },
            { "MutedCyan",       new Vector4(0.35f, 0.65f, 0.65f, 1f) },
            { "MutedMagenta",    new Vector4(0.65f, 0.35f, 0.65f, 1f) },
            { "MutedOrange",     new Vector4(0.75f, 0.50f, 0.30f, 1f) },
            { "MutedPurple",     new Vector4(0.55f, 0.40f, 0.70f, 1f) },
            { "MutedPink",       new Vector4(0.75f, 0.50f, 0.60f, 1f) },
            { "MutedLime",       new Vector4(0.60f, 0.75f, 0.40f, 1f) },
            { "MutedTeal",       new Vector4(0.30f, 0.55f, 0.55f, 1f) },
            { "MutedIndigo",     new Vector4(0.40f, 0.35f, 0.60f, 1f) },
            { "MutedBrown",      new Vector4(0.55f, 0.40f, 0.30f, 1f) },
            { "MutedGray",       new Vector4(0.55f, 0.55f, 0.55f, 1f) },
            { "MutedWhite",      new Vector4(0.85f, 0.85f, 0.85f, 1f) },
        };
    }
}