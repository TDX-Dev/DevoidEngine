using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Serialization;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using DevoidStandaloneLauncher.Scripts;
using DevoidStandaloneLauncher.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Prototypes
{
    public class DeserializationTest : Prototype
    {
        public override void OnInit()
        {
            DefaultInput.ConfigureInput();

            Scene scene = DeserializeScene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);
            scene.Play();

            var audioSources = scene.GetComponentsOfType<AudioSourceComponent3D>();
             for (int i = 0; i < audioSources.Count; i++)
                audioSources[i].Pause();

            scene.GetComponentsOfType<CameraComponent3D>()[0].gameObject.AddComponent<FreeCameraComponent>();
            var gameObject = scene.GetComponentsOfType<CameraComponent3D>()[0].gameObject;
            gameObject.Transform.Position = new Vector3(0, 0, -5);

            Model model = Asset.Load<Model>("model.glb");
            GameObject go = model.Instantiate(scene);

        }

        public Scene DeserializeScene()
        {
            return Asset.Load<Scene>("scene.scene") ?? new Scene();
        }

        public void SerializeComponent(Component comp)
        {

            byte[] data = ComponentSerializationRegistry.Serialize(comp);

            var restored =
                (TestComponent)ComponentSerializationRegistry.Deserialize(
                    typeof(TestComponent).FullName!,
                    data);

            //Console.WriteLine("Owner GameObject: " + restored.gameObject.Name);
            foreach (KeyValuePair<string, Vector3> entry in restored.KeyVecs)
            {
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }

    }
}
