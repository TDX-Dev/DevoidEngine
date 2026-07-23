using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics
{
    public struct PhysicsBodyDescription
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public bool AllowSleep;
        public float Mass;
        public bool IsKinematic;
        public bool IsTrigger;

        public bool AllowRotationX;
        public bool AllowRotationY;
        public bool AllowRotationZ;

        public PhysicsShapeDescription Shape;

        public PhysicsMaterial Material;

        public PhysicsCollisionDetectionSettings CollisionDetectionSettings;
    }
}
