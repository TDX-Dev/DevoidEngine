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

        public PhysicsShapeDescription Shape;

        public PhysicsMaterial Material;
    }

}