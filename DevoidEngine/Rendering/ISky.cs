using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public interface ISky
    {
        MaterialInstance Material { get; }
        Mesh Mesh { get; }
    }
}
