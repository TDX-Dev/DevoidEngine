using DevoidEngine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public sealed class PostProcessContext
    {
        private readonly Dictionary<string, Texture> textures = [];

        public Renderer Renderer = null!;
        public ICommandList CommandList = null!;
        public RenderContext RenderContext;

        public PostProcessSettings Settings { get; internal set; } = null!;

        public void Reset()
        {
            textures.Clear();
        }

        public void SetTexture(string name, Texture texture)
        {
            textures[name] = texture;
        }

        public Texture? GetTexture(string name)
        {
            if (!textures.TryGetValue(name, out Texture? texture))
                return null;

            return texture;
        }
    }
}
