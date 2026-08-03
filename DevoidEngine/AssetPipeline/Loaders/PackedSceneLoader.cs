using DevoidEngine.Assets;
using DevoidEngine.Audio;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline.Loaders
{
    public class PackedSceneLoader : IAssetLoader<PackedScene>
    {
        public string RuntimeExtension => "packedscene";
        public PackedScene Load(byte[] data)
        {
            PackedScene asset;
            try
            {
                asset = MessagePackSerializer.Deserialize<PackedScene>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[PackedScene Loader]: Error Loading PackedScene {e.Message}");
                throw new Exception();
            }

            return asset;
        }
    }
}
