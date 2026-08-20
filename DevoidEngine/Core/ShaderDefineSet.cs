namespace DevoidEngine.Core
{
    public readonly struct ShaderDefineSet : IEquatable<ShaderDefineSet>
    {
        private readonly KeyValuePair<string, string>[] defines;

        public IReadOnlyList<KeyValuePair<string, string>> Defines =>
            defines;

        public ShaderDefineSet(
            IEnumerable<KeyValuePair<string, string>>? defines)
        {
            this.defines =
                defines?
                    .GroupBy(
                        x => x.Key,
                        StringComparer.Ordinal)
                    .Select(x => x.Last())
                    .OrderBy(
                        x => x.Key,
                        StringComparer.Ordinal)
                    .ToArray()
                ?? [];
        }

        public ShaderDefineSet(
            IReadOnlyDictionary<string, string>? defines)
            : this(defines?.AsEnumerable())
        {
        }

        public bool Equals(ShaderDefineSet other)
        {
            if (defines.Length != other.defines.Length)
                return false;

            for (int i = 0; i < defines.Length; i++)
            {
                if (!string.Equals(
                        defines[i].Key,
                        other.defines[i].Key,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                if (!string.Equals(
                        defines[i].Value,
                        other.defines[i].Value,
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is ShaderDefineSet other &&
               Equals(other);

        public static bool operator ==(
            ShaderDefineSet left,
            ShaderDefineSet right)
            => left.Equals(right);

        public static bool operator !=(
            ShaderDefineSet left,
            ShaderDefineSet right)
            => !left.Equals(right);

        public override int GetHashCode()
        {
            HashCode hash = new();

            foreach (var define in defines)
            {
                hash.Add(
                    define.Key,
                    StringComparer.Ordinal);

                hash.Add(
                    define.Value,
                    StringComparer.Ordinal);
            }

            return hash.ToHashCode();
        }

        public override string ToString()
        {
            if (defines.Length == 0)
                return "Default";

            return string.Join(
                "_",
                defines.Select(
                    x => $"{x.Key}={x.Value}"));
        }
    }
}