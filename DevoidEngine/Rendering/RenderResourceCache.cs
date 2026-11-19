using DevoidEngine.Core;
using DevoidGPU;

namespace DevoidEngine.Rendering
{

    public sealed class RenderResourceCache : IDisposable
    {
        private readonly IGraphicsDevice _device;

        private readonly Dictionary<TextureResourceKey, Texture> _textures;

        public IEnumerable<Texture> Textures => _textures.Values;
        public int NumTextures => _textures.Count;

        public RenderResourceCache(IGraphicsDevice device)
        {
            this._device = device;
            _textures = [];
        }

        public Texture GetOrCreateTexture(string name, TextureDescription description)
        {
            var key = new TextureResourceKey(name, description);

            if (_textures.TryGetValue(key, out var texture))
                return texture;

            ITexture gpuTexture = _device.CreateTexture(description);
            texture = new(gpuTexture);
            //Console.WriteLine("Texture Created for: " + description.Width + " x " + description.Height);

            _textures.Add(key, texture);

            return texture;
        }

        public bool TryGetTexture(string name, TextureDescription description, out Texture texture)
        {
            return _textures.TryGetValue(
                new TextureResourceKey(name, description),
                out texture!);
        }

        public void RemoveTexture(string name, TextureDescription description)
        {
            var key = new TextureResourceKey(name, description);

            if (!_textures.TryGetValue(key, out var texture))
                return;

            texture.Dispose();
            _textures.Remove(key);
        }

        public IEnumerable<Texture> GetTextures()
        {
            return _textures.Values;
        }

        public void Clear()
        {
            //Console.WriteLine($"Clearing: {textures.Count} Textures");

            foreach (var texture in _textures.Values)
                texture.Dispose();

            _textures.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
