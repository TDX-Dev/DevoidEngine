using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Physics
{
    public struct PhysicsBodyDescription
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public bool AllowSleep;
        public float Mass;
        public bool IsKinematic;

        public PhysicsShapeDescription Shape;

        public PhysicsMaterial Material;
    }

}
