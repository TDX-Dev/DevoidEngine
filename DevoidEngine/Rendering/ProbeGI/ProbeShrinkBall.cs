using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    public static class ProbeShrinkBall
    {
        public static MedialBall ShrinkBall(
            KDTree tree,
            SurfacePoint[] surfacePoints,
            int pointIndex,
            float initialRadius,
            float epsilon = 0.0001f,
            int maxIterations = 64)
        {
            Vector3 p = surfacePoints[pointIndex].Position;
            Vector3 n = Vector3.Normalize(surfacePoints[pointIndex].Normal);

            float r = initialRadius;
            Vector3 c = p + n * r;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                int qIndex = tree.Nearest(c, pointIndex);

                if (qIndex == pointIndex)
                {
                    break;
                }

                Vector3 q = surfacePoints[qIndex].Position;
                Vector3 d = q - p;

                float denominator = 2.0f * Vector3.Dot(n, d);

                if (denominator <= 0.0f)
                {
                    break;
                }

                float rNext = Vector3.Dot(d, d) / denominator;

                if (rNext <= 0.0f)
                {
                    break;
                }

                Vector3 cNext = p + n * rNext;

                if (MathF.Abs(rNext - r) < epsilon)
                {
                    return new MedialBall(cNext, rNext);
                }

                c = cNext;
                r = rNext;
            }

            return new MedialBall(c, r);
        }

    }
}
