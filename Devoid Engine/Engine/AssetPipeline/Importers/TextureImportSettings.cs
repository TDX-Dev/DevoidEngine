using DevoidGPU;
using MessagePack;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{

    [MessagePackObject]
    public class TextureImportSettings
    {
        [Key(0)]
        public bool GenerateMipmaps = true;

        [Key(1)]
        public bool SRGB = true;

        [Key(2)]
        public bool Compress = true;
        [Key(3)]
        public TextureFilter Filter = TextureFilter.Linear;
        [Key(4)]
        public TextureWrapMode Wrap = TextureWrapMode.Repeat;
        [Key(5)]
        public int Anisotropy = 8;
    }
}
