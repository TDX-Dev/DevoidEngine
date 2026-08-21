using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    public static class ProbeSurfacePointMerge
    {
        public static List<SurfacePoint> MergeClosePoints(IReadOnlyList<SurfacePoint> points, float distance)
        {
            float inverseDistance = 1.0f / distance;

            Dictionary<(int X, int Y, int Z), SurfacePoint> grid = new(points.Count);
            List<SurfacePoint> result = [];

            for (int i = 0; i < points.Count; i++)
            {
                SurfacePoint point = points[i];

                int x = (int)MathF.Floor(point.Position.X * inverseDistance);
                int y = (int)MathF.Floor(point.Position.Y * inverseDistance);
                int z = (int)MathF.Floor(point.Position.Z * inverseDistance);

                if (grid.ContainsKey((x, y, z)))
                    continue;

                grid.Add((x, y, z), point);
                result.Add(point);
            }

            return result;
        }

        public static List<MedialBall> ReduceCandidates(IReadOnlyList<MedialBall> candidates, float spacing)
        {
            Dictionary<(int X, int Y, int Z), int> grid = [];
            List<MedialBall> result = [];

            float inverseSpacing = 1.0f / spacing;

            for (int i = 0; i < candidates.Count; i++)
            {
                MedialBall candidate = candidates[i];

                int x = (int)MathF.Floor(candidate.Center.X * inverseSpacing);
                int y = (int)MathF.Floor(candidate.Center.Y * inverseSpacing);
                int z = (int)MathF.Floor(candidate.Center.Z * inverseSpacing);

                var cell = (x, y, z);

                if (!grid.TryGetValue(cell, out int existingIndex))
                {
                    grid.Add(cell, result.Count);
                    result.Add(candidate);
                    continue;
                }

                if (candidate.Radius > result[existingIndex].Radius)
                    result[existingIndex] = candidate;
            }

            return result;
        }
    }
}
