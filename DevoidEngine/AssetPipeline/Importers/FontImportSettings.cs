using DevoidEngine.UI.Text;
using MessagePack;

namespace DevoidEngine.AssetPipeline.Importers
{
    [MessagePackObject]
    public class FontImportSettings
    {
        public const int CurrentVersion = 0;
        [Key(0)]
        public float PixelRange = 16f;

        [Key(1)]
        public uint SourceGlyphSize = 128;
        [Key(2)]
        public FontEncodingTypes FontEncoding = FontEncodingTypes.Unicode;
        [Key(3)]
        public FontCharacterSet CharacterSet = FontCharacterSet.ASCII;
        [Key(4)]
        public string Characters = "";
        [Key(5)]
        public int SuperSampleScale = 4;
    }
}
