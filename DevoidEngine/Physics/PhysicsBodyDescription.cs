using System.Numerics;

namespace DevoidEngine.Physics
{
    public struct PhysicsBodyDescription
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public Vector3 ShapePosition;
        public Quaternion ShapeRotation;

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
