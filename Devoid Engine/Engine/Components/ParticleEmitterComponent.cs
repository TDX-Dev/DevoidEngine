using DevoidEngine.Engine.Attributes;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ParticleSystem;
using DevoidEngine.Engine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public class ParticleEmitterComponent : Component
    {
        public override string Type => nameof(ParticleEmitterComponent);

        public Texture2D? Texture
        {
            get => texture;
            set => texture = value;
        }

        internal Texture2D? texture;

        public int MaxParticles = 200;
        public float SpawnRate = 50f;

        public float LifetimeMin = 0.2f;
        public float LifetimeMax = 0.5f;

        public float SpeedMin = 5f;
        public float SpeedMax = 20f;

        public Vector3 Gravity = new(0, -25f, 0);

        public float SizeMin = 0.02f;
        public float SizeMax = 0.08f;

        [ColorField]
        public Vector4 StartColor = new(1f, 0.8f, 0.3f, 1f);
        [ColorField]
        public Vector4 EndColor = new(1f, 0.2f, 0.1f, 0f);

        public bool Burst = false;
        public int BurstCount = 20;

        [DontSerialize]
        internal ParticlePool Pool = null!;
        [DontSerialize]
        internal float SpawnAccumulator;

        public override void OnStart()
        {
            Console.WriteLine("Particle Emitter Registered");
            Pool = new ParticlePool(MaxParticles);
            gameObject.Scene.ParticleSystem.Register(this);
        }

        public override void OnDestroy()
        {
            gameObject.Scene.ParticleSystem.Unregister(this);
        }
    }
}
