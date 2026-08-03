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
        private readonly List<PostProcessPass> passes = [];
        private readonly List<PostProcessPass> compiled = [];

        private readonly Dictionary<string, PostProcessPass> producers = [];

        private readonly Dictionary<PostProcessPass, List<PostProcessPass>> edges = [];
        private readonly Dictionary<PostProcessPass, int> incoming = [];
        private readonly Queue<PostProcessPass> ready = [];

        private bool dirty = true;

        public void AddPass(PostProcessPass pass)
        {
            pass.Setup();
            passes.Add(pass);
            dirty = true;
        }

        public void RemovePass(PostProcessPass pass)
        {
            passes.Remove(pass);
            dirty = true;
        }

        public IReadOnlyList<PostProcessPass> Passes => passes;

        private void Compile()
        {
            producers.Clear();

            foreach (var pass in passes)
            {
                foreach (var output in pass.Writes)
                    producers[output] = pass;
            }

            ResolveOrder();

            dirty = false;
        }

        private void ResolveOrder()
        {
            compiled.Clear();
            edges.Clear();
            incoming.Clear();
            ready.Clear();

            foreach (var pass in passes)
            {
                edges[pass] = [];
                incoming[pass] = 0;
            }

            foreach (var pass in passes)
            {
                foreach (var input in pass.Reads)
                {
                    if (!producers.TryGetValue(input, out var producer))
                        continue;

                    if (producer == pass)
                        continue;

                    edges[producer].Add(pass);
                    incoming[pass]++;
                }
            }

            foreach (var pass in passes)
            {
                if (incoming[pass] == 0)
                    ready.Enqueue(pass);
            }

            while (ready.Count > 0)
            {
                var pass = ready.Dequeue();

                compiled.Add(pass);

                foreach (var dependent in edges[pass])
                {
                    incoming[dependent]--;

                    if (incoming[dependent] == 0)
                        ready.Enqueue(dependent);
                }
            }

            if (compiled.Count != passes.Count)
                throw new Exception("Cycle detected in post process graph.");
        }

        public Texture Execute(PostProcessContext context, Texture sceneColor)
        {
            if (dirty)
                Compile();

            context.Reset();

            context.SetTexture("SceneColor", sceneColor);

            Texture current = sceneColor;

            foreach (var pass in compiled)
            {
                pass.Execute(context);

                if (pass.Writes.Count > 0)
                {
                    current = context.GetTexture(pass.Writes[^1]);
                }
            }

            return current;
        }

        public void Resize(int width, int height)
        {
            foreach (var pass in passes)
                pass.Resize(width, height);
        }
    }
}
