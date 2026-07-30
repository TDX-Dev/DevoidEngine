using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public interface ISky : IDisposable
    {
        MaterialInstance Material { get; }

        Mesh Mesh { get; }

        bool Dirty { get; }

        

        void BuildEnvironment(RenderContext context);
    }
}
