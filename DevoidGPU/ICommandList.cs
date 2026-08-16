using System.Numerics;

namespace DevoidGPU
{
    public interface ICommandList
    {
        public CommandListType Type { get; }
        void Begin();
        void End();
        void Reset();

        void SetViewport(int x, int y, int width, int height);
        void SetScissor(int x, int y, int width, int height);

        void SetFramebuffer(IFrameBuffer? framebuffer);

        void ClearColor(int attachmentIndex, Vector4 color);
        void ClearDepthStencil(float depth, byte stencil);

        void GenerateMipmaps(ITexture texture);

        void SetPipeline(IPipeline pipeline);
        void SetComputePipeline(IComputePipeline pipeline);
        void SetVertexBuffer(IVertexBuffer vertexBuffer);
        void SetIndexBuffer(IIndexBuffer indexBuffer);
        void SetDescriptorSet(uint binding, IDescriptorSet set);
        void Draw(int vertexCount, int startVertexLocation);
        void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation);
        void Dispatch(uint groupX, uint groupY, uint groupZ);
        void MemoryBarrier(MemoryBarrierFlags flags);
        void ResolveSubresource(ITexture multisampled, ITexture single);
    }
}
