using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public static class TextLayout
    {
        public static TextLayoutResult Layout(
            Font font,
            string text,
            TextLayoutSettings settings)
        {
            float scale = settings.FontSize / font.ReferenceSize;

            TextLayoutResult result = new();

            float penX = 0;
            float penY = font.Ascent * scale;

            List<int> lineStarts = [];
            List<float> lineWidths = [];

            lineStarts.Add(0);

            float currentLineWidth = 0;


            uint previous = 0;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    lineWidths.Add(currentLineWidth);

                    currentLineWidth = 0;

                    penX = 0;
                    penY += font.LineHeight * scale;

                    previous = 0;

                    lineStarts.Add(result.Glyphs.Count);

                    continue;
                }

                if (!font.Glyphs.TryGetValue(c, out Glyph glyph))
                    continue;

                if (previous != 0 &&
                    font.Kerning.TryGetValue((previous, glyph.Codepoint), out float kern))
                {
                    penX += kern * scale;
                }

                result.Glyphs.Add(new PositionedGlyph
                {
                    Glyph = glyph,
                    Position = new Vector2(penX, penY),
                });

                penX += glyph.Advance * scale;

                currentLineWidth = penX;

                previous = glyph.Codepoint;
            }

            lineWidths.Add(currentLineWidth);

            result.Width = lineWidths.Count == 0 ? 0 : lineWidths.Max();
            result.Height = lineWidths.Count * font.LineHeight * scale;
            result.LineCount = lineWidths.Count;
            result.LineWidths = lineWidths;
            result.LineStarts = lineStarts;


            // Horizontal Align

            for (int line = 0; line < result.LineCount; line++)
            {
                int start = result.LineStarts[line];

                int end = line == result.LineCount - 1
                    ? result.Glyphs.Count
                    : result.LineStarts[line + 1];

                float lineWidth = result.LineWidths[line];

                float offset = settings.HorizontalAlignment switch
                {
                    TextHorizontalAlignment.Left => 0.0f,
                    TextHorizontalAlignment.Center => (settings.MaxWidth - lineWidth) * 0.5f,
                    TextHorizontalAlignment.Right => result.Width - lineWidth,
                    _ => 0.0f
                };

                for (int i = start; i < end; i++)
                {
                    PositionedGlyph glyph = result.Glyphs[i];
                    glyph.Position.X += offset;
                    result.Glyphs[i] = glyph;
                }
            }

            float offsetY = settings.VerticalAlignment switch
            {
                TextVerticalAlignment.Top => 0,
                TextVerticalAlignment.Center => (settings.MaxHeight - result.Height) * 0.5f,
                TextVerticalAlignment.Bottom => settings.MaxHeight - result.Height,
                _ => 0
            };

            if (offsetY != 0)
            {
                for (int i = 0; i < result.Glyphs.Count; i++)
                {
                    PositionedGlyph glyph = result.Glyphs[i];
                    glyph.Position.Y += offsetY;
                    result.Glyphs[i] = glyph;
                }
            }

            return result;
        }
    }
}
