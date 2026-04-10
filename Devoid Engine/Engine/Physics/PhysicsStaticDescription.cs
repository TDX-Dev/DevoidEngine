using MessagePack;
using System.Numerics;

namespace DevoidEngine.Engine.Physics
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
    }
}