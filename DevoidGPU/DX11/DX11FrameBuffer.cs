using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11Framebuffer : IFrameBuffer
    {
        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<ITexture?> ColorAttachments => _colorAttachments;
        public ITexture? DepthAttachment => _depthAttachment;

        private readonly DX11Texture?[] _colorAttachments;
        private DX11Texture? _depthAttachment;

        internal readonly RenderTargetView?[] RtVs;
        internal DepthStencilView? Dsv;
        internal bool Dirty = true;


        public Dx11Framebuffer(
            DX11Texture?[] colorAttachments,
            DX11Texture? depthAttachment = null
        )
        {
            this._colorAttachments = colorAttachments;
            this._depthAttachment = depthAttachment;

            RtVs = new RenderTargetView?[colorAttachments.Length];

            for (int i = 0; i < colorAttachments.Length; i++)
            {
                RtVs[i] = colorAttachments[i]?.GetRtv();
            }

            Dsv = depthAttachment?.Dsv;
        }

        internal void ValidateFrameBuffer()
        {
            DX11Texture? reference =
                _colorAttachments.FirstOrDefault(x => x != null)
                ?? _depthAttachment;

            if (reference == null)
                return;

            foreach (var attachment in _colorAttachments)
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

            if (_depthAttachment != null)
            {
                if (_depthAttachment.Width != reference.Width ||
                    _depthAttachment.Height != reference.Height)
                {
                    throw new InvalidOperationException(
                        "Depth attachment size must match color attachments.");
                }
            }
        }

        public void SetColorAttachment(int index, ITexture texture, int mip = 0, int slice = 0)
        {
            var dxTex = (DX11Texture)texture;

            _colorAttachments[index] = dxTex;
            RtVs[index] = dxTex.GetRtv(mip, slice);
            Dirty = true;

        }

        public void SetDepthAttachment(ITexture texture)
        {
            var dxTex = (DX11Texture)texture;

            _depthAttachment = dxTex;
            Dsv = dxTex.Dsv;
        }

        public void Dispose()
        {

        }
    }
}
