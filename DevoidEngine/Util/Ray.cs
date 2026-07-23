using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction; // MUST be normalized

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = Vector3.Normalize(direction);
        }

        public readonly Vector3 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }
    }
}
