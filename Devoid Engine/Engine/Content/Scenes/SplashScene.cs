using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Content.Scenes
{
    class SplashScene 
    {
        public static Scene CreateSplashScene()
        {
            Scene scene = new Scene();

            // Camera
            var camera = scene.addGameObject("Camera");
            var cam = camera.AddComponent<CameraComponent3D>();
            cam.IsDefault = true;

            // Canvas
            var canvasObj = scene.addGameObject("Canvas");
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
                Layout = new LayoutOptions() { FlexGrowMain = 1 }
            };

            //root.Add(new LabelNode("Powered by", font, 32f));
            //root.Add(new LabelNode("DEVOID ENGINE", font, 64f));

            root.Add(new BoxNode()
            {
                Size = (new System.Numerics.Vector2(1920, 1080)) * 0.25f,
                Texture = Helper.loadImageAsTex("Engine/Content/Textures/DevoidLogo.png", DevoidGPU.TextureFilter.Linear)
            });

            canvas.Canvas.Add(root);

            // Add splash controller
            //var controllerGO = scene.addGameObject("SplashController");
            //controllerGO.AddComponent<SplashController>();
            return scene;
        }



    }
}
