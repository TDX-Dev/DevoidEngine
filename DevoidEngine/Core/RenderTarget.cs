using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class RenderTarget
    {
        public IReadOnlyList<Texture> ColorTextures => colorTextures;
        public Texture? DepthTexture { get; }

        public IFrameBuffer GPU { get; }

        public int Width => ColorTextures[0].Width;
        public int Height => ColorTextures[0].Height;

        private readonly Texture[] colorTextures;
        internal RenderTarget(
            IFrameBuffer gpu,
            Texture[] colorTextures,
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
    }
}
