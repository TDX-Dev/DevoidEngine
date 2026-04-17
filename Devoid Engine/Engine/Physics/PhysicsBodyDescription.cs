using System.Numerics;

namespace DevoidEngine.Engine.Physics
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