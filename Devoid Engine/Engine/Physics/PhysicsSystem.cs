using BepuPhysics.Collidables;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics.Bepu;
using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public class PhysicsSystem
    {
        private readonly IPhysicsBackend backend;

        private Dictionary<IPhysicsBody, GameObject> bodyMap = new();
        private HashSet<(IPhysicsBody, IPhysicsBody)> currentPairs = new();
        private HashSet<(IPhysicsBody, IPhysicsBody)> previousPairs = new();

        public PhysicsSystem(IPhysicsBackend backend)
        {
            this.backend = backend;
            backend.Initialize();

            backend.CollisionDetected += OnBackendCollision;
        }

        private static (IPhysicsBody, IPhysicsBody) NormalizePair(IPhysicsBody a, IPhysicsBody b)
        {
            var handleA = ((BepuPhysicsBody)a).Handle.Value;
            var handleB = ((BepuPhysicsBody)b).Handle.Value;

            return handleA < handleB ? (a, b) : (b, a);
        }

        private void OnBackendCollision(IPhysicsBody a, IPhysicsBody b)
        {
            if (a == null || b == null)
                return;

            currentPairs.Add(NormalizePair(a, b));
        }

        public void Step(float frameDelta)
        {
            backend.Step(frameDelta);

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

            foreach (var pair in previousPairs)
            {
                if (!currentPairs.Contains(pair))
                {
                    DispatchExit(pair);
                }
            }

            previousPairs = new HashSet<(IPhysicsBody, IPhysicsBody)>(currentPairs);
            currentPairs.Clear();
        }

        public bool Raycast(Ray ray, float maxDistance, out RaycastHit hit)
        {
            return backend.Raycast(ray, maxDistance, out hit);
        }

        public IPhysicsBody CreateBody(PhysicsBodyDescription desc, GameObject owner)
        {
            IPhysicsBody body = backend.CreateBody(desc, owner);
            bodyMap[body] = owner;
            return body;
        }

        public void CreateStatic(PhysicsStaticDescription desc, GameObject owner)
        {
            backend.CreateStatic(desc, owner);
        }

        public void RemoveBody(IPhysicsBody body)
        {
            backend.RemoveBody(body);
        }

        private void DispatchEnter((IPhysicsBody, IPhysicsBody) pair)
        {
            if (bodyMap.TryGetValue(pair.Item1, out var goA) &&
                bodyMap.TryGetValue(pair.Item2, out var goB))
            {
                goA.InvokeCollisionEnter(goB);
                goB.InvokeCollisionEnter(goA);
            }
        }

        private void DispatchStay((IPhysicsBody, IPhysicsBody) pair)
        {
            if (bodyMap.TryGetValue(pair.Item1, out var goA) &&
                bodyMap.TryGetValue(pair.Item2, out var goB))
            {
                goA.InvokeCollisionStay(goB);
                goB.InvokeCollisionStay(goA);
            }
        }

        private void DispatchExit((IPhysicsBody, IPhysicsBody) pair)
        {
            if (bodyMap.TryGetValue(pair.Item1, out var goA) &&
                bodyMap.TryGetValue(pair.Item2, out var goB))
            {
                goA.InvokeCollisionExit(goB);
                goB.InvokeCollisionExit(goA);
            }
        }
    }
}
