using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal class DX11Framebuffer : IFrameBuffer
    {
        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<ITexture> ColorAttachments => colorAttachments;
        public ITexture? DepthAttachment => depthAttachment;

        private readonly DX11Texture[] colorAttachments;
        private readonly DX11Texture? depthAttachment;

        internal readonly RenderTargetView[] RTVs;
        internal readonly DepthStencilView? DSV;

        public DX11Framebuffer(
            DX11Texture[] colorAttachments,
            DX11Texture? depthAttachment = null
        )
        {
            this.colorAttachments = colorAttachments;
            this.depthAttachment = depthAttachment;

            Width = colorAttachments[0].Width;
            Height = colorAttachments[0].Height;

            ValidateFrameBuffer();

            RTVs = new RenderTargetView[colorAttachments.Length];
            for (int i = 0; i < colorAttachments.Length; i++)
            {
                RTVs[i] = colorAttachments[i].RTV!;
            }

            DSV = depthAttachment?.DSV;
        }

        private void ValidateFrameBuffer()
        {
            for (int i = 1; i < colorAttachments.Length; i++)
            {
                if (colorAttachments[i].Width != Width ||
                    colorAttachments[i].Height != Height)
                {
                    throw new InvalidOperationException("All framebuffer attachments must have the same dimensions.");
                }
            }
            if (depthAttachment != null && (depthAttachment.Width != Width || depthAttachment.Height != Height))
            {
                throw new InvalidOperationException("Depth attachment size must match color attachments.");
            }
        }
    }
}
