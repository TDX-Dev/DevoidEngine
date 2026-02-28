using BepuPhysics;
using DevoidEngine.Engine.Physics;
using System.Numerics;

namespace DevoidEngine.Engine.Physics.Bepu
{
    internal class BepuPhysicsStatic : IPhysicsStatic
    {
        internal StaticHandle Handle;
        private Simulation simulation;
        private BepuPhysicsBackend backend;

        private static int nextId = 1;
        public int Id { get; }

        public BepuPhysicsStatic(
            StaticHandle handle,
            Simulation simulation,
            BepuPhysicsBackend backend)
        {
            Id = Interlocked.Increment(ref nextId);
            Handle = handle;
            this.simulation = simulation;
            this.backend = backend;
        }

        private StaticReference GetStatic()
        {
            return simulation.Statics.GetStaticReference(Handle);
        }

        public Vector3 Position
        {
            get => GetStatic().Pose.Position;
            set
            {
                var s = GetStatic();
                s.Pose.Position = value;
                s.UpdateBounds();
            }
        }

        public Quaternion Rotation
        {
            get => GetStatic().Pose.Orientation;
            set
            {
                var s = GetStatic();
                s.Pose.Orientation = value;
                s.UpdateBounds();
            }
        }

        public void Remove()
        {
            simulation.Statics.Remove(Handle);
            backend.RemoveStatic(this);
        }
    }
}