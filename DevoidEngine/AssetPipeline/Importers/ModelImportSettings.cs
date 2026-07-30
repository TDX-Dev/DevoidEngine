using DevoidEngine.Util;
using MessagePack;

namespace DevoidEngine.AssetPipeline.Importers
{
    [MessagePackObject]
    public class ModelImportSettings
    {
        public const int CurrentVersion = 0;
        [Key(0)]
        public Axis SourceUp = Axis.Y;

        [Key(1)]
        public Axis SourceForward = Axis.NegZ;

        [Key(2)]
        public float ImportScale = 1.0f;

        [Key(3)]
        public bool FlipUVs = false;
    }
}
