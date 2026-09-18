using DevoidEngine.Core;
using DevoidEngine.Nodes;

namespace DevoidEngine.Physics
{
    internal readonly struct AllowAllRaycastFilter : IRaycastFilter
    {
        public bool Allow(Node3D node)
        {
            return true;
        }
    }
}
