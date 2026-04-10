using MessagePack;
using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    [MessagePackObject]
    public struct PhysicsShapeDescription
    {
        [Key(0)]
        public PhysicsShapeType Type;

        [Key(1)]
        public Vector3 Size;

        [Key(2)]
        public float Radius;

        [Key(3)]
        public float Height;

        [Key(4)]
        public Vector3[] Vertices;

        [Key(5)]
        public int[] Indices;
    }
}