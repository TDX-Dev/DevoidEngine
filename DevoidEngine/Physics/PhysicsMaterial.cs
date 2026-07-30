using MessagePack;

namespace DevoidEngine.Physics
{
    [MessagePackObject]
    public struct PhysicsMaterial
    {
        [Key(0)]
        public float Friction;
        [Key(1)]
        public float Restitution; // bounciness
        [Key(2)]
        public float LinearDamping;
        [Key(3)]
        public float AngularDamping;

        public static PhysicsMaterial Default => new()
        {
            Friction = 0.5f,
            Restitution = 0.01f,
            LinearDamping = 0.01f,
            AngularDamping = 0.01f
        };
    }
}
