using DevoidEngine.Core;
using DevoidGPU;

namespace DevoidEngine.Rendering.RG
{
    public sealed class RenderGraphBuilder
    {
        private readonly RenderGraph _graph;
        private readonly List<PassDependency> _dependencies = [];

        internal IReadOnlyList<PassDependency> Dependencies => _dependencies;

        internal RenderGraphBuilder(RenderGraph graph)
        {
            this._graph = graph;
        }

        public RenderGraphHandle CreateTexture(TextureDescription description)
        {
            return _graph.CreateTexture(description);
        }

        public RenderGraphHandle ImportTexture(Texture texture)
        {
            return _graph.ImportTexture(texture);
        }

        public RenderGraphHandle FindTexture(string name)
        {
            return _graph.FindTexture(name);
        }

        public void Read(RenderGraphHandle resource)
        {
            if (!resource.IsValid)
                return;
            _dependencies.Add(new PassDependency(resource, RenderGraphAccessType.Read));
        }

        public void Write(RenderGraphHandle resource)
        {
            if (!resource.IsValid)
                return;
            _dependencies.Add(new PassDependency(resource, RenderGraphAccessType.Write));
        }

        public void ReadWrite(RenderGraphHandle resource)
        {
            if (!resource.IsValid)
                return;
            _dependencies.Add(new PassDependency(resource, RenderGraphAccessType.ReadWrite));
        }

        public void Use(RenderGraphHandle resource, RenderGraphAccessType access)
        {
            if (!resource.IsValid)
                return;
            _dependencies.Add(new PassDependency(resource, access));
        }
    }
}
