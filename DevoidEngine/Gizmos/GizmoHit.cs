using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public readonly struct GizmoHit
    {
        public readonly uint Id;
        public readonly float Distance;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public GizmoHit(
            uint id,
            float distance,
            Vector3 position,
            Vector3 normal)
        {
            Id = id;
            Distance = distance;
            Position = position;
            Normal = normal;
        }
    }
}
