using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    public class RenderGraph
    {
        List<RenderGraphPass> passes = new();
        List<RenderGraphPass> compiledPasses = new();

        Dictionary<string, RenderGraphPass> producers = new();

        bool dirty = true;

        public void AddPass(RenderGraphPass pass)
        {
            pass.Setup();
            passes.Add(pass);
            dirty = true;
        }

        public void RemovePass(RenderGraphPass pass)
        {
            passes.Remove(pass);
            dirty = true;
        }

        public void Clear()
        {
            passes.Clear();
            compiledPasses.Clear();
            producers.Clear();
            dirty = true;
        }

        public void Compile()
        {
            producers.Clear();

            foreach (var pass in passes)
            {
                foreach (var write in pass.Writes)
                {
                    producers[write] = pass;
                }
            }

            compiledPasses = ResolvePassOrder();
            dirty = false;
            PrintExecutionOrder();
        }

        public Texture2D Execute(Texture2D sceneColor)
        {
            if (dirty)
                Compile();

            var ctx = new RenderGraphContext();

            ctx.SetTexture("SceneColor", sceneColor);

            string lastWritten = null;

            foreach (var pass in compiledPasses)
            {
                pass.Execute(ctx);

                if (pass.Writes.Count > 0)
                    lastWritten = pass.Writes[pass.Writes.Count - 1];
            }

            if (lastWritten != null)
                return ctx.GetTexture(lastWritten);

            return sceneColor;
        }

        public void Resize(int width, int height)
        {
            foreach (var pass in passes)
            {
                pass.Resize(width, height);
            }
        }

        List<RenderGraphPass> ResolvePassOrder()
        {
            Dictionary<RenderGraphPass, List<RenderGraphPass>> edges = new();
            Dictionary<RenderGraphPass, int> incoming = new();

            foreach (var pass in passes)
            {
                edges[pass] = new();
                incoming[pass] = 0;
            }

            foreach (var pass in passes)
            {
                foreach (var read in pass.Reads)
                {
                    if (!producers.TryGetValue(read, out var producer))
                        continue;

                    if (producer == pass)
                        continue;

                    edges[producer].Add(pass);
                    incoming[pass]++;
                }
            }

            Queue<RenderGraphPass> ready = new();

            foreach (var pass in passes)
                if (incoming[pass] == 0)
                    ready.Enqueue(pass);

            List<RenderGraphPass> result = new();

            while (ready.Count > 0)
            {
                var p = ready.Dequeue();
                result.Add(p);

                foreach (var dep in edges[p])
                {
                    incoming[dep]--;

                    if (incoming[dep] == 0)
                        ready.Enqueue(dep);
                }
            }

            if (result.Count != passes.Count)
                throw new Exception("RenderGraph contains a cycle.");

            return result;
        }

        public void PrintExecutionOrder()
        {

            Console.WriteLine("=== RenderGraph Execution Order ===");

            for (int i = 0; i < compiledPasses.Count; i++)
            {
                var pass = compiledPasses[i];

                Console.WriteLine($"{i}: {pass.GetType().Name}");

                if (pass.Reads.Count > 0)
                    Console.WriteLine($"   Reads : {string.Join(", ", pass.Reads)}");

                if (pass.Writes.Count > 0)
                    Console.WriteLine($"   Writes: {string.Join(", ", pass.Writes)}");
            }

            Console.WriteLine("===================================");
        }
    }
}