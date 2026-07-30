using MessagePack;
using System.Numerics;

namespace DevoidEngine.UI.Text
{
    [MessagePackObject]
    public struct Glyph
    {
        [Key(0)]
        public uint Codepoint;

        [Key(1)]
        public float Advance;

        [Key(2)]
        public float BearingX;

        [Key(3)]
        public float BearingY;

        [Key(4)]
        public float Width;

        [Key(5)]
        public float Height;

        [Key(6)]
        public Vector2 UVMin;

        [Key(7)]
        public Vector2 UVMax;
    }
}
