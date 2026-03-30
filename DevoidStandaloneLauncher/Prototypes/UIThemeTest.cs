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
            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            int i = 0;

            foreach (var kv in DebugMutedColors)
            {
                if (i++ >= 10)
                    break;

                var container = new ContainerNode()
                {
                    Padding = Padding.GetAll(20),
                    Layout = new LayoutOptions()
                    {
                        FlexGrowMain = 1,
                    }
                };

                container.AddColorOverride(StyleKeys.Background, kv.Value);

                var label = new LabelNode(kv.Key, font, 26);

                label.AddColorOverride(StyleKeys.FontColor, GetReadableTextColor(kv.Value));

                container.Add(label);

                node.Add(container);
            }
        }

        public static Vector4 GetReadableTextColor(Vector4 background)
        {
            // Perceived luminance (sRGB)
            float luminance =
                (0.299f * background.X) +
                (0.587f * background.Y) +
                (0.114f * background.Z);

            // Threshold ~0.5 works well for UI
            if (luminance > 0.5f)
                return new Vector4(0f, 0f, 0f, 1f); // dark text
            else
                return new Vector4(1f, 1f, 1f, 1f); // light text
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