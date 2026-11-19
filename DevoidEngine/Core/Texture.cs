using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class Texture
    {
        public ITexture Gpu { get; }

        public TextureDimension Dimension => Gpu.Description.Dimension;
        public int Width => Gpu.Description.Width;
        public int Height => Gpu.Description.Height;
        public int Depth => Gpu.Description.Depth;
        public int MipLevels => Gpu.MipLevels;
        public int ArraySize => Gpu.ArraySize;
        public TextureUsage Usage => Gpu.Description.Usage;
        public TextureFormat Format => Gpu.Format;

        public ulong Id { get; } = 0;

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
                TextureFormat.Rgba16Float,
                TextureUsage.ShaderResource
            );

            Default.Gpu.Update<Half>(
            [
                (Half)1.0f, (Half) 1.0f, (Half) 1.0f, (Half) 1.0f
            ]);
        }

        internal Texture(ITexture gpu)
        {
            Gpu = gpu;
            Id = Engine.Instance.TextureManager.Register(this);
        }

        public Texture(TextureDescription description)
        {
            IGraphicsDevice device = Engine.GraphicsDevice;

            ITexture gpu = device.CreateTexture(description);

            Gpu = gpu;
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

        public static Texture CreateCube(
            int size,
            TextureFormat format,
            TextureUsage usage,
            int mipLevels = 1
        )
        {
            return new Texture(new TextureDescription()
            {
                Width = size,
                Height = size,
                Depth = 1,
                Format = format,
                Usage = usage,
                Dimension = TextureDimension.TextureCube,
                ArraySize = 6,
                MipLevels = mipLevels,
                Samples = new TextureSampleDescription()
                {
                    Count = 1,
                    Quality = 0
                }
            });
        }

        public static Texture CreateFromImage2D(ImageData data, TextureUsage usage, TextureFormat? format = null)
        {
            IGraphicsDevice device = Engine.GraphicsDevice;

            ITexture gpu = device.CreateTexture(new TextureDescription()
            {
                Width = data.Width,
                Height = data.Height,
                Format = format ?? data.Format,
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
            Engine.Instance.TextureManager.Unregister(this);
            Gpu.Dispose();
        }
    }
}
