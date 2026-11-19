using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public sealed class PostProcessGraph
    {
        private readonly List<PostProcessPass> _passes = [];
        private readonly List<PostProcessPass> _compiled = [];

        private readonly Dictionary<string, PostProcessPass> _producers = [];

        private readonly Dictionary<PostProcessPass, List<PostProcessPass>> _edges = [];
        private readonly Dictionary<PostProcessPass, int> _incoming = [];
        private readonly Queue<PostProcessPass> _ready = [];

        private bool _dirty = true;

        public void AddPass(PostProcessPass pass)
        {
            pass.Setup();
            _passes.Add(pass);
            _dirty = true;
        }

        public void RemovePass(PostProcessPass pass)
        {
            _passes.Remove(pass);
            _dirty = true;
        }

        public IReadOnlyList<PostProcessPass> Passes => _passes;

        private void Compile()
        {
            _producers.Clear();

            foreach (var pass in _passes)
            {
                foreach (var output in pass.Writes)
                    _producers[output] = pass;
            }

            ResolveOrder();

            _dirty = false;
        }

        private void ResolveOrder()
        {
            _compiled.Clear();
            _edges.Clear();
            _incoming.Clear();
            _ready.Clear();

            foreach (var pass in _passes)
            {
                _edges[pass] = [];
                _incoming[pass] = 0;
            }

            foreach (var pass in _passes)
            {
                foreach (var input in pass.Reads)
                {
                    if (!_producers.TryGetValue(input, out var producer))
                        continue;

                    if (producer == pass)
                        continue;

                    _edges[producer].Add(pass);
                    _incoming[pass]++;
                }
            }

            foreach (var pass in _passes)
            {
                if (_incoming[pass] == 0)
                    _ready.Enqueue(pass);
            }

            while (_ready.Count > 0)
            {
                var pass = _ready.Dequeue();

                _compiled.Add(pass);

                foreach (var dependent in _edges[pass])
                {
                    _incoming[dependent]--;

                    if (_incoming[dependent] == 0)
                        _ready.Enqueue(dependent);
                }
            }

            if (_compiled.Count != _passes.Count)
                throw new Exception("Cycle detected in post process graph.");
        }

        public Texture Execute(PostProcessContext context, Texture sceneColor)
        {
            if (_dirty)
                Compile();

            context.Reset();

            context.SetTexture("SceneColor", sceneColor);

            Texture current = sceneColor;

            foreach (var pass in _compiled)
            {
                pass.Execute(context);

                if (pass.Writes.Count > 0)
                {
                    current = context.GetTexture(pass.Writes[^1])!;
                }
            }

            return current!;
        }

        public void Resize(int width, int height)
        {
            foreach (var pass in _passes)
                pass.Resize(width, height);
        }
    }
}
