using DevoidGPU;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Core
{
    public sealed class RenderTarget
    {
        public IReadOnlyList<Texture?> ColorTextures => _colorTextures;
        public Texture? DepthTexture { get; private set; }

        public IFrameBuffer Gpu { get; }

        public int Width
        {
            get
            {
                Texture tex = _colorTextures.FirstOrDefault(t => t != null)
                    ?? throw new InvalidOperationException("RenderTarget has no attachments.");

                return tex.Width;
            }
        }
        public int Height
        {
            get
            {
                Texture tex = _colorTextures.FirstOrDefault(t => t != null)
                    ?? throw new InvalidOperationException("RenderTarget has no attachments.");

                return tex.Height;
            }
        }

        private readonly Texture?[] _colorTextures;

        internal RenderTarget(
            IFrameBuffer gpu,
            Texture?[] colorTextures,
            Texture? depthTexture = null
        )
        {
            Gpu = gpu;

            if (colorTextures.Length == 0)
                throw new ArgumentException("RenderTarget requires at least one color texture.");

            this._colorTextures = colorTextures;
            DepthTexture = depthTexture;
        }

        public static RenderTarget Create(Texture[] colorTextures, Texture? depthTexture)
        {
            IGraphicsDevice device = Engine.GraphicsDevice;
            IFrameBuffer frameBuffer = device.CreateFrameBuffer([.. colorTextures.Select(e => e.GPU)], depthTexture?.GPU);

            return new RenderTarget(frameBuffer, colorTextures, depthTexture);
        }

        public static RenderTarget Create(int colorAttachmentCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(colorAttachmentCount);

            IGraphicsDevice device = Engine.GraphicsDevice;

            IFrameBuffer frameBuffer = device.CreateFrameBuffer(
                new ITexture?[colorAttachmentCount],
                null
            );

            return new RenderTarget(
                frameBuffer,
                new Texture?[colorAttachmentCount],
                null
            );
        }

        public void SetColorAttachment(int slot, Texture texture, int mip = 0, int slice = 0)
        {
            _colorTextures[slot] = texture;
            Gpu.SetColorAttachment(slot, texture.GPU, mip, slice);
        }

        public void SetDepthAttachment(Texture texture)
        {
            DepthTexture = texture;
            Gpu.SetDepthAttachment(texture.GPU);
        }
    }
}
