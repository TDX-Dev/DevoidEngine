using DevoidEngine.Core;
using DevoidEngine.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics
{
    public readonly struct IgnoreGameObjectRaycastFilter : IRaycastFilter
    {
        private readonly Node3D _ignored;

        public IgnoreGameObjectRaycastFilter(Node3D ignored)
        {
            _ignored = ignored;
        }

        public bool Allow(Node3D node)
        {
            return node != _ignored;
        }
    }
}
