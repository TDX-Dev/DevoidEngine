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

        public static Matrix4x4 BillboardCircle(Vector3 position, float radius, Vector3 cameraForward)
        {
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(
                Matrix4x4.CreateLookAt(Vector3.Zero, -cameraForward, Vector3.UnitY)
            );

            return
                Matrix4x4.CreateScale(radius) *
                Matrix4x4.CreateFromQuaternion(rotation) *
                Matrix4x4.CreateTranslation(position);
        }

    }
}
