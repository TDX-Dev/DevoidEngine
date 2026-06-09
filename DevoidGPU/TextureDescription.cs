using System.Diagnostics.CodeAnalysis;

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

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }
        public static bool operator ==(TextureDescription left, TextureDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureDescription left, TextureDescription right)
        {
            return !(left == right);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(
                HashCode.Combine(
                    Dimension,
                    Width,
                    Height,
                    Depth,
                    MipLevels
                ),
                 HashCode.Combine(
                     ArraySize,
                     Format,
                     Usage,
                     Samples
                )
            );
        }
    }
}
