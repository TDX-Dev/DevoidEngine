using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Physics
{
    public class PhysicsSystem
    {
        private readonly IPhysicsBackend backend;

        private readonly Dictionary<IPhysicsObject, GameObject> objectMap = new();

        private readonly HashSet<(IPhysicsObject, IPhysicsObject)> currentPairs = new();

        private readonly Dictionary<(IPhysicsObject, IPhysicsObject), int> contactStates
            = new();

        private const int ExitGraceFrames = 1;


        public PhysicsSystem(IPhysicsBackend backend)
        {
            this.backend = backend;
            backend.Initialize();
            backend.CollisionDetected += OnBackendCollision;
        }

        public void Step(float fixedDelta)
        {
            // Only advance simulation
            backend.Step(fixedDelta);

            // DO NOT dispatch enter/stay/exit here
            // We only accumulate contact pairs during substeps
        }

        public void ResolveFrameCollisions()
        {
            // 1️⃣ Mark all existing contacts as seen this frame
            foreach (var pair in currentPairs)
            {
                if (!contactStates.ContainsKey(pair))
                {
                    // New contact
                    contactStates[pair] = 0;
                    DispatchEnter(pair);
                }
                else
                {
                    // Existing contact
                    contactStates[pair] = 0;
                    DispatchStay(pair);
                }
            }

            // 2️⃣ Process missing contacts
            var toRemove = new List<(IPhysicsObject, IPhysicsObject)>();

            foreach (var kvp in contactStates)
            {
                var pair = kvp.Key;

                if (!currentPairs.Contains(pair))
                {
                    contactStates[pair]++;

                    if (contactStates[pair] > ExitGraceFrames)
                    {
                        DispatchExit(pair);
                        toRemove.Add(pair);
                    }
                }
            }

            // 3️⃣ Remove expired contacts
            foreach (var pair in toRemove)
                contactStates.Remove(pair);

            // 4️⃣ Clear frame accumulation
            currentPairs.Clear();
        }

        public void SyncTransforms()
        {
            foreach (var pair in objectMap)
            {
                if (pair.Key is not IPhysicsBody body)
                    continue;

                if (body.IsKinematic)
                    continue;

                var go = pair.Value;

                // apply new physics pose
                go.Transform.Position = body.Position;
                go.Transform.Rotation = body.Rotation;
            }
        }

        private void OnBackendCollision(IPhysicsObject a, IPhysicsObject b)
        {
            if (a == null || b == null)
                return;

            currentPairs.Add(NormalizePair(a, b));
        }

        private static (IPhysicsObject, IPhysicsObject) NormalizePair(
            IPhysicsObject a,
            IPhysicsObject b)
        {
            return a.Id < b.Id ? (a, b) : (b, a);
        }

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