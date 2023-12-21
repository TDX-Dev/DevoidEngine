using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using EmberaEngine.Engine.Core;
using SharpFont;
using OpenTK.Mathematics;
using System.Text;
using System.Diagnostics;
using EmberaEngine.Engine.Rendering;

namespace EmberaEngine.Engine.Utilities
{

    public class FontObject
    {
        public Texture fontTexture;
        public List<FontChar> fontChars = new List<FontChar>();

        public void AddFontChar(FontChar character)
        {
            fontChars.Add(character);
        }
        
    }

    public class FontChar
    {
        public int x, y;
        public int w, h;
        public char character;
        public int bearingX;
        public int bearingY;
        public int bitmapTop;
    }

    public  class FontUtilities
    {


        public static FontObject LoadFont(string fontPath, float size)
        {
            Library lib = new Library();

            // Dispose when done using
            MemoryStream ms = new MemoryStream();

            using (FileStream fs = new FileStream(fontPath, FileMode.Open, FileAccess.Read))
            {
                fs.CopyTo(ms);   
            }


            Face face = new Face(lib, ms.ToArray(), 0);

            face.SetPixelSizes(0, (uint)size);


            int _texWidth = 2048;
            int _texHeight = 2048;

            Texture _tex = new Texture(TextureTarget2d.Texture2D);
            _tex.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            _tex.SetWrapMode(TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            _tex.TexImage2D(_texWidth, _texHeight, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);

            //_tex.GenerateMipmap();
            int xOffSet = 0;
            int yOffSet = 0;
            FontObject fontObject = new FontObject();

            for (int i = 0; i < face.CharmapsCount; i++)
            {
                if (face.CharMaps[i].Encoding == SharpFont.Encoding.Unicode)
                {
                    face.SetCharmap(face.CharMaps[i]);
                    break;
                }
            }


            GraphicsState.SetPixelStoreI(PixelStoreParameter.UnpackAlignment, 1);

            for (uint charCode = 0; charCode < 65536; charCode++)
            {

                uint glyphIndex = face.GetCharIndex(charCode);
                if (glyphIndex != 0)
                {
                    face.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);
                    //face.Glyph.RenderGlyph(RenderMode.Normal);


                    if (face.Glyph == null || face.Glyph.Format != GlyphFormat.Bitmap || (face.Glyph.Bitmap.Rows == 0 || face.Glyph.Bitmap.Width == 0) || face.Glyph.Bitmap.BufferData.Length == 0) { continue; }
                    byte[] pixels = face.Glyph.Bitmap.BufferData;
                    if (xOffSet + face.Glyph.Bitmap.Width > _texWidth)
                    {
                        xOffSet = 0;
                        yOffSet += face.Glyph.Bitmap.Rows + 10;
                    }

                    FontChar fChar = new FontChar();
                    fChar.x = xOffSet;
                    fChar.y = yOffSet;
                    fChar.w = face.Glyph.Bitmap.Width;
                    fChar.h = face.Glyph.Bitmap.Rows;
                    fChar.character = (char)charCode;
                    fChar.bearingX = face.Glyph.Metrics.HorizontalBearingX.Value;
                    fChar.bitmapTop = face.Glyph.BitmapTop;

                    fontObject.AddFontChar(fChar);

                    _tex.SubTexture2DB(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows, PixelFormat.Red, PixelType.UnsignedByte, face.Glyph.Bitmap.BufferData, 0, xOffSet, yOffSet);
                    xOffSet += face.Glyph.Bitmap.Width + 10;
                }

                fontObject.fontTexture = _tex;
            }

            GraphicsState.SetPixelStoreI(PixelStoreParameter.UnpackAlignment, 4);

            return fontObject;
        }

        static int[] GetUnicodeCodePointsToLoad()
        {
            // Define the range of code points or a list of specific code points you want to load.
            // For example, load all characters in the basic multilingual plane (BMP).
            // You can customize this list to meet your specific needs.
            int startCodePoint = 0x0020; // Start from space (U+0020)
            int endCodePoint = 0x007E;   // End at tilde (U+007E)

            int[] codePointsToLoad = new int[endCodePoint - startCodePoint + 1];
            for (int i = 0; i < codePointsToLoad.Length; i++)
            {
                codePointsToLoad[i] = startCodePoint + i;
            }

            return codePointsToLoad;
        }
    }
}
