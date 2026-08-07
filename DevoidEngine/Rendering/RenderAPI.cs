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
            FullscreenShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/blit_mat.dsd"));
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

        public void RenderToScreen(ICommandList cmd, MaterialInstance material)
        {
            if (material == null) return;

            cmd.SetPipeline(material.BaseMaterial.DefaultPass.Pipeline);

            cmd.SetDescriptorSet(0, material.DescriptorSet);
            FullscreenMesh.Draw(cmd);
        }
    }
}
