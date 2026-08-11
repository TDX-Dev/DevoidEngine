using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Physics.Bepu
{
    internal struct BepuRayHitHandler<TFilter> : IRayHitHandler
        where TFilter : struct, IRaycastFilter
    {
        public bool Hit;
        public float Distance;
        public Vector3 Normal;
        public CollidableReference Collidable;

        private readonly IPhysicsBackend backend;
        private readonly TFilter filter;

        public BepuRayHitHandler(
            IPhysicsBackend backend,
            TFilter filter)
        {
            this.backend = backend;
            this.filter = filter;
        }

        public readonly bool AllowTest(
            CollidableReference collidable)
        {
            if (backend.IsTrigger(collidable))
                return false;

            if (!backend.TryGetGameObject(
                    collidable,
                    out GameObject gameObject))
            {
                return false;
            }

            return filter.Allow(gameObject);
        }

        public readonly bool AllowTest(
            CollidableReference collidable,
            int childIndex)
        {
            return AllowTest(collidable);
        }

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

            maximumT = t;
        }
    }
}