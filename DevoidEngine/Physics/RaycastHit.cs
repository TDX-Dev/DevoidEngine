using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics
{
    public struct RaycastHit
    {
        public Vector3 Point;
        public Vector3 Normal;
        public float Distance;

        public GameObject HitObject;
    }
}
