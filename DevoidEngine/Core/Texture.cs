using DevoidEngine.Assets;
using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class Texture : AssetType
    {
        public ITexture GPU { get; }

        public TextureDimension Dimension => GPU.Description.Dimension;
        public int Width => GPU.Description.Width;
        public int Height => GPU.Description.Height;
        public int Depth => GPU.Description.Depth;
        public int MipLevels => GPU.MipLevels;
        public int ArraySize => GPU.ArraySize;
        public TextureUsage Usage => GPU.Description.Usage;
        public TextureFormat Format => GPU.Format;

        public ulong ID { get; } = 0;

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
            ID = Engine.Instance.TextureManager.Register(this);
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

        public override void Dispose()
        {
            Engine.Instance.TextureManager.Unregister(this);
            GPU.Dispose();
        }
    }
}
