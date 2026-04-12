using DevoidEngine.Engine.Core;
using DevoidGPU;
using StbImageSharp;
using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public static class Helper
    {
        public static void Measure(string name, Action action)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();

            action();

            long after = GC.GetAllocatedBytesForCurrentThread();
            long alloc = after - before;

            if (alloc > 0)
                Console.WriteLine($"{name}: {alloc} B");
        }

        public static Matrix4x4 BuildModel(
            Vector3 position,
            Vector3 scale,
            Quaternion rotation
        )
        {
            return
                Matrix4x4.CreateScale(scale) *
                Matrix4x4.CreateFromQuaternion(rotation) *
                Matrix4x4.CreateTranslation(position);
        }

        public static Vector4 RGBANormalize(Vector4 color) => color / 255;

        public static float SRGBToLinear(float v)
        {
            if (v <= 0.04045f)
                return v / 12.92f;

            return MathF.Pow((v + 0.055f) / 1.055f, 2.4f);
        }


        public static void LoadImageFloat(ReadOnlySpan<byte> data, out int width, out int height, out float[] pixels)
        {
            Image image = new Image();
            image.LoadPNGAsFloatFromMemory(data);

            width = image.Width;
            height = image.Height;
            pixels = image.PixelHP;
        }

        public static void LoadImageFloatSRGB(ReadOnlySpan<byte> data, out int width, out int height, out Half[] pixels)
        {
            Image image = new Image();
            image.LoadPNGAsFloatFromMemory(data);

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

            width = image.Width;
            height = image.Height;
            pixels = halfPixels;
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

            return texture;
        }

        public static Texture2D LoadImageAsTex(ReadOnlySpan<byte> data, TextureFilter textureFilter)
        {
            Image image = new Image();
            image.LoadPNGAsFloatFromMemory(data);

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

            texture.SetData(halfPixels);
            texture.GenerateMipmaps();

            return texture;
        }

        public static Texture2D LoadImageAsDataTex(ReadOnlySpan<byte> data, TextureFilter textureFilter)
        {
            Image image = new Image();
            image.LoadPNGAsFloatFromMemory(data);

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

                bytePixels[i] = (byte)(v * 255f);
            }

            texture.SetData(bytePixels);
            texture.GenerateMipmaps();

            return texture;
        }

        public static Texture2D LoadNormalMap(ReadOnlySpan<byte> data, TextureFilter textureFilter)
        {
            Image image = new Image();
            image.LoadPNGAsFloatFromMemory(data);

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

                bytePixels[i] = (byte)(v * 255f);
            }

            texture.SetData(bytePixels);
            texture.GenerateMipmaps();

            return texture;
        }

        static Texture2D? cachedHdr;

        public static Texture2D LoadCachedHDRI(string file)
        {
            if (cachedHdr != null)
                return cachedHdr;

            cachedHdr = LoadHDRI(file);
            return cachedHdr;
        }

        public static Texture2D LoadHDRI(string file)
        {
            using var stream = File.OpenRead(file);

            var result = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);

            int width = result.Width;
            int height = result.Height;

            float[] rgb = result.Data;

            float[] rgba = new float[width * height * 4];

            for (int i = 0, j = 0; i < rgb.Length; i += 3, j += 4)
            {
                rgba[j + 0] = rgb[i + 0];
                rgba[j + 1] = rgb[i + 1];
                rgba[j + 2] = rgb[i + 2];
                rgba[j + 3] = 1.0f;
            }

            result.Data = null;

            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA32_Float,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsDepthStencil = false,
                IsRenderTarget = false,
                IsMutable = false
            });

            texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            texture.SetFilter(TextureFilter.Linear, TextureFilter.Linear);

            texture.SetData<float>(rgba);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return texture;
        }

        // Data textures (Roughness / Metallic / AO)
        // NO gamma conversion
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

            return texture;
        }

        // Normal maps
        // Stored as linear UNorm data
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