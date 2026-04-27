using System.Numerics;

namespace DevoidGPU
{
    public interface ICommandList
    {
        void Begin();
        void End();

        void SetViewport(int x, int y, int width, int height);
        void SetScissor(int x, int y, int width,int height);

        void SetFramebuffer(IFrameBuffer framebuffer);

        void ClearColor(int attachmentIndex, Vector4 color);
        void ClearDepthStencil(float depth, byte stencil);

        void SetPipeline(IPipeline pipeline);
        void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation);
    }
}
