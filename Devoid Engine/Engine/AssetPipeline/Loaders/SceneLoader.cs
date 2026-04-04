using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serialization;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class SceneLoader : IAssetLoader<Scene>
    {
        public Scene Load(ReadOnlySpan<byte> data)
        {
            var sceneData = MessagePackSerializer.Deserialize<SceneData>(data.ToArray());
            return SceneSerializer.Deserialize(sceneData);
        }
    }
}
