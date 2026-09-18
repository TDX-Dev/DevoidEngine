using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering.PostProcessing
{
    public readonly struct BlurMip
    {
        public readonly Vector2 Size;
        public readonly Texture Texture;

        public BlurMip(
            Vector2 size,
            Texture texture)
        {
            Size = size;
            Texture = texture;
        }
    }

    public static class BlurUtil
    {
        public static void BuildMipChain(
            PostProcessContext ctx,
            Texture source,
            int mipCount,
            string namePrefix,
            List<BlurMip> mips)
        {
            mips.Clear();

            TextureDescription desc = source.GPU.Description;

            int width = Math.Max(desc.Width >> 1, 1);
            int height = Math.Max(desc.Height >> 1, 1);

            for (int i = 0; i < mipCount; i++)
            {
                desc.Width = width;
                desc.Height = height;
                desc.MipLevels = 1;

                Texture texture =
                    ctx.RenderContext.Resources.GetOrCreateTexture(
                        $"{namePrefix}_{i}",
                        desc);

                mips.Add(new BlurMip(
                    new Vector2(width, height),
                    texture));

                if (width == 1 && height == 1)
                    break;

                width = Math.Max(width >> 1, 1);
                height = Math.Max(height >> 1, 1);
            }
        }
    }
}