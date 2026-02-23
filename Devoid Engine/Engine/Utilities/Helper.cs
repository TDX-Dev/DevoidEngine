using DevoidEngine.Engine.Core;
using DevoidGPU;

namespace DevoidEngine.Engine.Utilities
{
    public static class Helper
    {
        public static Texture2D loadImageAsTex(string file, TextureFilter textureFilter)
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
                halfPixels[i] = (Half)floatPixels[i];

            texture.SetData<Half>(halfPixels);

            texture.GenerateMipmaps();

            image.PixelHP = [];

            return texture;
        }


    }
}
