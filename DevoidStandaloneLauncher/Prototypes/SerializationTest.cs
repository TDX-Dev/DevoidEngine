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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Prototypes
{
    public class SerializationTest : Prototype
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

            AudioClip toneAudio = Asset.Load<AudioClip>("tone.mp3");

            GameObject audio = scene.AddGameObject("Audio");
            var audC = audio.AddComponent<AudioSourceComponent3D>();
            audC.Audio = toneAudio;

            audC.Play();


            GameObject testObject = scene.AddGameObject("Test Object# This should print if deserialized correctly!");
            var testComponent = testObject.AddComponent<TestComponent>();
            SerializeComponent(testComponent);
        }

        public override void OnUpdate(float delta)
        {
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
