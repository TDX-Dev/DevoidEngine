using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class DX11Framebuffer : IFrameBuffer
    {
        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<ITexture?> ColorAttachments => colorAttachments;
        public ITexture? DepthAttachment => depthAttachment;

        private readonly DX11Texture?[] colorAttachments;
        private DX11Texture? depthAttachment;

        internal readonly RenderTargetView?[] RTVs;
        internal DepthStencilView? DSV;

        public DX11Framebuffer(
            DX11Texture?[] colorAttachments,
            DX11Texture? depthAttachment = null
        )
        {
            this.colorAttachments = colorAttachments;
            this.depthAttachment = depthAttachment;

            RTVs = new RenderTargetView?[colorAttachments.Length];

            for (int i = 0; i < colorAttachments.Length; i++)
            {
                RTVs[i] = colorAttachments[i]?.GetRTV();
            }

            DSV = depthAttachment?.DSV;
        }

        internal void ValidateFrameBuffer()
        {
            DX11Texture? reference =
                colorAttachments.FirstOrDefault(x => x != null)
                ?? depthAttachment;

            if (reference == null)
                return;

            foreach (var attachment in colorAttachments)
            {
                if (attachment == null)
                    continue;

                if (attachment.Width != reference.Width ||
                    attachment.Height != reference.Height)
                {
                    throw new InvalidOperationException(
                        "All framebuffer attachments must have the same dimensions.");
                }
            }

            if (depthAttachment != null)
            {
                if (depthAttachment.Width != reference.Width ||
                    depthAttachment.Height != reference.Height)
                {
                    throw new InvalidOperationException(
                        "Depth attachment size must match color attachments.");
                }
            }
        }

        public void SetColorAttachment(int index, ITexture texture, int mip = 0, int slice = 0)
        {
            var dxTex = (DX11Texture)texture;

            colorAttachments[index] = dxTex;
            RTVs[index] = dxTex.GetRTV(mip, slice);

        }

        public void SetDepthAttachment(ITexture texture)
        {
            var dxTex = (DX11Texture)texture;

            depthAttachment = dxTex;
            DSV = dxTex.DSV;
        }

        public void Dispose()
        {

        }
    }
}
