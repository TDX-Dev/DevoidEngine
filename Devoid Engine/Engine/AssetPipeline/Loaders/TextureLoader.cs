using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class TextureLoader : IAssetLoader<Texture2D>
    {
        public Texture2D Load(ReadOnlySpan<byte> data)
        {
            return Helper.LoadImageAsTex(data, DevoidGPU.TextureFilter.Linear);
        }
    }
}
