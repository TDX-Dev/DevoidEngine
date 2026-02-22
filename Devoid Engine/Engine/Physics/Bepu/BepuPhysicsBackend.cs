using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities.Memory;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using SharpDX.DXGI;
using System.Numerics;

namespace DevoidEngine.Engine.Physics.Bepu
{
    internal class BepuPhysicsBackend : IPhysicsBackend
    {
        private Simulation simulation;
        private BufferPool bufferPool;

        private Dictionary<BodyHandle, GameObject> bodyToGameObject = new Dictionary<BodyHandle, GameObject>();
        private Dictionary<StaticHandle, GameObject> staticToGameObject = new Dictionary<StaticHandle, GameObject>();

        private Dictionary<BodyHandle, PhysicsMaterial> bodyMaterials = new Dictionary<BodyHandle, PhysicsMaterial>();
        private Dictionary<StaticHandle, PhysicsMaterial> staticMaterials = new Dictionary<StaticHandle, PhysicsMaterial>();

        private Dictionary<BodyHandle, IPhysicsBody> bodyWrappers = new();


        public event Action<IPhysicsBody, IPhysicsBody> CollisionDetected;

        public void Initialize()
        {
            bufferPool = new BufferPool();

            var callbacks = new BepuNarrowPhaseCallbacks
            {
                MaterialLookup = LookupMaterial,
                Backend = this
            };

            simulation = Simulation.Create(
                bufferPool,
                callbacks,
                new BepuPoseIntegratorCallbacks { Gravity = new Vector3(0, -9.81f, 0) },
                new SolveDescription(8, 10),
                new DefaultTimestepper()
            );
        }

        internal void ReportCollision(CollidableReference a, CollidableReference b)
        {
            if (a.Mobility == CollidableMobility.Dynamic &&
                b.Mobility == CollidableMobility.Dynamic)
            {

                if (bodyToGameObject.TryGetValue(a.BodyHandle, out var goA) &&
                    bodyToGameObject.TryGetValue(b.BodyHandle, out var goB))
                {

                    var bodyA = simulation.Bodies.GetBodyReference(a.BodyHandle);
                    var bodyB = simulation.Bodies.GetBodyReference(b.BodyHandle);

                    if (bodyWrappers.TryGetValue(a.BodyHandle, out var wrapperA) &&
                        bodyWrappers.TryGetValue(b.BodyHandle, out var wrapperB))
                    {
                        CollisionDetected?.Invoke(wrapperA, wrapperB);
                    }
                }
            }
        }



        public void Step(float deltaTime)
        {
            simulation.Timestep(deltaTime);

            foreach (var pair in bodyMaterials)
            {
                var handle = pair.Key;
                var material = pair.Value;

                var body = simulation.Bodies.GetBodyReference(handle);

                float linearFactor = MathF.Max(0f, 1f - material.LinearDamping * deltaTime);
                float angularFactor = MathF.Max(0f, 1f - material.AngularDamping * deltaTime);

                body.Velocity.Linear *= linearFactor;
                body.Velocity.Angular *= angularFactor;
            }
        }



        private PhysicsMaterial LookupMaterial(CollidableReference collidable)
        {
            if (collidable.Mobility == CollidableMobility.Dynamic)
            {
                if (bodyMaterials.TryGetValue(collidable.BodyHandle, out var mat))
                    return mat;
            }
            else if (collidable.Mobility == CollidableMobility.Static)
            {
                if (staticMaterials.TryGetValue(collidable.StaticHandle, out var mat))
                    return mat;
            }

            return PhysicsMaterial.Default;
        }



        public IPhysicsBody CreateBody(PhysicsBodyDescription desc, GameObject owner)
        {
            TypedIndex shapeIndex = CreateShape(
                desc.Shape,
                desc.Mass,
                out BodyInertia inertia);

            var pose = new RigidPose(desc.Position, desc.Rotation);

            BodyDescription bodyDescription;

            if (desc.IsKinematic)
            {
                bodyDescription = BodyDescription.CreateKinematic(
                    pose,
                    new CollidableDescription(shapeIndex, 0.01f),
                    new BodyActivityDescription(0.01f)
                );
            }
            else
            {
                bodyDescription = BodyDescription.CreateDynamic(
                    pose,
                    inertia,
                    new CollidableDescription(shapeIndex, 0.01f),
                    new BodyActivityDescription(0.01f)
                );
            }


            BodyHandle handle = simulation.Bodies.Add(bodyDescription);

            bodyToGameObject[handle] = owner;
            bodyMaterials[handle] = desc.Material;

            var wrapper = new BepuPhysicsBody(handle, simulation, desc.Material, this);

            bodyWrappers[handle] = wrapper;

            return wrapper;
        }




        public void CreateStatic(PhysicsStaticDescription desc, GameObject owner)
        {
            TypedIndex shapeIndex = CreateShapeStatic(desc.Shape);

            var pose = new RigidPose(desc.Position, desc.Rotation);

            var staticDescription = new StaticDescription(pose, shapeIndex);

            StaticHandle handle = simulation.Statics.Add(staticDescription);

            staticToGameObject[handle] = owner;
            staticMaterials[handle] = desc.Material;
        }





        private TypedIndex CreateShape(
            PhysicsShapeDescription shapeDesc,
            float mass,
            out BodyInertia inertia)
        {
            switch (shapeDesc.Type)
            {
                case PhysicsShapeType.Box:
                    {
                        var shape = new Box(
                            shapeDesc.Size.X,
                            shapeDesc.Size.Y,
                            shapeDesc.Size.Z);

                        inertia = shape.ComputeInertia(mass);
                        return simulation.Shapes.Add(shape);
                    }

                case PhysicsShapeType.Sphere:
                    {
                        var shape = new Sphere(shapeDesc.Radius);

                        inertia = shape.ComputeInertia(mass);
                        return simulation.Shapes.Add(shape);
                    }

                case PhysicsShapeType.Capsule:
                    {
                        var shape = new Capsule(
                            shapeDesc.Radius,
                            shapeDesc.Height);

                        inertia = shape.ComputeInertia(mass);
                        return simulation.Shapes.Add(shape);
                    }

                case PhysicsShapeType.ConvexHull:
                    {
                        var shape = new ConvexHull(
                            shapeDesc.Vertices,
                            bufferPool,
                            out _);

                        inertia = shape.ComputeInertia(mass);
                        return simulation.Shapes.Add(shape);
                    }

                //case PhysicsShapeType.Mesh:
                //    {
                //        var shape = new Mesh(
                //            shapeDesc.Vertices,
                //            shapeDesc.Indices,
                //            bufferPool);

                //        // Mesh should NOT be dynamic
                //        inertia = default;
                //        return simulation.Shapes.Add(shape);
                //    }

                default:
                    throw new NotSupportedException("Shape not supported.");
            }
        }

        private TypedIndex CreateShapeStatic(PhysicsShapeDescription shapeDesc)
        {
            switch (shapeDesc.Type)
            {
                case PhysicsShapeType.Box:
                    {
                        var shape = new Box(
                            shapeDesc.Size.X,
                            shapeDesc.Size.Y,
                            shapeDesc.Size.Z);

                        return simulation.Shapes.Add(shape);
                    }

                case PhysicsShapeType.Sphere:
                    {
                        var shape = new Sphere(shapeDesc.Radius);
                        return simulation.Shapes.Add(shape);
                    }

                case PhysicsShapeType.Capsule:
                    {
                        var shape = new Capsule(
                            shapeDesc.Radius,
                            shapeDesc.Height);

                        return simulation.Shapes.Add(shape);
                    }

                case PhysicsShapeType.ConvexHull:
                    {
                        var shape = new ConvexHull(
                            shapeDesc.Vertices,
                            bufferPool,
                            out _);

                        return simulation.Shapes.Add(shape);
                    }

                //case PhysicsShapeType.Mesh:
                //    {
                //        var shape = new Mesh(
                //            shapeDesc.Vertices,
                //            shapeDesc.Indices,
                //            bufferPool);

                //        return simulation.Shapes.Add(shape);
                //    }

                default:
                    throw new NotSupportedException("Unsupported static shape type");
            }
        }



        public void RemoveBody(IPhysicsBody body)
        {
            if (body is BepuPhysicsBody b)
            {
                simulation.Bodies.Remove(b.Handle);

                bodyToGameObject.Remove(b.Handle);
                bodyMaterials.Remove(b.Handle);
            } else
            {
                Console.WriteLine("Removing Static bodies not implemented yet.");
            }
        }


        public bool Raycast(Ray ray, float maxDistance, out RaycastHit hit)
        {
            hit = default;

            var handler = new RayHitHandler();

            simulation.RayCast(
                ray.Origin,
                ray.Direction,
                maxDistance,
                ref handler);

            if (!handler.Hit)
                return false;

            hit.Distance = handler.Distance;
            hit.Point = ray.GetPoint(handler.Distance);
            hit.Normal = handler.Normal;

            if (handler.Collidable.Mobility == CollidableMobility.Dynamic)
            {
                var bodyHandle = handler.Collidable.BodyHandle;

                if (bodyToGameObject.TryGetValue(bodyHandle, out var go))
                    hit.HitObject = go;
            }
            else if (handler.Collidable.Mobility == CollidableMobility.Static)
            {
                var staticHandle = handler.Collidable.StaticHandle;

                if (staticToGameObject.TryGetValue(staticHandle, out var go))
                    hit.HitObject = go;
            }


            return true;
        }

    }

}