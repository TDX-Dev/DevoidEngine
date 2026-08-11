using DevoidGPU;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Core
{
    public sealed class RenderTarget
    {
        public IReadOnlyList<Texture?> ColorTextures => colorTextures;
        public Texture? DepthTexture { get; private set; }

        public IFrameBuffer GPU { get; }

        public int Width
        {
            get
            {
                Texture tex = colorTextures.FirstOrDefault(t => t != null)
                    ?? throw new InvalidOperationException("RenderTarget has no attachments.");

                return tex.Width;
            }
        }
        public int Height
        {
            get
            {
                Texture tex = colorTextures.FirstOrDefault(t => t != null)
                    ?? throw new InvalidOperationException("RenderTarget has no attachments.");

                return tex.Height;
            }
        }

        private readonly Texture?[] colorTextures;

        internal RenderTarget(
            IFrameBuffer gpu,
            Texture?[] colorTextures,
            Texture? depthTexture = null
        )
        {
            GPU = gpu;

            if (colorTextures.Length == 0)
                throw new ArgumentException("RenderTarget requires at least one color texture.");

            this.colorTextures = colorTextures;
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
            colorTextures[slot] = texture;
            GPU.SetColorAttachment(slot, texture.GPU, mip, slice);
        }

        public void SetDepthAttachment(Texture texture)
        {
            DepthTexture = texture;
            GPU.SetDepthAttachment(texture.GPU);
        }
    }
}
