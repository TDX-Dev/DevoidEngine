using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Prototypes
{
    public class ProjectLoaderTest : Prototype
    {
        public override void OnInit()
        {
            Scene scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);
            scene.Play(true);

            GameObject gameObject = scene.AddGameObject("Camera");
            CameraComponent3D camera = gameObject.AddComponent<CameraComponent3D>();
            camera.IsDefault = true;

            Texture2D shrekTexture = Asset.Load<Texture2D>("shrk.png");
            Audio toneAudio = Asset.Load<Audio>("tone.mp3");

            GameObject canvas = scene.AddGameObject("Canvas");
            var canvasComp = canvas.AddComponent<CanvasComponent>();

            var container = new ContainerNode()
            {
                Size = new System.Numerics.Vector2(200, 200)
            };

            container.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxTexture()
            {
                Texture = shrekTexture
            });

            canvasComp.Canvas.Add(container);
        }

    }
}
