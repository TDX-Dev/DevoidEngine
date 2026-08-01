using DevoidGPU.DX11;

namespace DevoidGPU
{
    public interface IGraphicsDevice
    {
        ISwapchain CreateSwapchain(SwapchainDescription desc);
        IShader CreateShader(ShaderDescription desc);
        IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription desc);
        IComputePipeline CreateComputePipeline(ComputePipelineDescription desc);

        IVertexBuffer CreateVertexBuffer(VertexBufferDescription desc);
        IIndexBuffer CreateIndexBuffer(IndexBufferDescription desc);
        ITexture CreateTexture(TextureDescription desc);
        ISampler CreateSampler(SamplerDescription desc);
        IFrameBuffer CreateFrameBuffer(ITexture?[] colorAttachments, ITexture? depthAttachment);
        IUniformBuffer CreateUniformBuffer(BufferDescription desc);
        IShaderStorageBuffer CreateShaderStorageBuffer(BufferDescription desc);
        IDescriptorLayout CreateDescriptorLayout(DescriptorBinding[] bindings);
        IDescriptorSet CreateDescriptorSet(IDescriptorLayout layout);
        ICommandList GetCommandList();
        ICommandQueue GetCommandQueue(CommandListType type);
        void Submit(ICommandList cmd);
    }
}
