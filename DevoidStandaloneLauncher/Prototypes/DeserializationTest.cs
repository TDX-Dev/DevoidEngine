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

            scene.GetComponentsOfType<AudioSourceComponent3D>()[0].Play();

            Console.WriteLine(scene.GetComponentsOfType<CameraComponent3D>()[0].IsDefault);
        }

        public override void OnUpdate(float delta)
        {
        }


        public Scene DeserializeScene()
        {
            var deserializationBytes = (SceneData)MessagePackSerializer.Deserialize<SceneData>(File.ReadAllBytes(ProjectManager.Current.AssetPath + "/scene.scene"));
            Scene deserializedSceneData = SceneSerializer.Deserialize(deserializationBytes);

            Console.WriteLine("GameObjects in scene after deserialization: " + deserializedSceneData.GameObjects.Count);

            return deserializedSceneData;
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
