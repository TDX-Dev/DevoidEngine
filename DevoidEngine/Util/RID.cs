namespace DevoidEngine.Util
{
    public readonly struct RID : IEquatable<RID>
    {
        public readonly int Index;
        public readonly uint Generation;

        public RID(int index, uint generation)
        {
            Index = index;
            Generation = generation;
        }

        public bool Equals(RID other)
            => Index == other.Index && Generation == other.Generation;

        public override bool Equals(object? obj)
            => obj is RID other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Index, Generation);

        public static readonly RID Invalid = new(-1, 0);

        public bool IsValid =>
            Index >= 0 &&
            Generation != 0;
        public static bool operator ==(RID left, RID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RID left, RID right)
        {
            return !(left == right);
        }
    }
}
