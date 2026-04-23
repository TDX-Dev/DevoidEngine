namespace DevoidGPU.DX11
{
    internal static class DX11MemoryEstimator
    {
        internal static long CalculateTextureSize(TextureDescription description)
        {
            float mipFactor = description.MipLevels > 1 ? 1.333f : 1f;
            long size =
                description.Width *
                description.Height *
                Math.Max(1, description.Depth) *
                DX11StateMapper.BytesPerComponent(description.Format) *
                description.ArraySize;

            size = (long)(size * mipFactor);

            return size;
        }

    }
}
