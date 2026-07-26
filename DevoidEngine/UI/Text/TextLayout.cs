using SharpFont;
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
        static readonly List<LineBuilder> lines = [];
        static readonly List<PositionedGlyph> glyphs = [];

        private struct LineBuilder
        {
            public float PenX;
            public float PenY;
            public float Width;
            public uint PreviousGlyph;

            public int StartGlyph;
            public int GlyphCount;
        }

        public static void Layout(
            Font font,
            string text,
            TextLayoutSettings settings,
            TextLayoutResult result)
        {
            float scale = settings.FontSize / font.ReferenceSize;

            lines.Clear();
            glyphs.Clear();

            result.Clear();

            result.Glyphs.EnsureCapacity(text.Length);
            result.LineStarts.EnsureCapacity(8);
            result.LineWidths.EnsureCapacity(8);

            glyphs.EnsureCapacity(text.Length);

            LineBuilder currentLine = CreateLine(font.Ascent * scale);

            int lastSpaceTextIndex = -1;
            int lastSpaceGlyphCount = 0;
            bool skipUntilNewline = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                {
                    currentLine.Width = currentLine.PenX;
                    lines.Add(currentLine);
                    currentLine = CreateLine(currentLine.PenY + font.LineHeight * scale);

                    lastSpaceTextIndex = -1;
                    skipUntilNewline = false;
                    continue;
                }

                if (skipUntilNewline)
                    continue;

                if (!font.Glyphs.TryGetValue(c, out Glyph glyph))
                    continue;

                float kern = 0;
                if (currentLine.PreviousGlyph != 0 &&
                    font.Kerning.TryGetValue((currentLine.PreviousGlyph, glyph.Codepoint), out float rawKern))
                {
                    kern = rawKern * scale;
                }

                float advance = (glyph.Advance * scale) + kern;

                // Handle Overflow
                if (settings.Overflow != TextOverflow.None && currentLine.PenX + advance > settings.MaxWidth)
                {
                    if (settings.Overflow == TextOverflow.Wrap)
                    {
                        if (lastSpaceTextIndex != -1)
                        {
                            // Rollback to the last space to break the word cleanly
                            if (lastSpaceGlyphCount < currentLine.GlyphCount)
                            {
                                int index = currentLine.StartGlyph + lastSpaceGlyphCount;

                                currentLine.PenX = glyphs[index].Position.X;

                                glyphs.RemoveRange(
                                    index,
                                    currentLine.GlyphCount - lastSpaceGlyphCount);

                                currentLine.GlyphCount = lastSpaceGlyphCount;
                            }

                            currentLine.Width = currentLine.PenX;
                            lines.Add(currentLine);
                            currentLine = CreateLine(currentLine.PenY + font.LineHeight * scale);

                            i = lastSpaceTextIndex; // Rewind iterator so the next loop processes the character AFTER the space
                            lastSpaceTextIndex = -1;
                            continue;
                        }
                        else
                        {
                            // No space available: Force wrap mid-word
                            currentLine.Width = currentLine.PenX;
                            lines.Add(currentLine);
                            currentLine = CreateLine(currentLine.PenY + font.LineHeight * scale);

                            kern = 0; // Reset kerning for the first character of the new line
                            advance = glyph.Advance * scale;
                        }
                    }
                    else if (settings.Overflow == TextOverflow.Clip)
                    {
                        skipUntilNewline = true;
                        continue;
                    }
                    else if (settings.Overflow == TextOverflow.Ellipsis)
                    {
                        ApplyEllipsis(ref currentLine, font, scale, settings.MaxWidth);
                        skipUntilNewline = true;
                        continue;
                    }
                }

                // Track space index before adding it, so we can split exactly at this point
                if (char.IsWhiteSpace(c))
                {
                    lastSpaceTextIndex = i;
                    lastSpaceGlyphCount = currentLine.GlyphCount;
                }

                glyphs.Add(new PositionedGlyph
                {
                    Glyph = glyph,
                    Position = new Vector2(currentLine.PenX + kern, currentLine.PenY)
                });

                currentLine.GlyphCount++;

                currentLine.PenX += advance;
                currentLine.PreviousGlyph = glyph.Codepoint;
            }

            // Add the final line
            currentLine.Width = currentLine.PenX;
            lines.Add(currentLine);

            BuildFinalResult(lines, font, settings, scale, result);
        }

        private static LineBuilder CreateLine(float penY)
        {
            return new LineBuilder
            {
                PenX = 0,
                PenY = penY,
                Width = 0,
                PreviousGlyph = 0,
                StartGlyph = glyphs.Count,
                GlyphCount = 0
            };
        }

        private static void ApplyEllipsis(ref LineBuilder currentLine, Font font, float scale, float maxWidth)
        {
            bool hasDot = font.Glyphs.TryGetValue('.', out Glyph dot);
            float dotAdvance = hasDot ? dot.Advance * scale : 0;
            float ellipsisWidth = dotAdvance * 3;

            // Rollback glyphs until we have enough space to fit "..."
            while (currentLine.GlyphCount > 0 &&
                   currentLine.PenX + ellipsisWidth > maxWidth)
            {
                int lastIndex = currentLine.StartGlyph + currentLine.GlyphCount - 1;

                currentLine.PenX = glyphs[lastIndex].Position.X;

                glyphs.RemoveAt(lastIndex);

                currentLine.GlyphCount--;
            }

            if (hasDot)
            {
                for (int e = 0; e < 3; e++)
                {
                    glyphs.Add(new PositionedGlyph
                    {
                        Glyph = dot,
                        Position = new Vector2(currentLine.PenX, currentLine.PenY)
                    });

                    currentLine.PenX += dotAdvance;
                    currentLine.GlyphCount++;
                }
            }

            currentLine.Width = currentLine.PenX;
        }

        private static void BuildFinalResult(
            List<LineBuilder> lines,
            Font font,
            TextLayoutSettings settings,
            float scale,
            TextLayoutResult result)
        {

            float maxContentWidth = 0;

            result.Glyphs.AddRange(glyphs);

            foreach (LineBuilder line in lines)
            {
                result.LineStarts.Add(line.StartGlyph);
                result.LineWidths.Add(line.Width);

                if (line.Width > maxContentWidth)
                        maxContentWidth = line.Width;
            }

            result.Width = settings.Overflow == TextOverflow.None ? maxContentWidth : Math.Min(maxContentWidth, settings.MaxWidth);
            result.Height = lines.Count * font.LineHeight * scale;

            result.LineCount = lines.Count;
            // Apply Horizontal Alignments
            for (int line = 0; line < result.LineCount; line++)
            {
                int start = result.LineStarts[line];
                int end = line == result.LineCount - 1 ? result.Glyphs.Count : result.LineStarts[line + 1];

                float lineWidth = result.LineWidths[line];
                float alignBoxWidth = settings.Overflow == TextOverflow.None ? result.Width : settings.MaxWidth;

                float offsetX = settings.HorizontalAlignment switch
                {
                    TextHorizontalAlignment.Center => (alignBoxWidth - lineWidth) * 0.5f,
                    TextHorizontalAlignment.Right => alignBoxWidth - lineWidth,
                    _ => 0.0f
                };

                if (offsetX != 0)
                {
                    for (int i = start; i < end; i++)
                    {
                        PositionedGlyph glyph = result.Glyphs[i];
                        glyph.Position.X += offsetX;
                        result.Glyphs[i] = glyph;
                    }
                }
            }

            // Apply Vertical Alignments
            float alignBoxHeight = (settings.Overflow == TextOverflow.None || settings.MaxHeight <= 0) ? result.Height : settings.MaxHeight;
            float offsetY = settings.VerticalAlignment switch
            {
                TextVerticalAlignment.Center => (alignBoxHeight - result.Height) * 0.5f,
                TextVerticalAlignment.Bottom => alignBoxHeight - result.Height,
                _ => 0.0f
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
        }
    }
}
