using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    public static class ProbeGeometrySubdivide
    {
        public static List<SurfacePoint> GetSurfacePoints(Vector3[] Vertices, uint[] Indices, float EdgeLength)
        {
            List<SurfacePoint> subdividedPoints = [];


            for (int x = 0; x < Indices.Length; x +=3)
            {
                Vector3 v1 = Vertices[Indices[x]];
                Vector3 v2 = Vertices[Indices[x+1]];
                Vector3 v3 = Vertices[Indices[x+2]];

                float v1v2 = Vector3.Distance(v1, v2);
                float v2v3 = Vector3.Distance(v2, v3);
                float v3v1 = Vector3.Distance(v3, v1);

                if (v1v2 < EdgeLength && v2v3 < EdgeLength && v3v1 < EdgeLength)
                {
                    continue;
                }

                float longestEdge = Math.Max(v1v2, Math.Max(v2v3, v3v1));
                int subdivisions = Math.Max(1, (int)Math.Ceiling(longestEdge / EdgeLength));

                Vector3 normal = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));

                subdividedPoints.Add(new SurfacePoint
                {
                    Position = v1,
                    Normal = normal
                });

                subdividedPoints.Add(new SurfacePoint
                {
                    Position = v2,
                    Normal = normal
                });

                subdividedPoints.Add(new SurfacePoint
                {
                    Position = v3,
                    Normal = normal
                });

                for (int i = 0; i <= subdivisions; i++)
                {
                    for (int j = 0; j <= subdivisions - i; j++)
                    {
                        float u = (float)i / subdivisions;
                        float v = (float)j / subdivisions;
                        float w = 1.0f - u - v;

                        Vector3 position =
                            v1 * u +
                            v2 * v +
                            v3 * w;

                        subdividedPoints.Add(new SurfacePoint()
                        {
                            Position = position,
                            Normal = normal,
                        });
                    }
                }

            }
            return subdividedPoints;
        }

    }
}
