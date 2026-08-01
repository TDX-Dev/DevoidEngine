using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public interface ISky : IDisposable
    {

        Mesh Mesh { get; }

        bool Dirty { get; set; }

        

        void BuildEnvironment(RenderContext context, EnvironmentLighting environment);
    }
}
