using SharpDX.Direct3D11;

namespace DevoidGPU
{
    public interface IGraphicsDevice
    {
        ISwapchain CreateSwapchain(SwapchainDescription desc);
        IShader CreateShader(ShaderDescription desc);
        IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription desc);
        IVertexBuffer CreateVertexBuffer(VertexBufferDescription desc);
        IIndexBuffer CreateIndexBuffer(IndexBufferDescription desc);
        ITexture CreateTexture(TextureDescription desc);
        IFrameBuffer CreateFrameBuffer(ITexture[] colorAttachments, ITexture? depthAttachment);
        IUniformBuffer CreateUniformBuffer(UniformBufferDescription desc);
        ICommandList GetCommandList();
        ICommandQueue GetCommandQueue(CommandListType type);
        void Submit(ICommandList cmd);
    }
}
