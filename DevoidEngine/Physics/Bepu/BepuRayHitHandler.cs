using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics.Bepu
{
    internal struct BepuRayHitHandler : IRayHitHandler
    {
        public bool Hit;
        public float Distance;
        public Vector3 Normal;
        public CollidableReference Collidable;

        private readonly IPhysicsBackend backend;

        public BepuRayHitHandler(IPhysicsBackend backend)
        {
            this.backend = backend;
        }

        public readonly bool AllowTest(CollidableReference collidable)
        {
            if (backend.IsTrigger(collidable))
                return false;

            return true;
        }

        public readonly bool AllowTest(CollidableReference collidable, int childIndex) => true;

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
