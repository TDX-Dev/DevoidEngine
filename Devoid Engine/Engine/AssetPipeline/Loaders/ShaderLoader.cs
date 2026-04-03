using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class ShaderLoader : IAssetLoader<Shader>
    {
        public Shader Load(ReadOnlySpan<byte> data)
        {
            return Shader.CreateFromSource(data);
        }
    }
}
