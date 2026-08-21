using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    public readonly struct MedialBall
    {
        public readonly Vector3 Center;
        public readonly float Radius;

        public MedialBall(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
