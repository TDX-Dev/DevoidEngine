using SharpDX.Direct3D11;
using System.Numerics;

namespace DevoidGPU
{
    public interface ICommandList
    {
        public CommandListType Type { get; }
        void Begin();
        void End();
        void Reset();

        void BeginTimestampDisjoint(IGPUTimestampDisjoint query);
        void EndTimestampDisjoint(IGPUTimestampDisjoint query);
        void WriteTimestamp(IGPUTimestamp query);

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
        void DrawInstancedIndexed(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation);
        void Dispatch(uint groupX, uint groupY, uint groupZ);
        void MemoryBarrier(MemoryBarrierFlags flags);
        void ResolveSubresource(ITexture multisampled, ITexture single);
        void ClearTextureResource(ITexture texture, ClearValue value);
        void CopyTextureSubresourceRegion(ITexture source, ITexture destination, int sourceX, int sourceY, int width, int height);
        MappedTexture MapTexture(ITexture texture, MapMode mode);
        void UnmapTexture(ITexture texture);
    }
}
