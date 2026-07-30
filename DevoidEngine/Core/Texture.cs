using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class Texture : IDisposable
    {
        public ITexture GPU { get; }

        public TextureDimension Dimension => GPU.Description.Dimension;
        public int Width => GPU.Description.Width;
        public int Height => GPU.Description.Height;
        public int Depth => GPU.Description.Depth;

        public static Texture Default { get; }

        static Texture()
        {
            //Default = CreateFromImage2D(
            //    TextureUtil.LoadImage("Assets/dvs.png"),
            //    TextureUsage.ShaderResource
            //);

            Default = Create2D(
                1,
                1,
                TextureFormat.RGBA16_Float,
                TextureUsage.ShaderResource);

            Default.GPU.Update<Half>(
            [
                (Half)1.0f, (Half) 1.0f, (Half) 1.0f, (Half) 1.0f
            ]);
        }

        internal Texture(ITexture gpu)
        {
            GPU = gpu;
        }

        public Texture(TextureDescription description)
        {
            IGraphicsDevice device = Engine.GraphicsDevice;

            ITexture gpu = device.CreateTexture(description);

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

        public static Texture CreateFromImage2D(ImageData data, TextureUsage usage)
        {
            IGraphicsDevice device = Engine.GraphicsDevice;

            ITexture gpu = device.CreateTexture(new TextureDescription()
            {
                Width = data.Width,
                Height = data.Height,
                Format = data.Format,
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

            gpu.Update<byte>(data.Data);

            return new Texture(gpu);
        }

        public void Dispose()
        {
            GPU.Dispose();
        }
    }
}
