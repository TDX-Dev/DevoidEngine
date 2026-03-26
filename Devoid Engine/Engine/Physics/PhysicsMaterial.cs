namespace DevoidEngine.Engine.Physics
{
    public struct PhysicsMaterial
    {
        public float Friction;
        public float Restitution; // bounciness
        public float LinearDamping;
        public float AngularDamping;

        public static PhysicsMaterial Default => new PhysicsMaterial
        {
            Friction = 0.5f,
            Restitution = 0.01f,
            LinearDamping = 0.01f,
            AngularDamping = 0.01f
        };
    }

}