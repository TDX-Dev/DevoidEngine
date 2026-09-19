using DevoidEngine.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Tools
{
    public class NodeIcons
    {
        public static Dictionary<Type, string> NodeIconMapping = new Dictionary<Type, string>()
        {
            { typeof(Node), LucideIconFont.IconLineDotRightHorizontal },
            { typeof(Node3D), LucideIconFont.IconScale3d },
            { typeof(MeshNode), LucideIconFont.IconBox },
        };


    }
}
