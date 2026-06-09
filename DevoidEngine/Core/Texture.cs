using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class Texture : IDisposable
    {
        public ITexture GPU { get; }

        public TextureDimension Dimension => GPU.Description.Dimension;
        public int Width => GPU.Description.Width;
        public int Height => GPU.Description.Height;
        public int Depth => GPU.Description.Depth;

        public static Texture Default => Texture.Create2D(1,1,TextureFormat.RGBA16_Float, TextureUsage.ShaderResource);

        internal Texture(ITexture gpu)
        {
            GPU = gpu;
        }

        public static Texture Create2D(int width, int height, TextureFormat format, TextureUsage usage)
        {
            IGraphicsDevice device = Engine.GraphicsDevice;

            ITexture gpu = device.CreateTexture(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = format,
                Usage = usage,
                ArraySize = 1,
                Depth = 1,
                Dimension = TextureDimension.Texture2D,
                MipLevels = 1,
                Samples = new TextureSampleDescription()
                {
                    Count = 1,
                    Quality = 0
                }
            });

            return new Texture(gpu);
        }

        public void Dispose()
        {
            GPU.Dispose();
        }
    }
}
