using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public static class Helper
    {
        private static float SRGBToLinear(float v)
        {
            if (v <= 0.04045f)
                return v / 12.92f;

            return MathF.Pow((v + 0.055f) / 1.055f, 2.4f);
        }

        // ------------------------------------------
        // Color texture (Albedo / Emissive etc)
        // sRGB -> Linear conversion
        // ------------------------------------------
        public static Texture2D LoadImageAsTex(string file, TextureFilter textureFilter)
        {
            Image image = new Image();
            image.LoadPNGAsFloat(file);

            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = image.Width,
                Height = image.Height,
                Format = TextureFormat.RGBA16_Float,
                GenerateMipmaps = true,
                MipLevels = 0,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false
            });

            texture.SetFilter(textureFilter, textureFilter);
            texture.SetAnisotropy(8);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            float[] floatPixels = image.PixelHP;
            Half[] halfPixels = new Half[floatPixels.Length];

            for (int i = 0; i < floatPixels.Length; i++)
            {
                float v = floatPixels[i];

                if (v < 0f) v = 0f;
                if (v > 1f) v = 1f;

                v = SRGBToLinear(v);

                halfPixels[i] = (Half)v;
            }

            texture.SetData<Half>(halfPixels);
            texture.GenerateMipmaps();

            image.PixelHP = null;

            return texture;
        }

        // ------------------------------------------
        // Data textures (Roughness / Metallic / AO)
        // NO gamma conversion
        // ------------------------------------------
        public static Texture2D LoadImageAsDataTex(string file, TextureFilter textureFilter)
        {
            Image image = new Image();
            image.LoadPNGAsFloat(file);

            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = image.Width,
                Height = image.Height,
                Format = TextureFormat.RGBA8_UNorm,
                GenerateMipmaps = true,
                MipLevels = 0,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false
            });

            texture.SetFilter(textureFilter, textureFilter);
            texture.SetAnisotropy(8);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            float[] floatPixels = image.PixelHP;
            byte[] bytePixels = new byte[floatPixels.Length];

            for (int i = 0; i < floatPixels.Length; i++)
            {
                float v = floatPixels[i];

                if (v < 0f) v = 0f;
                if (v > 1f) v = 1f;

                bytePixels[i] = (byte)(v * 255.0f);
            }

            texture.SetData<byte>(bytePixels);
            texture.GenerateMipmaps();

            image.PixelHP = null;

            return texture;
        }

        // ------------------------------------------
        // Normal maps
        // Stored as linear UNorm data
        // ------------------------------------------
        public static Texture2D LoadNormalMap(string file, TextureFilter textureFilter)
        {
            Image image = new Image();
            image.LoadPNGAsFloat(file);

            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = image.Width,
                Height = image.Height,
                Format = TextureFormat.RGBA8_UNorm,
                GenerateMipmaps = true,
                MipLevels = 0,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false
            });

            texture.SetFilter(textureFilter, textureFilter);
            texture.SetAnisotropy(8);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            float[] floatPixels = image.PixelHP;
            byte[] bytePixels = new byte[floatPixels.Length];

            for (int i = 0; i < floatPixels.Length; i++)
            {
                float v = floatPixels[i];

                if (v < 0f) v = 0f;
                if (v > 1f) v = 1f;

                bytePixels[i] = (byte)(v * 255.0f);
            }

            texture.SetData<byte>(bytePixels);
            texture.GenerateMipmaps();

            image.PixelHP = null;

            return texture;
        }

        public static Vector3[] VertexToVector3(Vertex[] vertices)
        {
            List<Vector3> verticesList = new List<Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                verticesList.Add(vertices[i].Position);
            }
            return verticesList.ToArray();
        }
    }
}