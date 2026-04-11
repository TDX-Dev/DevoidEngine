using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Utilities
{
    public static class PrimitiveHelper
    {
        public static Matrix4x4 GetSpotlightModel(GPUSpotLight light)
        {
            float range = light.direction.W;

            float angle = light.outerCutoff; // already radians

            float radius = range * MathF.Tan(angle);

            Matrix4x4 scale = Matrix4x4.CreateScale(radius, radius, range);

            Vector3 dir = Vector3.Normalize(light.direction.AsVector3());

            Vector3 up = Vector3.UnitY;

            if (MathF.Abs(Vector3.Dot(dir, up)) > 0.999f)
                up = Vector3.UnitX;

            Matrix4x4 rotation = Matrix4x4.CreateWorld(Vector3.Zero, dir, up);

            Matrix4x4 translation =
                Matrix4x4.CreateTranslation(light.position.AsVector3());

            return scale * rotation * translation;
        }

    }
}
