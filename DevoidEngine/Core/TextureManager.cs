using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class TextureManager
    {
        private ulong _nextId = 1;

        private readonly Dictionary<ulong, Texture> _textures = [];
        private readonly Dictionary<Texture, ulong> _ids = [];

        internal ulong Register(Texture texture)
        {
            ulong id = _nextId++;

            _textures.Add(id, texture);
            _ids.Add(texture, id);

            return id;
        }

        internal void Unregister(Texture texture)
        {
            if (!_ids.Remove(texture, out ulong id))
                return;

            _textures.Remove(id);
        }

        public Texture Get(ulong id)
        {
            return _textures[id];
        }

        public bool TryGet(ulong id, out Texture texture)
        {
            return _textures.TryGetValue(id, out texture!);
        }
    }
}
