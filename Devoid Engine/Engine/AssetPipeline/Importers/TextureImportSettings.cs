using DevoidGPU;
using MessagePack;

namespace DevoidEngine.Engine.AssetPipeline.Importers
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
        public TextureFilter Filter = TextureFilter.Linear;
        [Key(4)]
        public TextureWrapMode Wrap = TextureWrapMode.Repeat;
        [Key(5)]
        public int Anisotropy = 8;
        [Key(6)]
        public TextureFormat Format = TextureFormat.RGBA8_UNorm;
    }
}
