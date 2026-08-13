using DevoidEngine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public struct GizmoMaterial : IEquatable<GizmoMaterial>
    {
        public Vector4 Color;
        public Texture? Texture;

        public Vector2 UVMin;
        public Vector2 UVMax;

        public bool Billboard;

        public static GizmoMaterial Default => new()
        {
            Color = Vector4.One,
            UVMin = Vector2.Zero,
            UVMax = Vector2.One
        };

        public static GizmoMaterial Colored(Vector4 color)
        {
            return new GizmoMaterial
            {
                Color = color,
                UVMin = Vector2.Zero,
                UVMax = Vector2.One
            };
        }

        public static GizmoMaterial Textured(
            Texture texture,
            Vector4 color = default)
        {
            return new GizmoMaterial
            {
                Color = color == default
                    ? Vector4.One
                    : color,

                Texture = texture,
                UVMin = Vector2.Zero,
                UVMax = Vector2.One
            };
        }

        public readonly bool Equals(GizmoMaterial other)
        {
            return Color == other.Color &&
                   ReferenceEquals(Texture, other.Texture) &&
                   UVMin == other.UVMin &&
                   UVMax == other.UVMax &&
                   Billboard == other.Billboard;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is GizmoMaterial other &&
                   Equals(other);
        }

        public static bool operator ==(
            GizmoMaterial left,
            GizmoMaterial right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            GizmoMaterial left,
            GizmoMaterial right)
        {
            return !left.Equals(right);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(
                Color,
                Texture,
                UVMin,
                UVMax,
                Billboard);
        }
    }
}