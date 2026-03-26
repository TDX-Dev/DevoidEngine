using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction; // MUST be normalized

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = Vector3.Normalize(direction);
        }

        public Vector3 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }
    }

}