using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering
{
    public class RenderAPI
    {
        public Shader FullscreenShader;
        public IPipeline FullscreenPipeline;
        public Mesh FullscreenMesh;
        public ISampler FullscreenSampler;
        public IDescriptorSet FullscreenDescriptor;

        public RenderAPI()
        {
            FullscreenShader = new Shader("Fullscreen Shader", "Content/Shaders/fullscreen.vert.hlsl", "Content/Shaders/blit.frag.hlsl");

            FullscreenMesh = PrimitiveMeshes.GetFullscreenPlane();
            FullscreenDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(FullscreenShader.GetActivePass().DescriptorLayout);

            FullscreenPipeline = Engine.GraphicsDevice.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
                VertexShader = FullscreenShader.GetActivePass().VS,
                PixelShader = FullscreenShader.GetActivePass().VS,
                Blend = new BlendStateDescription()
                {
                    BlendStates =
                        [
                            BlendState.FromBlendMode(BlendMode.AlphaBlend)
                        ]
                },
                Rasterizer = new RasterizerState()
                {
                    CullMode = CullMode.Back,
                    EnableScissor = false,
                    FillMode = FillMode.Solid,
                },
                DepthStencil = new DepthStencilState()
                {
                    DepthTest = false,
                    DepthWrite = false
                },
                Topology = PrimitiveType.Triangles,
                VertexLayout = FullscreenMesh.Surfaces[0].VertexInfo
            });

            FullscreenSampler = Engine.GraphicsDevice.CreateSampler(new SamplerDescription()
            {
                AddressU = WrapMode.ClampToEdge,
                AddressV = WrapMode.ClampToEdge,
                AddressW = WrapMode.ClampToEdge,
            });

            FullscreenDescriptor.SetSampler(0, FullscreenSampler);
        }

        public void RenderToScreen(ICommandList cmd, Texture texture)
        {
            if (texture == null) return;

            cmd.SetPipeline(FullscreenPipeline);
            FullscreenDescriptor.SetTexture(0, texture.GPU);

            cmd.SetDescriptorSet(0, FullscreenDescriptor);
            FullscreenMesh.Draw(cmd);
        }

        public void RenderToScreen(ICommandList cmd, MaterialInstance material)
        {
            if (material == null) return;

            cmd.SetPipeline(FullscreenPipeline);

            cmd.SetDescriptorSet(0, material.DescriptorSet);
            FullscreenMesh.Draw(cmd);
        }
    }
}
