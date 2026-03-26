using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace DevoidEngine.Engine.Physics.Bepu
{

    internal struct BepuNarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public Func<CollidableReference, PhysicsMaterial> MaterialLookup;
        public BepuPhysicsBackend Backend;

        public void Initialize(Simulation simulation)
        {

        }

        public void Dispose() { }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
            => true;

        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            return true;
        }

        public bool ConfigureContactManifold<TManifold>(
            int workerIndex,
            CollidablePair pair,
            ref TManifold manifold,
            out PairMaterialProperties pairMaterial)
            where TManifold : unmanaged, IContactManifold<TManifold>
        {
            if (Backend.IsTrigger(pair.A) || Backend.IsTrigger(pair.B))
            {
                pairMaterial = new PairMaterialProperties
                {
                    FrictionCoefficient = 0f,
                    MaximumRecoveryVelocity = 0f,
                    SpringSettings = new SpringSettings(30f, 1f)
                };
                Backend.ReportCollision(pair.A, pair.B);
                return false;
            }

            var matA = MaterialLookup(pair.A);
            var matB = MaterialLookup(pair.B);

            float friction = (matA.Friction + matB.Friction) * 0.5f;
            float restitution = (matA.Restitution + matB.Restitution) * 0.5f;

            float frequency = 30f;
            float dampingRatio = 1f;

            float angularFrequency = 2f * MathF.PI * frequency;
            float twiceDampingRatio = 2f * dampingRatio;

            var spring = new SpringSettings(angularFrequency, twiceDampingRatio);


            pairMaterial = new PairMaterialProperties
            {
                FrictionCoefficient = friction,
                MaximumRecoveryVelocity = 1,
                SpringSettings = spring
            };

            // Enable restitution
            //if (restitution > 0f)
            //{
            //    pairMaterial.SpringSettings = spring;
            //    pairMaterial.MaximumRecoveryVelocity = MathF.Max(
            //        pairMaterial.MaximumRecoveryVelocity,
            //        restitution * 10f
            //    );
            //}

            if (manifold.Count > 0)
            {
                //Backend.ReportCollision(pair.A, pair.B);
                //Console.WriteLine($"Registering Collision A:{pair.A.GetHashCode()} B:{pair.B.GetHashCode()}");
            }

            return true;
        }


        public bool ConfigureContactManifold(
            int workerIndex,
            CollidablePair pair,
            int childIndexA,
            int childIndexB,
            ref ConvexContactManifold manifold)
        {


            return true;
        }
    }

}