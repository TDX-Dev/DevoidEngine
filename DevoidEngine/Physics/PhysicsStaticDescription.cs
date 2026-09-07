using MessagePack;
using System.Numerics;

namespace DevoidEngine.Physics
{
    [MessagePackObject]
    public struct PhysicsStaticDescription
    {
        [Key(0)]
        public Vector3 Position;

        [Key(1)]
        public Quaternion Rotation;

        [Key(2)]
        public PhysicsShapeDescription Shape;

        [Key(3)]
        public PhysicsMaterial Material;
        [Key(4)]
        public Vector3 ShapePosition;
        [Key(5)]
        public Quaternion ShapeRotation;
    }
}
