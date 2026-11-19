namespace DevoidEngine.Rendering.RG
{
    public sealed class RenderGraph
    {
        private readonly List<RenderPass> _passes = [];
        private readonly List<int> _executionOrder = [];

        private List<HashSet<int>> _adjacency = [];
        private List<PassDependency>[] _passDependencies = [];
        private Dictionary<string, RenderGraphHandle> _resourcesByName = [];

        private List<PassDependency>[] _passDependencies = [];


        public void Compile()
        {

            _executionOrder.Clear();

            CapturePasses();
        }

        void CapturePasses()
        {

        }
    }
}
