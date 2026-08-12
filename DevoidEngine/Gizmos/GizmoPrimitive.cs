using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public struct GizmoPrimitive
    {
        public GizmoPrimitiveType Type;

        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Size;

        public float Radius;
        public float Length;

        public Matrix4x4 Transform;

        public Mesh? Mesh;

        public GizmoStyle Style;

        public GizmoId Id;
        public bool Interactive;

        public static GizmoPrimitive Line(
            Vector3 from,
            Vector3 to,
            GizmoStyle style,
            uint id)
        {
            return new GizmoPrimitive
            {
                Type = GizmoPrimitiveType.Line,
                Position = from,
                Direction = to - from,
                Style = style,
                Id = id
            };
        }

        public static GizmoPrimitive Arrow(
            Vector3 position,
            Vector3 direction,
            float length,
            GizmoStyle style,
            uint id)
        {
            return new GizmoPrimitive
            {
                Type = GizmoPrimitiveType.Arrow,
                Position = position,
                Direction = direction,
                Length = length,
                Style = style,
                Id = id
            };
        }

        public static GizmoPrimitive Cube(
            Matrix4x4 transform,
            Vector3 size,
            GizmoStyle style,
            uint id)
        {
            return new GizmoPrimitive
            {
                Type = GizmoPrimitiveType.Cube,
                Transform = transform,
                Size = size,
                Style = style,
                Id = id
            };
        }

        public static GizmoPrimitive Sphere(
            Vector3 position,
            float radius,
            GizmoStyle style,
            uint id)
        {
            return new GizmoPrimitive
            {
                Type = GizmoPrimitiveType.Sphere,
                Position = position,
                Radius = radius,
                Style = style,
                Id = id
            };
        }

        public static GizmoPrimitive Circle(
            Vector3 center,
            Vector3 normal,
            float radius,
            GizmoStyle style,
            uint id)
        {
            return new GizmoPrimitive
            {
                Type = GizmoPrimitiveType.Circle,
                Position = center,
                Direction = normal,
                Radius = radius,
                Style = style,
                Id = id
            };
        }

        public static GizmoPrimitive SetMesh(
            Mesh mesh,
            Matrix4x4 transform,
            GizmoStyle style,
            uint id)
        {
            return new GizmoPrimitive
            {
                Type = GizmoPrimitiveType.Mesh,
                Mesh = mesh,
                Transform = transform,
                Style = style,
                Id = id
            };
        }
    }
}
