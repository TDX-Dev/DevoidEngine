using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public struct PhysicsShapeDescription
    {
        public PhysicsShapeType Type;

        // Box
        public Vector3 Size;

        // Sphere
        public float Radius;

        // Capsule
        public float Height;

        // Convex / Mesh
        public Vector3[] Vertices;
        public int[] Indices; // Needed for Mesh
    }
}