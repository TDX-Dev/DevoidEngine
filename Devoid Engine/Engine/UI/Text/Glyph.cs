using System.Numerics;

namespace DevoidEngine.Engine.UI.Text
{
    struct Glyph
    {
        public uint Codepoint;

        public Vector2 Size;
        public Vector2 Bearing;
        public float Advance;

        public Rect UV;
    }

}