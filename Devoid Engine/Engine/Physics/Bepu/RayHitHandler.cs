using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using System.Numerics;

namespace DevoidEngine.Engine.Physics.Bepu
{

    internal struct RayHitHandler : IRayHitHandler
    {
        public bool Hit;
        public float Distance;
        public Vector3 Normal;
        public CollidableReference Collidable;

        private IPhysicsBackend backend;

        public RayHitHandler(IPhysicsBackend backend)
        {
            this.backend = backend;
        }

        public bool AllowTest(CollidableReference collidable)
        {
            if (backend.IsTrigger(collidable))
                return false;

            return true;
        }

        public bool AllowTest(CollidableReference collidable, int childIndex) => true;

        public void OnRayHit(
            in RayData ray,
            ref float maximumT,
            float t,
            in Vector3 normal,
            CollidableReference collidable,
            int childIndex)
        {
            Hit = true;
            Distance = t;
            Normal = normal;
            Collidable = collidable;

            maximumT = t; // keep closest hit
        }
    }


}