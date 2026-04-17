namespace DevoidEngine.Engine.ParticleSystem
{
    class ParticlePool
    {
        public Particle[] Particles;
        public int AliveCount;

        public ParticlePool(int maxParticles)
        {
            Particles = new Particle[maxParticles];
            AliveCount = 0;
        }

        public bool HasSpace => AliveCount < Particles.Length;

        public ref Particle Spawn()
        {
            return ref Particles[AliveCount++];
        }

        public void Kill(int index)
        {
            AliveCount--;

            if (index != AliveCount)
                Particles[index] = Particles[AliveCount];
        }
    }
}