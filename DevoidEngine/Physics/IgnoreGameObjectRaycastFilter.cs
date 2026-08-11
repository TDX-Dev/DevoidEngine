using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics
{
    public readonly struct IgnoreGameObjectRaycastFilter : IRaycastFilter
    {
        private readonly GameObject _ignored;

        public IgnoreGameObjectRaycastFilter(GameObject ignored)
        {
            _ignored = ignored;
        }

        public bool Allow(GameObject gameObject)
        {
            return gameObject != _ignored;
        }
    }
}
