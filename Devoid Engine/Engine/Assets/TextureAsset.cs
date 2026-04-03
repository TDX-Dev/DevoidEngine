using DevoidGPU;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Assets
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
        public TextureFilter Filter;
        [Key(4)]
        public TextureWrapMode Wrap;
        [Key(5)]
        public int Anisotropy;

        [Key(6)]
        public byte[] PixelData = Array.Empty<byte>();
    }


}
