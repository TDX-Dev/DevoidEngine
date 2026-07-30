using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.UI.Text
{
    public static class TextMeshBuilder
    {
        public static Mesh Build(
            Font font,
            TextLayoutResult layout,
            float fontSize,
            Mesh mesh)
        {
            float scale = fontSize / font.ReferenceSize;
            int glyphCount = layout.Glyphs.Count;


            mesh.Positions = new Vector3[glyphCount * 4];
            mesh.UVs = new Vector2[glyphCount * 4];
            mesh.Indices = new uint[glyphCount * 6];

            int vertex = 0;
            int index = 0;

            for (int i = 0; i < glyphCount; i++)
            {
                PositionedGlyph positioned = layout.Glyphs[i];

                Glyph glyph = positioned.Glyph;

                float x0 =
                    positioned.Position.X +
                    glyph.BearingX * scale;

                float y0 =
                    positioned.Position.Y -
                    glyph.BearingY * scale;

                float x1 = x0 + glyph.Width * scale;
                float y1 = y0 + glyph.Height * scale;

                mesh.Positions[vertex + 0] = new Vector3(x0, y0, 0);
                mesh.Positions[vertex + 1] = new Vector3(x1, y0, 0);
                mesh.Positions[vertex + 2] = new Vector3(x1, y1, 0);
                mesh.Positions[vertex + 3] = new Vector3(x0, y1, 0);

                Vector2 uvMin = glyph.UVMin;
                Vector2 uvMax = glyph.UVMax;

                mesh.UVs[vertex + 0] = new Vector2(uvMin.X, uvMin.Y);
                mesh.UVs[vertex + 1] = new Vector2(uvMax.X, uvMin.Y);
                mesh.UVs[vertex + 2] = new Vector2(uvMax.X, uvMax.Y);
                mesh.UVs[vertex + 3] = new Vector2(uvMin.X, uvMax.Y);

                mesh.Indices[index + 0] = (uint)(vertex + 0);
                mesh.Indices[index + 1] = (uint)(vertex + 1);
                mesh.Indices[index + 2] = (uint)(vertex + 2);

                mesh.Indices[index + 3] = (uint)(vertex + 0);
                mesh.Indices[index + 4] = (uint)(vertex + 2);
                mesh.Indices[index + 5] = (uint)(vertex + 3);

                vertex += 4;
                index += 6;
            }

            mesh.Upload();

            return mesh;
        }
    }
}
