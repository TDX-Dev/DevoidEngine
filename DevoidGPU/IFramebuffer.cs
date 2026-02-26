using System.Numerics;

namespace DevoidGPU
{
    public interface IFramebuffer : IDisposable
    {
        List<ITexture2D> ColorAttachments { get; }
        ITexture2D DepthAttachment { get; }

        //void Bind();
        int Width { get; }
        int Height { get; }

        void AddColorAttachment(ITexture2D texture, int index = 0);
        void AddDepthAttachment(ITexture2D texture);

        void ClearColor(Vector4 color);
        void ClearDepth(float depth = 1.0f);

    }
}
