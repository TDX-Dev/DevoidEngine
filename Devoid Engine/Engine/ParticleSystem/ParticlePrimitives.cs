using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ParticleSystem
{
    public static class ParticlePrimitives
    {
        public static ParticleVertex[] GetParticleQuad()
        {
            return new ParticleVertex[]
            {
                new(new Vector2(-0.5f, -0.5f), new Vector2(0f, 1f)),
                new(new Vector2( 0.5f, -0.5f), new Vector2(1f, 1f)),
                new(new Vector2( 0.5f,  0.5f), new Vector2(1f, 0f)),

                new(new Vector2(-0.5f, -0.5f), new Vector2(0f, 1f)),
                new(new Vector2( 0.5f,  0.5f), new Vector2(1f, 0f)),
                new(new Vector2(-0.5f,  0.5f), new Vector2(0f, 0f))
            };
        }


    }
}
