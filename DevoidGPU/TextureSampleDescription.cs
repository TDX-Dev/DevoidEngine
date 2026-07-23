namespace DevoidGPU
{
    public struct TextureSampleDescription :
        IEquatable<TextureSampleDescription>
    {
        public int Count;
        public int Quality;

        public TextureSampleDescription(
            int count,
            int quality)
        {
            Count = count;
            Quality = quality;
        }

        public readonly bool Equals(
            TextureSampleDescription other)
        {
            return Count == other.Count &&
                   Quality == other.Quality;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TextureSampleDescription other &&
                   Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(
                Count,
                Quality);
        }

        public static bool operator ==(
            TextureSampleDescription left,
            TextureSampleDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            TextureSampleDescription left,
            TextureSampleDescription right)
        {
            return !left.Equals(right);
        }
    }
}
