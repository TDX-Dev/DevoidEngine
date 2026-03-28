using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public static class Primitives
    {
        public static Mesh Cube
        {
            get
            {
                Mesh cubeMesh = new Mesh();
                cubeMesh.SetVertices(GetCubeVertex());
                return cubeMesh;
            }
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