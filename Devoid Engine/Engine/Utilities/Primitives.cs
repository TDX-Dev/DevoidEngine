using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public static class Primitives
    {
        public static Vertex[] GetCircleLineVertices(int segments = 32)
        {
            Vertex[] vertices = new Vertex[segments + 1];

            Vector3 normal = Vector3.UnitZ;

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * MathF.PI * 2f;

                Vector3 pos = new Vector3(
                    MathF.Cos(angle),
                    MathF.Sin(angle),
                    0f
                );

                vertices[i] = new Vertex(
                    pos,
                    normal,
                    new Vector2(t, 1)
                );
            }

            return vertices;
        }

        public static int[] GetCircleLineIndices(int segments = 32)
        {
            int[] indices = new int[segments * 2];

            int idx = 0;

            for (int i = 0; i < segments; i++)
            {
                indices[idx++] = i;
                indices[idx++] = i + 1;
            }

            return indices;
        }

        public static void GenerateLineCameraFrustum(
            out Vertex[] vertices,
            out int[] indices
        )
        {
            List<Vertex> verts = new();
            List<int> inds = new();

            Vector3 origin = Vector3.Zero;

            verts.Add(new Vertex(origin, Vector3.UnitZ, Vector2.Zero));
            int originIndex = 0;

            int baseStart = verts.Count;

            float z = -1.0f;

            Vector3[] corners =
            {
        new Vector3(-1, -1, z),
        new Vector3( 1, -1, z),
        new Vector3( 1,  1, z),
        new Vector3(-1,  1, z),
    };

            for (int i = 0; i < 4; i++)
                verts.Add(new Vertex(corners[i], Vector3.UnitZ, Vector2.Zero));

            // near plane rectangle
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                inds.Add(baseStart + i);
                inds.Add(baseStart + next);
            }

            // origin → corners
            for (int i = 0; i < 4; i++)
            {
                inds.Add(originIndex);
                inds.Add(baseStart + i);
            }

            // ---- Floating Camera Up Arrow ----

            int arrowStart = verts.Count;

            float zArrow = -1.05f; // slight offset to avoid z-fighting

            Vector3 arrowTip = new Vector3(0, 1.75f, zArrow);
            Vector3 arrowLeft = new Vector3(-0.75f, 1.2f, zArrow);
            Vector3 arrowRight = new Vector3(0.75f, 1.2f, zArrow);

            verts.Add(new Vertex(arrowTip, Vector3.UnitZ, Vector2.Zero));
            verts.Add(new Vertex(arrowLeft, Vector3.UnitZ, Vector2.Zero));
            verts.Add(new Vertex(arrowRight, Vector3.UnitZ, Vector2.Zero));

            int tip = arrowStart;
            int left = arrowStart + 1;
            int right = arrowStart + 2;

            // triangle outline only
            inds.Add(tip); inds.Add(left);
            inds.Add(left); inds.Add(right);
            inds.Add(right); inds.Add(tip);

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }

        public static void GenerateBillboardCone(
            int segments,
            out Vertex[] vertices,
            out int[] indices
        )
        {
            List<Vertex> verts = new();
            List<int> inds = new();

            float step = MathF.Tau / segments;

            // circle vertices
            for (int i = 0; i < segments; i++)
            {
                float angle = i * step;

                float x = MathF.Cos(angle);
                float y = MathF.Sin(angle);

                verts.Add(new Vertex(
                    new Vector3(x, y, 1f), // base at +Z
                    Vector3.UnitZ,
                    Vector2.Zero
                ));
            }

            // apex
            int apexIndex = verts.Count;

            verts.Add(new Vertex(
                Vector3.Zero,
                Vector3.UnitZ,
                Vector2.Zero
            ));

            // circle line loop
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                inds.Add(i);
                inds.Add(next);
            }

            // two cone edges
            inds.Add(apexIndex);
            inds.Add(0);

            inds.Add(apexIndex);
            inds.Add(segments / 2);

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }
        public static void GenerateLineCone(
            int segments,
            out Vertex[] vertices,
            out int[] indices)
        {
            List<Vertex> verts = new();
            List<int> inds = new();

            float radius = 1.0f;
            float height = 1.0f;

            Vector3 tip = Vector3.Zero;

            // tip vertex
            verts.Add(new Vertex(tip, Vector3.UnitZ, Vector2.Zero));
            int tipIndex = 0;

            int baseStart = verts.Count;

            // generate base circle
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / segments);

                float x = MathF.Cos(angle) * radius;
                float y = MathF.Sin(angle) * radius;

                Vector3 pos = new Vector3(x, y, -height);

                Vector3 normal = Vector3.Normalize(new Vector3(x, y, 0));

                verts.Add(new Vertex(pos, normal, new Vector2((float)i / segments, 1)));
            }

            // base circle lines
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                inds.Add(baseStart + i);
                inds.Add(baseStart + next);
            }

            // side lines (tip -> base)
            for (int i = 0; i < segments; i++)
            {
                inds.Add(tipIndex);
                inds.Add(baseStart + i);
            }

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }
        public static void GenerateConeBack(
            int segments,
            out Vertex[] vertices,
            out int[] indices
        )
        {
            List<Vertex> verts = new();
            List<int> inds = new();

            float radius = 1.0f;
            float height = 1.0f;

            Vector3 tip = new Vector3(0, 0, 0);
            Vector3 baseCenter = new Vector3(0, 0, -height);

            verts.Add(new Vertex(tip, Vector3.UnitZ, Vector2.Zero));
            int tipIndex = 0;

            verts.Add(new Vertex(baseCenter, -Vector3.UnitZ, Vector2.Zero));

            int baseStart = verts.Count;

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / segments);

                float x = MathF.Cos(angle) * radius;
                float y = MathF.Sin(angle) * radius;

                Vector3 pos = new Vector3(x, y, -height);

                Vector3 normal = Vector3.Normalize(new Vector3(x, y, -radius));

                verts.Add(new Vertex(pos, normal, new Vector2((float)i / segments, 1)));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                inds.Add(tipIndex);
                inds.Add(baseStart + next);
                inds.Add(baseStart + i);
            }

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }

        public static void GenerateCone(
            int segments,
            out Vertex[] vertices,
            out int[] indices)
        {
            List<Vertex> verts = new();
            List<int> inds = new();

            float radius = 1.0f;
            float height = 1.0f;

            Vector3 tip = new Vector3(0, 0, 0);
            Vector3 baseCenter = new Vector3(0, 0, -height);

            verts.Add(new Vertex(tip, Vector3.UnitZ, Vector2.Zero));
            int tipIndex = 0;

            verts.Add(new Vertex(baseCenter, -Vector3.UnitZ, Vector2.Zero));

            int baseStart = verts.Count;

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / segments);

                float x = MathF.Cos(angle) * radius;
                float y = MathF.Sin(angle) * radius;

                Vector3 pos = new Vector3(x, y, -height);

                Vector3 normal = Vector3.Normalize(new Vector3(x, y, -radius));

                verts.Add(new Vertex(pos, normal, new Vector2((float)i / segments, 1)));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                // flipped winding
                inds.Add(tipIndex);
                inds.Add(baseStart + i);
                inds.Add(baseStart + next);
            }

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }

        public static Vertex[] GetSphereVertices(int latSegments = 32, int lonSegments = 32, float radius = 0.5f)
        {
            List<Vertex> vertices = new List<Vertex>();

            for (int lat = 0; lat < latSegments; lat++)
            {
                float theta0 = MathF.PI * lat / latSegments;
                float theta1 = MathF.PI * (lat + 1) / latSegments;

                for (int lon = 0; lon < lonSegments; lon++)
                {
                    float phi0 = 2 * MathF.PI * lon / lonSegments;
                    float phi1 = 2 * MathF.PI * (lon + 1) / lonSegments;

                    Vector3 p00 = Sphere(theta0, phi0) * radius;
                    Vector3 p10 = Sphere(theta1, phi0) * radius;
                    Vector3 p01 = Sphere(theta0, phi1) * radius;
                    Vector3 p11 = Sphere(theta1, phi1) * radius;

                    Vector3 n00 = Vector3.Normalize(p00);
                    Vector3 n10 = Vector3.Normalize(p10);
                    Vector3 n01 = Vector3.Normalize(p01);
                    Vector3 n11 = Vector3.Normalize(p11);

                    Vector2 uv00 = new Vector2(phi0 / (2 * MathF.PI), theta0 / MathF.PI);
                    Vector2 uv10 = new Vector2(phi0 / (2 * MathF.PI), theta1 / MathF.PI);
                    Vector2 uv01 = new Vector2(phi1 / (2 * MathF.PI), theta0 / MathF.PI);
                    Vector2 uv11 = new Vector2(phi1 / (2 * MathF.PI), theta1 / MathF.PI);

                    // Triangle 1
                    vertices.Add(new Vertex(p00, n00, uv00));
                    vertices.Add(new Vertex(p10, n10, uv10));
                    vertices.Add(new Vertex(p11, n11, uv11));

                    // Triangle 2
                    vertices.Add(new Vertex(p00, n00, uv00));
                    vertices.Add(new Vertex(p11, n11, uv11));
                    vertices.Add(new Vertex(p01, n01, uv01));
                }
            }

            return vertices.ToArray();
        }

        private static Vector3 Sphere(float theta, float phi)
        {
            float x = MathF.Sin(theta) * MathF.Cos(phi);
            float y = MathF.Cos(theta);
            float z = MathF.Sin(theta) * MathF.Sin(phi);
            return new Vector3(x, y, z);
        }

        public static Vertex[] GetScreenQuadVertex()
        {
            return new Vertex[]
            {
        // First triangle
        new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 1)),
        new Vertex(new Vector3( 1.0f, -1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 1)),
        new Vertex(new Vector3( 1.0f,  1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 0)),

        // Second triangle
        new Vertex(new Vector3( 1.0f,  1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 0)),
        new Vertex(new Vector3(-1.0f,  1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 0)),
        new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 1)),
            };
        }


        public static Vertex[] GetQuadVertex()
        {
            return new Vertex[]
            {
        // First triangle
        new Vertex(new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ, new Vector2(0, 0)),
        new Vertex(new Vector3(1.0f, 0.0f, 0.0f), Vector3.UnitZ, new Vector2(1, 0)),
        new Vertex(new Vector3(1.0f, 1.0f, 0.0f), Vector3.UnitZ, new Vector2(1, 1)),

        // Second triangle
        new Vertex(new Vector3(1.0f, 1.0f, 0.0f), Vector3.UnitZ, new Vector2(1, 1)),
        new Vertex(new Vector3(0.0f, 1.0f, 0.0f), Vector3.UnitZ, new Vector2(0, 1)),
        new Vertex(new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ, new Vector2(0, 0)),
            };
        }


        public static Vertex[] GetCubeVertex()
        {
            return new Vertex[]
            {
        // Front face (+Z)
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(1, 0)),
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(1, 1)),

        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(0, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(0, 0)),

        // Back face (-Z)
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0, 0)),
        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1, 1)),

        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0, 0)),

        // Left face (-X)
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-1, 0, 0), new Vector2(1, 0)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0, 0)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1, 0, 0), new Vector2(1, 1)),

        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1, 0, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0, 0)),

        // Right face (+X)
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(1, 0, 0), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1, 1)),

        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(1, 0, 0), new Vector2(0, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(1, 0, 0), new Vector2(0, 0)),

        // Bottom face (-Y)
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(1, 0)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, -1, 0), new Vector2(1, 1)),

        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, -1, 0), new Vector2(0, 1)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, -1, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(0, 0)),

        // Top face (+Y)
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0, 1, 0), new Vector2(1, 0)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0, 1, 0), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(1, 1)),

        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(0, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0, 1, 0), new Vector2(0, 0)),
            };
        }

        public static Vertex[] GetCubeLineVertices()
        {
            return new Vertex[]
            {
                // Front (+Z)
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f)), // 0
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f)), // 1
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f)), // 2
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f)), // 3

                // Back (-Z)
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f)), // 4
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f)), // 5
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f)), // 6
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f)), // 7
            };
        }

        public static int[] GetCubeLineIndices()
        {
            return new int[]
            {
                // Front square
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                // Back square
                4, 5,
                5, 6,
                6, 7,
                7, 4,

                // Connections
                0, 4,
                1, 5,
                2, 6,
                3, 7
            };
        }

        public static Vertex[] GetQuadLineVertices()
        {
            return new Vertex[]
            {
                new Vertex(new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ, new Vector2(0, 0)), // 0 bottom-left
                new Vertex(new Vector3(1.0f, 0.0f, 0.0f), Vector3.UnitZ, new Vector2(1, 0)), // 1 bottom-right
                new Vertex(new Vector3(1.0f, 1.0f, 0.0f), Vector3.UnitZ, new Vector2(1, 1)), // 2 top-right
                new Vertex(new Vector3(0.0f, 1.0f, 0.0f), Vector3.UnitZ, new Vector2(0, 1)), // 3 top-left
            };
        }
        public static int[] GetQuadLineIndices()
        {
            return new int[]
            {
                0, 1, // bottom
                1, 2, // right
                2, 3, // top
                3, 0  // left
            };
        }

    }
}