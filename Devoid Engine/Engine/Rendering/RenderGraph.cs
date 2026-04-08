using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;

public class RenderGraph
{
    readonly List<RenderGraphPass> passes = new();
    readonly List<RenderGraphPass> compiledPasses = new();

    readonly Dictionary<string, RenderGraphPass> producers = new();

    readonly Dictionary<RenderGraphPass, List<RenderGraphPass>> edges = new();
    readonly Dictionary<RenderGraphPass, int> incoming = new();
    readonly Queue<RenderGraphPass> ready = new();

    readonly RenderGraphContext ctx = new();

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

    public List<RenderGraphPass> GetPasses() => passes;

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

        for (int i = 0; i < passes.Count; i++)
        {
            var pass = passes[i];

            for (int w = 0; w < pass.Writes.Count; w++)
            {
                producers[pass.Writes[w]] = pass;
            }
        }

        ResolvePassOrder();
        dirty = false;

        //PrintExecutionOrder();
    }

    public Texture2D Execute(Texture2D sceneColor, CameraRenderContext frame)
    {
        if (dirty)
            Compile();

        ctx.Reset();
        ctx.FrameContext = frame;
        ctx.SetTexture("SceneColor", sceneColor);

        Texture2D lastTexture = sceneColor;

        for (int i = 0; i < compiledPasses.Count; i++)
        {
            var pass = compiledPasses[i];
            pass.Execute(ctx);

            if (pass.Writes.Count > 0)
            {
                var name = pass.Writes[pass.Writes.Count - 1];
                lastTexture = ctx.GetTexture(name);
            }
        }

        return lastTexture;
    }

    public void Resize(int width, int height)
    {
        for (int i = 0; i < passes.Count; i++)
            passes[i].Resize(width, height);
    }

    void ResolvePassOrder()
    {
        edges.Clear();
        incoming.Clear();
        ready.Clear();
        compiledPasses.Clear();

        for (int i = 0; i < passes.Count; i++)
        {
            var pass = passes[i];
            edges[pass] = new List<RenderGraphPass>();
            incoming[pass] = 0;
        }

        for (int i = 0; i < passes.Count; i++)
        {
            var pass = passes[i];

            for (int r = 0; r < pass.Reads.Count; r++)
            {
                var read = pass.Reads[r];

                if (!producers.TryGetValue(read, out var producer))
                    continue;

                if (producer == pass)
                    continue;

                edges[producer].Add(pass);
                incoming[pass]++;
            }
        }

        for (int i = 0; i < passes.Count; i++)
        {
            var pass = passes[i];

            if (incoming[pass] == 0)
                ready.Enqueue(pass);
        }

        while (ready.Count > 0)
        {
            var p = ready.Dequeue();
            compiledPasses.Add(p);

            var deps = edges[p];

            for (int i = 0; i < deps.Count; i++)
            {
                var dep = deps[i];

                incoming[dep]--;

                if (incoming[dep] == 0)
                    ready.Enqueue(dep);
            }
        }

        if (compiledPasses.Count != passes.Count)
            throw new Exception("RenderGraph contains a cycle.");
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