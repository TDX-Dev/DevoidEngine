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
        public int MipCount;

        [Key(4)]
        public byte[] PixelData = Array.Empty<byte>();
    }
}
