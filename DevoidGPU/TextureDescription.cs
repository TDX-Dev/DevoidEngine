namespace DevoidGPU
{
    public struct TextureDescription : IEquatable<TextureDescription>
    {
        public TextureDimension Dimension;
        public int Width;
        public int Height;
        public int Depth;

        public int MipLevels;
        public int ArraySize;
        public TextureFormat Format;

        public TextureUsage Usage;
        public TextureSampleDescription Samples;

        public readonly bool Equals(TextureDescription other)
        {
            return Dimension == other.Dimension &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Depth == other.Depth &&
                   MipLevels == other.MipLevels &&
                   ArraySize == other.ArraySize &&
                   Format == other.Format &&
                   Usage == other.Usage &&
                   Samples.Equals(other.Samples);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TextureDescription other &&
                   Equals(other);
        }
        public static bool operator ==(TextureDescription left, TextureDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureDescription left, TextureDescription right)
        {
            return !(left == right);
        }

#pragma warning disable IDE0070 // Use 'System.HashCode'
        public override readonly int GetHashCode()
#pragma warning restore IDE0070 // Use 'System.HashCode'
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 31 + (int)Dimension;
                hash = hash * 31 + Width;
                hash = hash * 31 + Height;
                hash = hash * 31 + Depth;
                hash = hash * 31 + MipLevels;
                hash = hash * 31 + ArraySize;
                hash = hash * 31 + (int)Format;
                hash = hash * 31 + (int)Usage;
                hash = hash * 31 + Samples.GetHashCode();

                return hash;
            }
        }
    }
}
