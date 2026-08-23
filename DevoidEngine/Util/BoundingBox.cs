using System.Numerics;

namespace DevoidEngine.Util
{
    public struct BoundingBox
    {
        public static BoundingBox Empty => new(Vector3.Zero, Vector3.Zero);

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

        public static BoundingBox CreateEmptyBounds()
        {
            float max = float.MaxValue;

            return new BoundingBox(
                new Vector3(max, max, max),
                new Vector3(-max, -max, -max));
        }

        public static BoundingBox Union(BoundingBox a, BoundingBox b)
        {
            return new BoundingBox(
                Vector3.Min(a.min, b.min),
                Vector3.Max(a.max, b.max));
        }

        public static float SurfaceArea(BoundingBox bounds)
        {
            Vector3 d = bounds.max - bounds.min;

            return 2.0f * (d.X * d.Y + d.X * d.Z + d.Y * d.Z);
        }
        public static bool Intersects(BoundingBox a, BoundingBox b)
        {
            return a.min.X <= b.max.X &&
                   a.max.X >= b.min.X &&
                   a.min.Y <= b.max.Y &&
                   a.max.Y >= b.min.Y &&
                   a.min.Z <= b.max.Z &&
                   a.max.Z >= b.min.Z;
        }
        public static bool IntersectsRay(BoundingBox bounds, Vector3 origin, Vector3 direction, float maxDistance)
        {
            float tMin = 0.0f;
            float tMax = maxDistance;

            for (int axis = 0; axis < 3; axis++)
            {
                float o = origin[axis];
                float d = direction[axis];
                float min = bounds.min[axis];
                float max = bounds.max[axis];

                if (MathF.Abs(d) < 1e-8f)
                {
                    if (o < min || o > max)
                        return false;

                    continue;
                }

                float invD = 1.0f / d;

                float t1 = (min - o) * invD;
                float t2 = (max - o) * invD;

                if (t1 > t2)
                    (t1, t2) = (t2, t1);

                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);

                if (tMin > tMax)
                    return false;
            }

            return true;
        }

        public static bool IntersectsRay(BoundingBox bounds, Vector3 origin, Vector3 direction, float maxDistance, out float distance)
        {
            float tMin = 0.0f;
            float tMax = maxDistance;

            for (int axis = 0; axis < 3; axis++)
            {
                float o = origin[axis];
                float d = direction[axis];
                float min = bounds.min[axis];
                float max = bounds.max[axis];

                if (MathF.Abs(d) < 1e-8f)
                {
                    if (o < min || o > max)
                    {
                        distance = 0;
                        return false;
                    }

                    continue;
                }

                float invD = 1.0f / d;

                float t1 = (min - o) * invD;
                float t2 = (max - o) * invD;

                if (t1 > t2)
                    (t1, t2) = (t2, t1);

                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);

                if (tMin > tMax)
                {
                    distance = 0;
                    return false;
                }
            }

            distance = tMin;

            return true;
        }
    }
}