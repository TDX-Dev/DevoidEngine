using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public readonly struct GizmoId : IEquatable<GizmoId>
    {
        public readonly uint Value;

        public GizmoId(uint value)
        {
            Value = value;
        }

        public bool IsNone => Value == 0;

        public static GizmoId None => default;

        public bool Equals(GizmoId other)
            => Value == other.Value;

        public override bool Equals(object? obj)
            => obj is GizmoId other && Equals(other);

        public override int GetHashCode()
            => (int)Value;

        public static implicit operator GizmoId(uint value)
            => new(value);
        public static bool operator ==(GizmoId left, GizmoId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GizmoId left, GizmoId right)
        {
            return !(left == right);
        }
    }
}
