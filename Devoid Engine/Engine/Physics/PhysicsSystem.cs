using BepuPhysics;
using DevoidEngine.Engine.Core;
using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public class PhysicsSystem
    {
        private readonly IPhysicsBackend backend;

        private readonly Dictionary<IPhysicsObject, GameObject> objectMap = new();

        private readonly HashSet<(IPhysicsObject, IPhysicsObject)> currentPairs = new();

        private readonly Dictionary<(IPhysicsObject, IPhysicsObject), int> contactStates
            = new();

        private readonly List<(IPhysicsObject, IPhysicsObject)> toRemove = new();

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

        }

        public void ResolveFrameCollisions()
        {

            foreach (var pair in currentPairs)
            {
                if (!contactStates.ContainsKey(pair))
                {

                    contactStates[pair] = 0;
                    DispatchEnter(pair);
                }
                else
                {

                    contactStates[pair] = 0;
                    DispatchStay(pair);
                }
            }

            toRemove.Clear();

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


            foreach (var pair in toRemove)
                contactStates.Remove(pair);


            currentPairs.Clear();
        }

        public void SyncTransforms(float dt)
        {
            foreach (var pair in objectMap)
            {
                if (pair.Key is not IPhysicsBody body)
                    continue;

                var go = pair.Value;

                if (body.IsKinematic)
                {
                    Vector3 targetPos = go.Transform.Position;
                    Quaternion targetRot = go.Transform.Rotation;

                    Vector3 currentPos = body.Position;
                    Quaternion currentRot = body.Rotation;

                    // Linear velocity
                    Vector3 linearVelocity = (targetPos - currentPos) / dt;

                    // Quaternion delta
                    Quaternion delta = targetRot * Quaternion.Inverse(currentRot);
                    delta = Quaternion.Normalize(delta);

                    // Convert to axis-angle
                    float angle = 2f * MathF.Acos(delta.W);
                    float sinHalfAngle = MathF.Sqrt(1f - delta.W * delta.W);

                    Vector3 axis;
                    if (sinHalfAngle < 0.0001f)
                        axis = Vector3.UnitX;
                    else
                        axis = new Vector3(delta.X, delta.Y, delta.Z) / sinHalfAngle;

                    // Angular velocity
                    Vector3 angularVelocity = axis * (angle / dt);

                    body.LinearVelocity = linearVelocity;
                    body.AngularVelocity = angularVelocity;

                } else
                {
                    go.Transform.Position = body.Position;
                    go.Transform.Rotation = body.Rotation;
                }
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