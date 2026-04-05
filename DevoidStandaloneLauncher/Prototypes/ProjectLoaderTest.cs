using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serialization;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using DevoidStandaloneLauncher.Scripts;
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
            AudioClip toneAudio = Asset.Load<AudioClip>("tone.mp3");

            GameObject audio = scene.AddGameObject("Audio");
            var audC = audio.AddComponent<AudioSourceComponent3D>();
            audC.Audio = toneAudio;

            audio.Transform.Position = new System.Numerics.Vector3(0, 10, 0);

            audC.Play();

            GameObject canvas = scene.AddGameObject("Canvas");
            var canvasComp = canvas.AddComponent<CanvasComponent>();

            container = new ContainerNode()
            {
                Size = new System.Numerics.Vector2(200, 200)
            };

            container.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxTexture()
            {
                Texture = shrekTexture
            });

            canvasComp.Canvas.Add(container);








        }
        ContainerNode container;
        float timer = 0;

        public override void OnUpdate(float delta)
        {
            timer += delta;
            container.Rotation = timer;
        }

    }
}
