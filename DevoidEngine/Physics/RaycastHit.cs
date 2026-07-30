using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Physics
{
    public struct RaycastHit
    {
        public Vector3 Point;
        public Vector3 Normal;
        public float Distance;

        public GameObject HitObject;
    }
}
