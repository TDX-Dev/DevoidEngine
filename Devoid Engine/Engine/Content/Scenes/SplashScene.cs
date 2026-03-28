using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;

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

            var root = new FlexboxNode()
            {
                Direction = FlexDirection.Column,
                Align = AlignItems.Center,
                Justify = JustifyContent.Center,
                Layout = new LayoutOptions() { FlexGrowMain = 1 },
            };

            var root1 = new FlexboxNode()
            {
                Offset = new System.Numerics.Vector2(10, 10),
                Direction = FlexDirection.Column,
                Align = AlignItems.Center,
                Justify = JustifyContent.Center,
                Layout = new LayoutOptions() { FlexGrowMain = 1 },
                ParticipatesInLayout = false
            };

            root1.Add(new LabelNode($"Currently Loading: {loadingName}", font, 16f));

            root.Add(new BoxNode()
            {
                Size = Screen.Size * 0.25f,
                Texture = Helper.LoadImageAsTex("Engine/Content/Textures/DevoidLogo.png", DevoidGPU.TextureFilter.Linear)
            });

            canvas.Canvas.Add(root);
            canvas.Canvas.Add(root1);

            // Add splash controller
            //var controllerGO = scene.addGameObject("SplashController");
            //controllerGO.AddComponent<SplashController>();
            return scene;
        }



    }
}