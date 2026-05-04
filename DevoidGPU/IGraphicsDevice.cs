namespace DevoidGPU
{
    public interface IGraphicsDevice
    {
        ISwapchain CreateSwapchain(SwapchainDescription desc);
        IShader CreateShader(ShaderDescription desc);
        IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription desc);
        IVertexBuffer CreateVertexBuffer(VertexBufferDescription desc);
        IIndexBuffer CreateIndexBuffer(IndexBufferDescription desc);
    }
}
