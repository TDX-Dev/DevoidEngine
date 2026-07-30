using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering
{
    public class RenderAPI
    {
        public Shader FullscreenShader;
        public Mesh FullscreenMesh;
        public ISampler FullscreenSampler;
        public IDescriptorSet FullscreenDescriptor;

        public RenderAPI()
        {
            FullscreenShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/blit_mat.dsd");
            FullscreenMesh = PrimitiveMeshes.GetFullscreenPlane();
            FullscreenDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(FullscreenShader.DefaultPass.DescriptorLayout);

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

            cmd.SetPipeline(FullscreenShader.DefaultPass.Pipeline);
            FullscreenDescriptor.SetTexture(0, texture.GPU);

            cmd.SetDescriptorSet(0, FullscreenDescriptor);
            FullscreenMesh.Draw(cmd);
        }
    }
}
