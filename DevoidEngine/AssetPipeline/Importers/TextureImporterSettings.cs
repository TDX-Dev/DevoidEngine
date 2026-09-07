using DevoidGPU;
using MessagePack;

namespace DevoidEngine.AssetPipeline.Importers
{
    [MessagePackObject]
    public class TextureImportSettings
    {
        public const int CurrentVersion = 0;
        [Key(0)]
        public bool GenerateMipmaps = true;

        [Key(1)]
        public bool SRGB = false;

        [Key(2)]
        public bool Compress = true;
        [Key(3)]
        public FilterMode Filter = FilterMode.Linear;
        [Key(4)]
        public WrapMode WrapU = WrapMode.Repeat;
        [Key(5)]
        public WrapMode WrapV = WrapMode.Repeat;
        [Key(6)]
        public WrapMode WrapW = WrapMode.Repeat;
        [Key(7)]
        public int Anisotropy = 8;
        [Key(8)]
        public TextureFormat Format = TextureFormat.RGBA8_UNorm;
    }
}
