using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    public static class GizmoTools
    {
        public static Matrix4x4 CreateLineTransform(Vector3 start, Vector3 end)
        {
            Vector3 direction = end - start;
            float length = direction.Length();

            // Prevent division by zero if start and end are the same
            if (length < 0.0001f)
                return Matrix4x4.CreateTranslation(start);

            direction /= length;

            // Create an orthonormal basis. Assuming your line primitive points along +Z.
            // If your line primitive points along +Y, change Vector3.UnitZ below to Vector3.UnitY.
            Vector3 up = Vector3.UnitY;
            if (MathF.Abs(Vector3.Dot(direction, up)) > 0.999f)
                up = Vector3.UnitX;

            Matrix4x4 rotation = Matrix4x4.CreateWorld(Vector3.Zero, direction, up);

            // Scale by length along the forward axis, then apply rotation and translation
            return Matrix4x4.CreateScale(1, 1, length) * rotation * Matrix4x4.CreateTranslation(start);
        }

        public static Matrix4x4 CreateRotationToNormal(Vector3 normal)
        {
            Vector3 normalized = Vector3.Normalize(normal);

            // Assuming your circle primitive is drawn flat on the XZ plane (facing +Y up).
            // If it's drawn on the XY plane (facing +Z), change this to Vector3.UnitZ.
            Vector3 up = Vector3.UnitY;

            float dot = Vector3.Dot(up, normalized);

            // Handle parallel cases to avoid NaN in Cross product
            if (dot > 0.999f) return Matrix4x4.Identity;
            if (dot < -0.999f) return Matrix4x4.CreateRotationX(MathF.PI);

            Vector3 axis = Vector3.Normalize(Vector3.Cross(up, normalized));
            float angle = MathF.Acos(dot);

            return Matrix4x4.CreateFromAxisAngle(axis, angle);
        }
    }
}
