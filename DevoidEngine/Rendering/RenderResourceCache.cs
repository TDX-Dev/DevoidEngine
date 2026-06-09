using DevoidEngine.Core;
using DevoidGPU;

namespace DevoidEngine.Rendering
{

    public sealed class RenderResourceCache : IDisposable
    {
        private readonly IGraphicsDevice device;

        private readonly Dictionary<TextureResourceKey, Texture> textures;

        public RenderResourceCache(IGraphicsDevice device)
        {
            this.device = device;
            textures = [];
        }

        public Texture GetOrCreateTexture(
            string name,
            TextureDescription description)
        {
            var key = new TextureResourceKey(name, description);

            if (textures.TryGetValue(key, out var texture))
                return texture;

            ITexture gpuTexture = device.CreateTexture(description);
            texture = new(gpuTexture);
            Console.WriteLine("Texture Created for: " + description.Width + " x " + description.Height);

            textures.Add(key, texture);

            return texture;
        }

        public bool TryGetTexture(
            string name,
            TextureDescription description,
            out Texture texture)
        {
            return textures.TryGetValue(
                new TextureResourceKey(name, description),
                out texture!);
        }

        public void RemoveTexture(
            string name,
            TextureDescription description)
        {
            var key = new TextureResourceKey(name, description);

            if (!textures.TryGetValue(key, out var texture))
                return;

            texture.Dispose();
            textures.Remove(key);
        }

        public void Clear()
        {
            Console.WriteLine($"Clearing: {textures.Count} Textures");

            foreach (var texture in textures.Values)
                texture.Dispose();

            textures.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
