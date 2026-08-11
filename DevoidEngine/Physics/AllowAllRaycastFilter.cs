using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics
{
    internal readonly struct AllowAllRaycastFilter : IRaycastFilter
    {
        public bool Allow(GameObject gameObject)
        {
            return true;
        }
    }
}
