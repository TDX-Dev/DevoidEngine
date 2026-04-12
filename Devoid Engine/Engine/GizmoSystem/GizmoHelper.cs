using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.GizmoSystem
{
    public static class GizmoHelper
    {
        public static Matrix4x4 GetCameraFrustumModel(
            Vector3 position,
            Vector3 forward,
            float fovRadians,
            float width,
            float height,
            float distance
        )
        {
            float aspect = width / height;

            float halfHeight = MathF.Tan(fovRadians * 0.5f) * distance;
            float halfWidth = halfHeight * aspect;

            Matrix4x4 scale = Matrix4x4.CreateScale(halfWidth, halfHeight, distance);

            Vector3 dir = Vector3.Normalize(forward);

            Vector3 up = Vector3.UnitY;

            if (MathF.Abs(Vector3.Dot(dir, up)) > 0.999f)
                up = Vector3.UnitX;

            Matrix4x4 rotation = Matrix4x4.CreateWorld(Vector3.Zero, dir, up);

            Matrix4x4 translation = Matrix4x4.CreateTranslation(position);

            return scale * rotation * translation;
        }

        public static Matrix4x4 GetSpotlightModel(
            Vector3 position,
            Vector3 direction,
            float range,
            float outerAngleRadians)
        {
            float radius = range * MathF.Tan(outerAngleRadians);

            Matrix4x4 scale = Matrix4x4.CreateScale(radius, radius, range);

            Vector3 dir = Vector3.Normalize(direction);

            Vector3 up = Vector3.UnitY;

            if (MathF.Abs(Vector3.Dot(dir, up)) > 0.999f)
                up = Vector3.UnitX;

            Matrix4x4 rotation = Matrix4x4.CreateWorld(Vector3.Zero, dir, up);

            Matrix4x4 translation = Matrix4x4.CreateTranslation(position);

            return scale * rotation * translation;
        }

        public static Matrix4x4 GetSpotlightModelBillboard(
            Vector3 position,
            Vector3 direction,
            Vector3 cameraPosition,
            float range,
            float outerAngleRadians
        )
        {
            direction = Vector3.Normalize(direction);

            float radius = range * MathF.Tan(outerAngleRadians);

            // Vector from light to camera
            Vector3 toCamera = Vector3.Normalize(cameraPosition - position);

            // Project onto plane perpendicular to the spotlight direction
            Vector3 right = toCamera - direction * Vector3.Dot(toCamera, direction);
            right = Vector3.Normalize(right);

            // Build orthonormal basis
            Vector3 up = Vector3.Normalize(Vector3.Cross(direction, right));

            Matrix4x4 rotation = new Matrix4x4(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                direction.X, direction.Y, direction.Z, 0,
                0, 0, 0, 1
            );

            Matrix4x4 scale = Matrix4x4.CreateScale(radius, radius, range);

            // Rotate 90° around the cone's local forward axis
            Matrix4x4 localRotation = Matrix4x4.CreateRotationZ(MathF.PI * 0.5f);

            return localRotation * scale * rotation * Matrix4x4.CreateTranslation(position);
        }

        public static Matrix4x4 BillboardCone(
            Vector3 position,
            Vector3 axis,
            float scale,
            Vector3 cameraPosition
        )
        {
            axis = Vector3.Normalize(axis);

            Vector3 toCamera = Vector3.Normalize(cameraPosition - position);

            // project onto plane perpendicular to axis
            Vector3 right = toCamera - axis * Vector3.Dot(toCamera, axis);

            if (right.LengthSquared() < 0.0001f)
                right = Vector3.UnitX; // fallback

            right = Vector3.Normalize(right);

            Vector3 up = Vector3.Normalize(Vector3.Cross(axis, right));

            Matrix4x4 rotation = new Matrix4x4(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                axis.X, axis.Y, axis.Z, 0,
                0, 0, 0, 1
            );

            return
                Matrix4x4.CreateScale(scale) *
                rotation *
                Matrix4x4.CreateTranslation(position);
        }

        public static Matrix4x4 BillboardCircle(Vector3 position, float radius, Vector3 cameraPosition)
        {
            Vector3 forward = Vector3.Normalize(position - cameraPosition);

            Matrix4x4 rotationMatrix = Matrix4x4.CreateWorld(
                Vector3.Zero,
                forward,
                Vector3.UnitY
            );

            return
                Matrix4x4.CreateScale(radius) *
                rotationMatrix *
                Matrix4x4.CreateTranslation(position);
        }

    }
}
