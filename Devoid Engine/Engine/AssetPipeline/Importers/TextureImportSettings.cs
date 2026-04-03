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
    }
}
