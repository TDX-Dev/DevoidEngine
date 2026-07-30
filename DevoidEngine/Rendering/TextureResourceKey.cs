using DevoidGPU;

namespace DevoidEngine.Rendering
{
    public readonly struct TextureResourceKey : IEquatable<TextureResourceKey>
    {
        public readonly string Name;
        public readonly TextureDescription Description;

        public TextureResourceKey(string name, TextureDescription description)
        {
            Name = name;
            Description = description;
        }

        public bool Equals(TextureResourceKey other)
        {
            return Name == other.Name &&
                   Description.Equals(other.Description);
        }

        public override bool Equals(object? obj)
        {
            return obj is TextureResourceKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description);
        }
        public static bool operator ==(TextureResourceKey left, TextureResourceKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureResourceKey left, TextureResourceKey right)
        {
            return !(left == right);
        }
    }
}
