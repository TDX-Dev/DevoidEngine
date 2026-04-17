using DevoidEngine.Engine.Components;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DevoidEngine.Engine.ParticleSystem
{
    public class ParticleSystemManager
    {
        public IReadOnlyList<ParticleEmitterComponent> Emitters => emitters;
        private readonly List<ParticleEmitterComponent> emitters = new();
        private readonly Random random = new();

        public void Register(ParticleEmitterComponent emitter)
        {
            emitters.Add(emitter);
        }

        public void Unregister(ParticleEmitterComponent emitter)
        {
            emitters.Remove(emitter);
        }

        public void Update(float dt)
        {
            foreach (var emitter in emitters)
            {
                UpdateEmitter(emitter, dt);
                UpdateParticles(emitter, dt);
            }
        }

        private void UpdateEmitter(ParticleEmitterComponent emitter, float dt)
        {
            emitter.SpawnAccumulator += emitter.SpawnRate * dt;

            int spawnCount = (int)emitter.SpawnAccumulator;
            emitter.SpawnAccumulator -= spawnCount;

            Spawn(emitter, spawnCount);
        }

        private void Spawn(ParticleEmitterComponent emitter, int count)
        {
            var pool = emitter.Pool;

            for (int i = 0; i < count; i++)
            {
                if (!pool.HasSpace)
                    return;

                ref var p = ref pool.Spawn();

                p.Position = emitter.gameObject.Transform.Position;

                float speed = RandomRange(emitter.SpeedMin, emitter.SpeedMax);
                p.Velocity = RandomDirection() * speed;

                p.MaxLife = RandomRange(emitter.LifetimeMin, emitter.LifetimeMax);
                p.Life = p.MaxLife;

                p.Size = RandomRange(emitter.SizeMin, emitter.SizeMax);
                p.Color = emitter.StartColor;
            }
        }

        private void UpdateParticles(ParticleEmitterComponent emitter, float dt)
        {
            var pool = emitter.Pool;

            for (int i = 0; i < pool.AliveCount; i++)
            {
                ref var p = ref pool.Particles[i];

                p.Life -= dt;

                if (p.Life <= 0f)
                {
                    pool.Kill(i);
                    i--;
                    continue;
                }

                p.Velocity += emitter.Gravity * dt;
                p.Position += p.Velocity * dt;

                float t = 1f - (p.Life / p.MaxLife);

                p.Color = Vector4.Lerp(
                    emitter.StartColor,
                    emitter.EndColor,
                    t);
            }
        }

        private float RandomRange(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private Vector3 RandomDirection()
        {
            float x = (float)random.NextDouble() * 2f - 1f;
            float y = (float)random.NextDouble() * 2f - 1f;
            float z = (float)random.NextDouble() * 2f - 1f;

            Vector3 v = new(x, y, z);

            if (v == Vector3.Zero)
                v = Vector3.UnitY;

            return Vector3.Normalize(v);
        }
    }
}