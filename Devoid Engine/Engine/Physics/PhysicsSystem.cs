using DevoidEngine.Engine.Core;
using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public class PhysicsSystem
    {
        private readonly IPhysicsBackend backend;

        // Maps physics objects → game objects
        private readonly Dictionary<IPhysicsObject, GameObject> objectMap = new();

        // Accumulated during physics substeps (frame-wide)
        private readonly HashSet<(IPhysicsObject, IPhysicsObject)> currentPairs = new();

        // Last frame’s stable contact set
        private HashSet<(IPhysicsObject, IPhysicsObject)> previousPairs = new();

        public PhysicsSystem(IPhysicsBackend backend)
        {
            this.backend = backend;
            backend.Initialize();
            backend.CollisionDetected += OnBackendCollision;
        }

        // ---------------------------------------------------------
        // Physics Substep (called multiple times per frame)
        // ---------------------------------------------------------
        public void Step(float fixedDelta)
        {
            // Only advance simulation
            backend.Step(fixedDelta);

            // DO NOT dispatch enter/stay/exit here
            // We only accumulate contact pairs during substeps
        }

        // ---------------------------------------------------------
        // Frame-Level Collision Resolution (call once per frame)
        // ---------------------------------------------------------
        public void ResolveFrameCollisions()
        {
            //Console.WriteLine($"Resolve: current={currentPairs.Count}, previous={previousPairs.Count}");
            // ENTER + STAY
            foreach (var pair in currentPairs)
            {
                if (!previousPairs.Contains(pair))
                {
                    DispatchEnter(pair);
                }
                else
                {
                    DispatchStay(pair);
                }
            }

            // EXIT
            foreach (var pair in previousPairs)
            {
                if (!currentPairs.Contains(pair))
                {
                    DispatchExit(pair);
                }
            }

            // Move current → previous
            previousPairs = new HashSet<(IPhysicsObject, IPhysicsObject)>(currentPairs);

            // Clear accumulation for next frame
            currentPairs.Clear();
        }

        // ---------------------------------------------------------
        // Contact Collection (called from backend during substeps)
        // ---------------------------------------------------------
        private void OnBackendCollision(IPhysicsObject a, IPhysicsObject b)
        {
            if (a == null || b == null)
                return;

            currentPairs.Add(NormalizePair(a, b));
        }

        // Stable ordering so (A,B) == (B,A)
        private static (IPhysicsObject, IPhysicsObject) NormalizePair(
            IPhysicsObject a,
            IPhysicsObject b)
        {
            //Console.WriteLine("---- NormalizePair ----");

            //Console.WriteLine(
            //    $"A -> Id:{a.Id} | RefHash:{a.GetHashCode()} | Type:{a.GetType().Name}"
            //);

            //Console.WriteLine(
            //    $"B -> Id:{b.Id} | RefHash:{b.GetHashCode()} | Type:{b.GetType().Name}"
            //);

            //Console.WriteLine(
            //    $"ReferenceEquals(A,B): {ReferenceEquals(a, b)}"
            //);

            //Console.WriteLine("------------------------");

            return a.Id < b.Id ? (a, b) : (b, a);
        }

        // ---------------------------------------------------------
        // Public API
        // ---------------------------------------------------------

        public bool Raycast(Ray ray, float maxDistance, out RaycastHit hit)
        {
            return backend.Raycast(ray, maxDistance, out hit);
        }

        public IPhysicsBody CreateBody(PhysicsBodyDescription desc, GameObject owner)
        {
            var body = backend.CreateBody(desc, owner);
            objectMap[body] = owner;
            return body;
        }

        public IPhysicsStatic CreateStatic(PhysicsStaticDescription desc, GameObject owner)
        {
            var stat = backend.CreateStatic(desc, owner);
            objectMap[stat] = owner;
            return stat;
        }

        public void RemoveBody(IPhysicsBody body)
        {
            objectMap.Remove(body);
            backend.RemoveBody(body);
        }

        public void RemoveStatic(IPhysicsStatic stat)
        {
            objectMap.Remove(stat);
            backend.RemoveStatic(stat);
        }

        // ---------------------------------------------------------
        // Dispatch Helpers
        // ---------------------------------------------------------

        private void DispatchEnter((IPhysicsObject, IPhysicsObject) pair)
        {
            if (objectMap.TryGetValue(pair.Item1, out var goA) &&
                objectMap.TryGetValue(pair.Item2, out var goB))
            {
                goA.InvokeCollisionEnter(goB);
                goB.InvokeCollisionEnter(goA);
            }
        }

        private void DispatchStay((IPhysicsObject, IPhysicsObject) pair)
        {
            if (objectMap.TryGetValue(pair.Item1, out var goA) &&
                objectMap.TryGetValue(pair.Item2, out var goB))
            {
                goA.InvokeCollisionStay(goB);
                goB.InvokeCollisionStay(goA);
            }
        }

        private void DispatchExit((IPhysicsObject, IPhysicsObject) pair)
        {
            if (objectMap.TryGetValue(pair.Item1, out var goA) &&
                objectMap.TryGetValue(pair.Item2, out var goB))
            {
                goA.InvokeCollisionExit(goB);
                goB.InvokeCollisionExit(goA);
            }
        }
    }
}