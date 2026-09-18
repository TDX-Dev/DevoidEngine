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

            Vector3[] positions = new Vector3[glyphCount * 4];
            Vector2[] uvs = new Vector2[glyphCount * 4];
            uint[] indices = new uint[glyphCount * 6];

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

                positions[vertex + 0] = new Vector3(x0, y0, 0);
                positions[vertex + 1] = new Vector3(x1, y0, 0);
                positions[vertex + 2] = new Vector3(x1, y1, 0);
                positions[vertex + 3] = new Vector3(x0, y1, 0);

                Vector2 uvMin = glyph.UVMin;
                Vector2 uvMax = glyph.UVMax;

                uvs[vertex + 0] = new Vector2(uvMin.X, uvMin.Y);
                uvs[vertex + 1] = new Vector2(uvMax.X, uvMin.Y);
                uvs[vertex + 2] = new Vector2(uvMax.X, uvMax.Y);
                uvs[vertex + 3] = new Vector2(uvMin.X, uvMax.Y);

                indices[index + 0] = (uint)(vertex + 0);
                indices[index + 1] = (uint)(vertex + 1);
                indices[index + 2] = (uint)(vertex + 2);

                indices[index + 3] = (uint)(vertex + 0);
                indices[index + 4] = (uint)(vertex + 2);
                indices[index + 5] = (uint)(vertex + 3);

                vertex += 4;
                index += 6;
            }

            MeshSurface surface = new(positions, null, uvs, null, indices);

            mesh.Surfaces = [surface];
            mesh.Upload();

            return mesh;
        }
    }
}