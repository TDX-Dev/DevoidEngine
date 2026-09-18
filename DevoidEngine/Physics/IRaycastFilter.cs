using DevoidEngine.Core;
using DevoidEngine.Nodes;

namespace DevoidEngine.Physics
{
    public interface IRaycastFilter
    {
        bool Allow(Node3D node);
    }
}
