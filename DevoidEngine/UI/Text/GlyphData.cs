using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public class GlyphData
    {
        // Unicode
        public uint Codepoint;

        // FreeType glyph index
        public uint GlyphIndex;

        // Font metrics
        public float Advance;
        public float BearingX;
        public float BearingY;

        // Assigned during atlas packing
        //public Rect AtlasRect = new();

        public byte[] Bitmap = null!;
        // Bitmap dimensions
        public int Width;
        public int Height;
        public int Pitch;

        public GlyphSDFImage SDF;

        public int AtlasX;
        public int AtlasY;

        public Vector2 UVMin;
        public Vector2 UVMax;
    }
}
