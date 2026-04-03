using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class TextureLoader : IAssetLoader<Texture>
    {
        public Texture Load(ReadOnlySpan<byte> data)
        {
            return Texture.CreateFromMemory(data);
        }
    }
}
