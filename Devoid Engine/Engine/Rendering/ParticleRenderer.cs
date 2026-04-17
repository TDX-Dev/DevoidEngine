using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ParticleSystem;
using DevoidGPU;
using System.Diagnostics;

namespace DevoidEngine.Engine.Rendering
{
    public class ParticleRenderer
    {
        public static readonly VertexInfo ParticleLayout = new VertexInfo(
            typeof(void),
            new VertexAttribute(
                "POSITION",
                0,
                2,
                0
            ),

            new VertexAttribute(
                "TEXCOORD",
                0,
                2,
                2 * sizeof(float)
            ),

// ----- slot 1 : instance buffer -----
            new VertexAttribute("INSTANCE_POS", 0, 3, 0, 1, VertexAttribType.Float, VertexStepMode.Instance),
            new VertexAttribute("INSTANCE_SIZE", 0, 1, 12, 1, VertexAttribType.Float, VertexStepMode.Instance),
            new VertexAttribute("INSTANCE_COLOR", 0, 4, 16, 1, VertexAttribType.Float, VertexStepMode.Instance)
        );

        VertexBuffer quadBuffer;
        VertexBuffer instanceBuffer;
        IInputLayout quadBufferInputLayout;

        Shader particleShader;

        ParticleInstance[] instances = new ParticleInstance[10000];

        public ParticleRenderer()
        {
            particleShader = ShaderLibrary.GetShader("PARTICLE_SHADER")!;

            var quad = ParticlePrimitives.GetParticleQuad();

            quadBuffer = new VertexBuffer(
                BufferUsage.Default,
                ParticleVertex.VertexInfo,
                quad.Length
            );

            instanceBuffer = new VertexBuffer(
                BufferUsage.Dynamic,
                ParticleInstance.VertexInfo,
                instances.Length
            );


            quadBuffer.SetData(quad);
            
            quadBufferInputLayout = Renderer.GetInputLayout(ParticleLayout, particleShader);
        }

        public void Render()
        {
            var emitters = EngineSingleton.Instance.ParticleSystem.Emitters;

            Renderer.GraphicsDevice.SetBlendState(BlendMode.AlphaBlend);
            Renderer.ResourceManager.VertexBufferManager.BindVertexBuffers(quadBuffer.GetHandle(), instanceBuffer.GetHandle());

            quadBufferInputLayout.Bind();
            particleShader.Use();
            for (int e = 0; e < emitters.Count; e++)
            {
                var emitter = emitters[e];
                var pool = emitter.Pool;

                int count = pool.AliveCount;
                if (count == 0)
                    continue;

                for (int i = 0; i < count; i++)
                {
                    ref var p = ref pool.Particles[i];

                    instances[i] = new ParticleInstance(
                        p.Position,
                        p.Size,
                        p.Color
                    );
                }

                if (emitter.texture != null)
                    emitter.texture.Bind(0);
                instanceBuffer.SetData(instances);

                Renderer.GraphicsDevice.DrawInstanced(
                    6,
                    count   // particle count
                );

            }

            Renderer.GraphicsDevice.SetBlendState(BlendMode.Opaque);
        }
    }
}
