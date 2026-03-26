using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public struct PhysicsStaticDescription
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public PhysicsShapeDescription Shape;

        public PhysicsMaterial Material;   // ADD THIS
    }

}