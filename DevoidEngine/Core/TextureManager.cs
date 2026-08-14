using System;
using System.Collections.Generic;

namespace DevoidEngine.Core
{
    public sealed class TextureManager
    {
        private ulong _nextId = 1;

        private readonly Dictionary<ulong, Texture> _textures = [];
        private readonly Dictionary<Texture, ulong> _ids = [];

        public ulong Register(Texture texture)
        {
            if (texture == null) return 0;

            // Return existing ID if already registered
            if (_ids.TryGetValue(texture, out ulong existingId))
                return existingId;

            ulong id = _nextId++;

            _textures.Add(id, texture);
            _ids.Add(texture, id);

            return id;
        }

        public ulong GetId(Texture texture)
        {
            if (texture == null) return 0;

            if (_ids.TryGetValue(texture, out ulong id))
                return id;

            return Register(texture);
        }

        public void Unregister(Texture texture)
        {
            if (texture == null) return;

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