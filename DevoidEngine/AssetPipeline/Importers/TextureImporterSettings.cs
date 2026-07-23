using DevoidGPU;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public WrapMode Wrap = WrapMode.MirrorRepeat;
        [Key(5)]
        public int Anisotropy = 8;
        [Key(6)]
        public TextureFormat Format = TextureFormat.RGBA8_UNorm;
    }
}
