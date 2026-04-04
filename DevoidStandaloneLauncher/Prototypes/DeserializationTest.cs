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

            Scene scene = DeserializeScene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);
            scene.Play();

            var audioSources = scene.GetComponentsOfType<AudioSourceComponent3D>();
            for (int i = 0; i < audioSources.Count; i++)
                audioSources[i].Play();

            //Console.WriteLine(scene.GetComponentsOfType<CameraComponent3D>()[0].IsDefault);
        }

        public override void OnUpdate(float delta)
        {
        }


        public Scene DeserializeScene()
        {
            string path = ProjectManager.Current.AssetPath + "/scene.scene";

            try
            {
                byte[] bytes = File.ReadAllBytes(path);

                var options = MessagePackSerializerOptions.Standard
                    .WithSecurity(MessagePackSecurity.UntrustedData);

                SceneData sceneData =
                    MessagePackSerializer.Deserialize<SceneData>(bytes, options);

                return SceneSerializer.Deserialize(sceneData);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Scene] Failed to deserialize scene: {e.Message}");
                return new Scene();
            }
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
