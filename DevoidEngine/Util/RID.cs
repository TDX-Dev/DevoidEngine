namespace DevoidEngine.Util
{
    public readonly struct Rid : IEquatable<Rid>
    {
        public readonly int Index;
        public readonly uint Generation;

        public Rid(int index, uint generation)
        {
            Index = index;
            Generation = generation;
        }

        public override string ToString() => $"{Generation}:{Index}";
        public bool Equals(Rid other) => Index == other.Index && Generation == other.Generation;

        public override bool Equals(object? obj) => obj is Rid other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Index, Generation);

        public static readonly Rid Invalid = new(-1, 0);

        public bool IsValid => Index >= 0 && Generation != 0;
        public static bool operator ==(Rid left, Rid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rid left, Rid right)
        {
            return !(left == right);
        }
    }
}
