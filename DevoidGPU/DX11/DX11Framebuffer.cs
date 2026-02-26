using SharpDX.Direct3D11;
using System.Numerics;

namespace DevoidGPU.DX11
{
    public class DX11Framebuffer : IFramebuffer
    {
        public List<ITexture2D> ColorAttachments { get; set; }

        public ITexture2D DepthAttachment { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11Framebuffer(Device device, DeviceContext context)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            this.deviceContext = context ?? throw new ArgumentNullException(nameof(context));

            ColorAttachments = new List<ITexture2D>();
        }

        public void AddColorAttachment(ITexture2D texture, int index = 0)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (!texture.IsRenderTarget)
                throw new InvalidOperationException("Texture is not render-target capable.");
            while (ColorAttachments.Count <= index)
                ColorAttachments.Add(null);
            ColorAttachments[index] = texture;
            if (Width != 0 || Height != 0)
            {
                Width = texture.Width;
                Height = texture.Height;
            }
        }

        public void AddDepthAttachment(ITexture2D texture)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (!texture.IsDepthStencil)
                throw new InvalidOperationException("Texture is not a depth-stencil.");

            DepthAttachment = texture;

            if (Width == 0 || Height == 0)
            {
                Width = texture.Width;
                Height = texture.Height;
            }
        }

        public void Bind()
        {


            //var rtvs = new RenderTargetView[ColorAttachments.Count];
            //for (int i = 0; i < ColorAttachments.Count; i++)
            //{
            //    if (ColorAttachments[i] != null && ColorAttachments[i] is DX11Texture2D tex2D)
            //        rtvs[i] = tex2D.RenderTargetView;
            //}

            //DepthStencilView dsv = null;
            //if (DepthAttachment is DX11Texture2D depthTex)
            //    dsv = depthTex.DepthStencilView;

            //deviceContext.OutputMerger.SetRenderTargets(dsv, rtvs);
        }

        public void ClearColor(Vector4 color)
        {
            foreach (DX11Texture2D tex in ColorAttachments)
            {
                if (tex.IsRenderTarget)
                {
                    deviceContext.ClearRenderTargetView(tex.RenderTargetView, new SharpDX.Mathematics.Interop.RawColor4(color.X, color.Y, color.Z, color.W));
                }
            }
        }

        public void ClearDepth(float depth = 1)
        {
            if (DepthAttachment != null)
                deviceContext.ClearDepthStencilView((DepthAttachment as DX11Texture2D).DepthStencilView, DepthStencilClearFlags.Depth, depth, 0);
        }

        public void Dispose()
        {

        }
    }
}
