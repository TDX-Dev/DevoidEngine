using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Content.Scenes
{
    public class SplashScene
    {
        public static Scene CreateSplashScene(string loadingName = "None")
        {
            Scene scene = new Scene();

            // Camera
            var camera = scene.AddGameObject("Camera");
            var cam = camera.AddComponent<CameraComponent3D>();
            cam.IsDefault = true;

            // Canvas
            var canvasObj = scene.AddGameObject("Canvas");
            var canvas = canvasObj.AddComponent<CanvasComponent>();

            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                64
            );

            // Background
            var background = new ContainerNode()
            {
                Size = Screen.Size,
                ParticipatesInLayout = false
            };

            // Center layout (logo)
            var center = new FlexboxNode()
            {
                Direction = FlexDirection.Column,
                Align = AlignItems.Center,
                Justify = JustifyContent.Center,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1
                }
            };

            center.Add(new BoxNode()
            {
                Size = Screen.Size * 0.45f,
                Texture = Helper.LoadImageAsTex(
                    "Engine/Content/Textures/DevoidLogo.png",
                    DevoidGPU.TextureFilter.Linear
                )
            });

            // Top-left overlay
            var loadingOverlay = new FlexboxNode()
            {
                Offset = new Vector2(10, 10),
                Direction = FlexDirection.Column,
                Align = AlignItems.Start,
                Justify = JustifyContent.Start,
                ParticipatesInLayout = false
            };

            loadingOverlay.Add(
                new LabelNode($"Currently Loading: {loadingName}", font, 16f)
            );

            // Canvas order matters (background first)
            canvas.Canvas.Add(background);
            canvas.Canvas.Add(center);
            canvas.Canvas.Add(loadingOverlay);

            return scene;
        }
    }
}