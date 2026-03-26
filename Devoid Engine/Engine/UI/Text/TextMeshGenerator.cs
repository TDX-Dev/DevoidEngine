using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Text
{
    public static class TextMeshGenerator
    {
        private static List<Vertex> vertices = new List<Vertex>();
        private static List<int> indices = new List<int>();

        public static Mesh Generate(FontInternal font, string text, float scale, TextLayoutOptions options)
        {
            vertices.Clear();
            indices.Clear();

            if (string.IsNullOrEmpty(text) || font == null)
            {
                return new Mesh();
            }

            float fontAscender = font.Ascender;

            if (fontAscender == 0)
            {
                fontAscender = font.LineHeight * 0.8f;
            }

            float startX = 0;
            float startY = fontAscender * scale;

            float maxWidth = options.MaxWidth;

            Vector2 cursor = new Vector2(startX, startY);
            bool isFirstCharacterOfLine = true;

            List<int> lineVertexStart = new List<int>();
            List<float> lineWidths = new List<float>();

            int currentLineStart = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                uint charCode = (uint)c;

                if (c == '\n')
                {
                    lineVertexStart.Add(currentLineStart);
                    lineWidths.Add(cursor.X);

                    currentLineStart = vertices.Count;

                    cursor.X = startX;
                    cursor.Y += (font.Ascender - font.Descender) * scale;
                    isFirstCharacterOfLine = true;
                    continue;
                }

                if (!font.Metrics.TryGetValue(charCode, out var metric))
                {
                    continue;
                }

                float advance = metric.HorizontalAdvance * scale;

                bool overflow = !float.IsInfinity(maxWidth) && cursor.X + advance > maxWidth;

                float h, w, ypos, xpos, scaledBearingX, scaledBearingY;
                int indexOffset;

                if (overflow)
                {
                    if (options.Overflow == TextOverflow.Wrap)
                    {
                        lineVertexStart.Add(currentLineStart);
                        lineWidths.Add(cursor.X);

                        currentLineStart = vertices.Count;

                        cursor.X = startX;
                        cursor.Y += (font.Ascender - font.Descender) * scale;
                        isFirstCharacterOfLine = true;
                    }
                    else if (options.Overflow == TextOverflow.Clip)
                    {
                        break;
                    }
                    else if (options.Overflow == TextOverflow.Ellipsis)
                    {
                        string ellipsis = "...";

                        for (int e = 0; e < ellipsis.Length; e++)
                        {
                            uint ec = ellipsis[e];

                            if (!font.Metrics.TryGetValue(ec, out var em))
                                continue;

                            float eAdvance = em.HorizontalAdvance * scale;

                            if (cursor.X + eAdvance > maxWidth)
                                break;

                            scaledBearingY = em.BearingY * scale;
                            scaledBearingX = em.BearingX * scale;

                            xpos = cursor.X + scaledBearingX;
                            ypos = cursor.Y - scaledBearingY;

                            w = em.Width * scale;
                            h = em.Height * scale;

                            indexOffset = vertices.Count;

                            vertices.Add(new Vertex(new Vector3(xpos, ypos, 0), Vector3.Zero, new Vector2(em.U, em.V)));
                            vertices.Add(new Vertex(new Vector3(xpos + w, ypos, 0), Vector3.Zero, new Vector2(em.S, em.V)));
                            vertices.Add(new Vertex(new Vector3(xpos + w, ypos + h, 0), Vector3.Zero, new Vector2(em.S, em.T)));
                            vertices.Add(new Vertex(new Vector3(xpos, ypos + h, 0), Vector3.Zero, new Vector2(em.U, em.T)));

                            indices.Add(indexOffset + 0);
                            indices.Add(indexOffset + 1);
                            indices.Add(indexOffset + 2);
                            indices.Add(indexOffset + 0);
                            indices.Add(indexOffset + 2);
                            indices.Add(indexOffset + 3);

                            cursor.X += eAdvance;
                        }

                        break;
                    }
                }

                scaledBearingY = metric.BearingY * scale;
                scaledBearingX = metric.BearingX * scale;

                if (isFirstCharacterOfLine)
                {
                    cursor.X -= metric.OriginalBearingX * scale;
                    isFirstCharacterOfLine = false;
                }

                xpos = cursor.X + scaledBearingX;
                ypos = cursor.Y - scaledBearingY;

                w = metric.Width * scale;
                h = metric.Height * scale;

                indexOffset = vertices.Count;

                vertices.Add(new Vertex(new Vector3(xpos, ypos, 0), Vector3.Zero, new Vector2(metric.U, metric.V)));
                vertices.Add(new Vertex(new Vector3(xpos + w, ypos, 0), Vector3.Zero, new Vector2(metric.S, metric.V)));
                vertices.Add(new Vertex(new Vector3(xpos + w, ypos + h, 0), Vector3.Zero, new Vector2(metric.S, metric.T)));
                vertices.Add(new Vertex(new Vector3(xpos, ypos + h, 0), Vector3.Zero, new Vector2(metric.U, metric.T)));

                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 1);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 3);

                cursor.X += metric.HorizontalAdvance * scale;
            }

            lineVertexStart.Add(currentLineStart);
            lineWidths.Add(cursor.X);

            if (options.Align != TextAlign.Left && !float.IsInfinity(maxWidth))
            {
                for (int i = 0; i < lineVertexStart.Count; i++)
                {
                    float offset = 0;

                    if (options.Align == TextAlign.Center)
                        offset = (maxWidth - lineWidths[i]) * 0.5f;

                    if (options.Align == TextAlign.Right)
                        offset = maxWidth - lineWidths[i];

                    int start = lineVertexStart[i];
                    int end = (i + 1 < lineVertexStart.Count) ? lineVertexStart[i + 1] : vertices.Count;

                    for (int v = start; v < end; v++)
                    {
                        var vert = vertices[v];
                        var position = vert.Position;
                        position.X += offset;
                        vertices[v] = new Vertex(position, vert.Normal, vert.UV1, vert.Tangent, vert.BiTangent);
                    }
                }
            }

            // -------- PIXEL SNAP FIX (single offset) --------

            float snapX = MathF.Round(startX) - startX;
            float snapY = MathF.Round(startY) - startY;

            for (int i = 0; i < vertices.Count; i++)
            {
                var vert = vertices[i];
                var pos = vert.Position;

                pos.X += snapX;
                pos.Y += snapY;

                vertices[i] = new Vertex(pos, vert.Normal, vert.UV1, vert.Tangent, vert.BiTangent);
            }

            // -----------------------------------------------

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices.ToArray());
            mesh.SetIndices(indices.ToArray());
            return mesh;
        }

        public static Vector2 Measure(FontInternal font, string text, float scale, TextLayoutOptions options)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return Vector2.Zero;

            float lineHeight = (font.Ascender - font.Descender) * scale;
            float maxWidth = options.MaxWidth;

            float currentLineWidth = 0f;
            float maxLineWidth = 0f;
            float totalHeight = lineHeight;

            bool isFirstCharacterOfLine = true;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
                    currentLineWidth = 0;
                    totalHeight += lineHeight;
                    isFirstCharacterOfLine = true;
                    continue;
                }

                if (!font.Metrics.TryGetValue((uint)c, out var metric))
                    continue;

                float advance = metric.HorizontalAdvance * scale;

                // Apply the same bearing adjustment as Generate
                float adjustedAdvance = advance;
                if (isFirstCharacterOfLine)
                {
                    // We subtract the bearing just like in Generate to align the first char to 0
                    adjustedAdvance -= metric.OriginalBearingX * scale;
                }

                // Check for overflow BEFORE adding to currentLineWidth
                if (!float.IsInfinity(maxWidth) && (currentLineWidth + adjustedAdvance) > maxWidth)
                {
                    if (options.Overflow == TextOverflow.Wrap)
                    {
                        maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
                        currentLineWidth = 0; // Reset for new line
                        totalHeight += lineHeight;
                        isFirstCharacterOfLine = true;

                        // Recalculate advance for the start of the new line
                        adjustedAdvance = advance - (metric.OriginalBearingX * scale);
                    }
                    else if (options.Overflow == TextOverflow.Clip)
                    {
                        break;
                    }
                    else if (options.Overflow == TextOverflow.Ellipsis)
                    {
                        // Ellipsis usually takes up remaining space; 
                        // for measurement, we cap at maxWidth
                        currentLineWidth = maxWidth;
                        break;
                    }
                }

                currentLineWidth += adjustedAdvance;
                isFirstCharacterOfLine = false;
            }

            maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);

            return new Vector2(maxLineWidth, totalHeight);
        }

        public static Vector2 GetCursorPosition(FontInternal font, string text, float scale, TextLayoutOptions options)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return Vector2.Zero;

            float maxWidth = options.MaxWidth;
            float startX = 0;

            // We start Y at 0 so it aligns perfectly with the top of the current line
            Vector2 cursor = new Vector2(startX, 0);
            float lineHeight = (font.Ascender - font.Descender) * scale;
            bool isFirstCharacterOfLine = true;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    cursor.X = startX;
                    cursor.Y += lineHeight;
                    isFirstCharacterOfLine = true;
                    continue;
                }

                if (!font.Metrics.TryGetValue((uint)c, out var metric))
                    continue;

                float advance = metric.HorizontalAdvance * scale;

                // Use the lookahead logic we fixed earlier to prevent premature wrapping
                float nextX = cursor.X + advance;
                if (isFirstCharacterOfLine)
                {
                    nextX -= metric.OriginalBearingX * scale;
                }

                if (!float.IsInfinity(maxWidth) && nextX > maxWidth)
                {
                    if (options.Overflow == TextOverflow.Wrap)
                    {
                        cursor.X = startX;
                        cursor.Y += lineHeight;
                        isFirstCharacterOfLine = true;

                        // Recalculate nextX for the start of the new line
                        nextX = cursor.X + advance - (metric.OriginalBearingX * scale);
                    }
                    else if (options.Overflow == TextOverflow.Clip)
                    {
                        break;
                    }
                }

                cursor.X = nextX;
                isFirstCharacterOfLine = false;
            }

            return cursor;
        }
    }
}