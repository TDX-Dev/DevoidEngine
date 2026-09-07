using DevoidGPU;
using MessagePack;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class TextureAsset
    {
        [Key(0)]
        public int Width;
        [Key(1)]
        public int Height;
        [Key(2)]
        public TextureFormat Format;
        [Key(3)]
        public FilterMode Filter;
        [Key(4)]
        public WrapMode WrapU = WrapMode.Repeat;
        [Key(5)]
        public WrapMode WrapV = WrapMode.Repeat;
        [Key(6)]
        public WrapMode WrapW = WrapMode.Repeat;
        [Key(7)]
        public int Anisotropy;
        [Key(8)]
        public byte[] PixelData = [];
        [Key(9)]
        public bool GenerateMipmaps = true;
    }

}
