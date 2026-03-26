using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public class BoundingBox
    {
        public static BoundingBox Empty => new BoundingBox(Vector3.Zero, Vector3.Zero);

        public Vector3 min;
        public Vector3 max;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public static void TransformAABB(
            Vector3 min,
            Vector3 max,
            Matrix4x4 model,
            out Vector3 worldMin,
            out Vector3 worldMax)
        {
            Vector3 center = (min + max) * 0.5f;
            Vector3 extents = (max - min) * 0.5f;

            Vector3 worldCenter = Vector3.Transform(center, model);

            Vector3 right = new Vector3(model.M11, model.M12, model.M13) * extents.X;
            Vector3 up = new Vector3(model.M21, model.M22, model.M23) * extents.Y;
            Vector3 forward = new Vector3(model.M31, model.M32, model.M33) * extents.Z;

            Vector3 worldExtents =
                new Vector3(MathF.Abs(right.X), MathF.Abs(right.Y), MathF.Abs(right.Z)) +
                new Vector3(MathF.Abs(up.X), MathF.Abs(up.Y), MathF.Abs(up.Z)) +
                new Vector3(MathF.Abs(forward.X), MathF.Abs(forward.Y), MathF.Abs(forward.Z));

            worldMin = worldCenter - worldExtents;
            worldMax = worldCenter + worldExtents;
        }

    }
}