using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public readonly struct GizmoBatchState : IEquatable<GizmoBatchState>
    {
        public readonly GizmoMaterial Material;
        public readonly PrimitiveType PrimitiveType;

        public GizmoBatchState(
            GizmoMaterial material,
            PrimitiveType primitiveType)
        {
            Material = material;
            PrimitiveType = primitiveType;
        }

        public bool Equals(GizmoBatchState other)
        {
            return Material.Equals(other.Material) &&
                   PrimitiveType == other.PrimitiveType;
        }

        public override bool Equals(object? obj)
        {
            return obj is GizmoBatchState other &&
                   Equals(other);
        }

        public static bool operator ==(
            GizmoBatchState left,
            GizmoBatchState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            GizmoBatchState left,
            GizmoBatchState right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Material,
                PrimitiveType);
        }
    }
}
