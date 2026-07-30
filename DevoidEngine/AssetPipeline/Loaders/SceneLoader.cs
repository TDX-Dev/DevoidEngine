using DevoidEngine.Core;
using DevoidEngine.Serialization;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class SceneLoader : IAssetLoader<Scene>
    {
        public string RuntimeExtension => "scene";

        public Scene Load(ReadOnlySpan<byte> data)
        {
            Scene scene;
            try
            {
                scene = SceneSerializer.Deserialize(MessagePackSerializer.Deserialize<SceneData>(data.ToArray()));
            } catch (Exception ex)
            {
                scene = new Scene();
                Console.WriteLine($"[Scene Loader]: Error Loading Scene {ex.Message}");
            }

            return scene;
        }
    }
}
