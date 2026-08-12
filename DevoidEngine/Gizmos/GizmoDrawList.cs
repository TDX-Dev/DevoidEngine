using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoDrawList
    {
        private readonly List<GizmoPrimitive> primitives = [];

        public void Line(
            Vector3 from,
            Vector3 to,
            GizmoStyle style,
            uint id = 0)
        {
            primitives.Add(
                GizmoPrimitive.Line(
                    from,
                    to,
                    style,
                    id));
        }

        public void Arrow(
            Vector3 position,
            Vector3 direction,
            float length,
            GizmoStyle style,
            uint id = 0)
        {
            primitives.Add(
                GizmoPrimitive.Arrow(
                    position,
                    direction,
                    length,
                    style,
                    id));
        }

        public void Cube(
            Matrix4x4 transform,
            Vector3 size,
            GizmoStyle style,
            uint id = 0)
        {
            primitives.Add(
                GizmoPrimitive.Cube(
                    transform,
                    size,
                    style,
                    id));
        }

        public void Sphere(
            Vector3 position,
            float radius,
            GizmoStyle style,
            uint id = 0)
        {
            primitives.Add(
                GizmoPrimitive.Sphere(
                    position,
                    radius,
                    style,
                    id));
        }

        public void Circle(
            Vector3 center,
            Vector3 normal,
            float radius,
            GizmoStyle style,
            uint id = 0)
        {
            primitives.Add(
                GizmoPrimitive.Circle(
                    center,
                    normal,
                    radius,
                    style,
                    id));
        }

        public void Mesh(
            Mesh mesh,
            Matrix4x4 transform,
            GizmoStyle style,
            uint id = 0)
        {
            primitives.Add(
                GizmoPrimitive.SetMesh(
                    mesh,
                    transform,
                    style,
                    id));
        }

        public ReadOnlySpan<GizmoPrimitive> Primitives =>
            CollectionsMarshal.AsSpan(primitives);

        public void Clear()
        {
            primitives.Clear();
        }
    }
}
