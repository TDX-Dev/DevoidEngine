namespace DevoidEngine.Core
{
    public readonly struct ShaderDefineSet : IEquatable<ShaderDefineSet>
    {
        private readonly KeyValuePair<string, string>[] _defines;

        public IReadOnlyList<KeyValuePair<string, string>> Defines => _defines;

        public ShaderDefineSet(IEnumerable<KeyValuePair<string, string>>? defines)
        {
            if (defines is null)
            {
                this._defines = [];
                return;
            }

            List<KeyValuePair<string, string>> list = [];

            foreach (KeyValuePair<string, string> define in defines)
            {
                int index = -1;

                for (int i = 0; i < list.Count; i++)
                {
                    if (string.Equals(
                            list[i].Key,
                            define.Key,
                            StringComparison.Ordinal))
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                    list[index] = define;
                else
                    list.Add(define);
            }

            list.Sort(static (a, b) =>
                string.Compare(
                    a.Key,
                    b.Key,
                    StringComparison.Ordinal));

            this._defines = [.. list];
        }

        public ShaderDefineSet(IReadOnlyDictionary<string, string>? defines)
            : this(defines is null
                ? null
                : Enumerate(defines))
        {
        }

        private static IEnumerable<KeyValuePair<string, string>> Enumerate(IReadOnlyDictionary<string, string> defines)
        {
            foreach (KeyValuePair<string, string> define in defines)
                yield return define;
        }

        public bool Equals(ShaderDefineSet other)
        {
            KeyValuePair<string, string>[] left = _defines;
            KeyValuePair<string, string>[] right = other._defines;

            if (left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; i++)
            {
                if (!string.Equals(
                        left[i].Key,
                        right[i].Key,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                if (!string.Equals(
                        left[i].Value,
                        right[i].Value,
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj) => obj is ShaderDefineSet other && Equals(other);

        public static bool operator ==(ShaderDefineSet left, ShaderDefineSet right) => left.Equals(right);

        public static bool operator !=(ShaderDefineSet left, ShaderDefineSet right) => !left.Equals(right);

        public override int GetHashCode()
        {
            HashCode hash = new();

            KeyValuePair<string, string>[] values = _defines;
            if (values == null)
            {
                return 0;
            }

            for (int i = 0; i < values.Length; i++)
            {
                hash.Add(values[i].Key, StringComparer.Ordinal);
                hash.Add(values[i].Value, StringComparer.Ordinal);
            }

            return hash.ToHashCode();
        }

        public override string ToString()
        {
            if (_defines.Length == 0)
                return "Default";

            int length = 0;

            for (int i = 0; i < _defines.Length; i++)
            {
                if (i != 0)
                    length++;

                length += _defines[i].Key.Length;
                length++;
                length += _defines[i].Value.Length;
            }

            return string.Create(
                length,
                _defines,
                static (buffer, values) =>
                {
                    int position = 0;

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i != 0)
                            buffer[position++] = '_';

                        string key = values[i].Key;
                        key.AsSpan().CopyTo(buffer[position..]);
                        position += key.Length;

                        buffer[position++] = '=';

                        string value = values[i].Value;
                        value.AsSpan().CopyTo(buffer[position..]);
                        position += value.Length;
                    }
                });
        }
    }
}