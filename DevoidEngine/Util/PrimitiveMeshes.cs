using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Util
{
    public static class PrimitiveMeshes
    {
        private static Mesh? quad;
        private static Mesh? cube;
        private static Mesh? fullscreenMesh;
        private static Mesh? uvsphereMesh;

        public static Mesh GetQuad()
        {
            quad ??= CreateQuad();
            return quad;
        }
        public static Mesh GetCube()
        {
            cube ??= CreateCube();
            return cube;
        }
        public static Mesh GetFullscreenPlane()
        {
            fullscreenMesh ??= CreateFullscreenTriangle();
            return fullscreenMesh;
        }
        public static Mesh GetUVSphere()
        {
            uvsphereMesh ??= CreateSphere();
            return uvsphereMesh;
        }
        public static Mesh CreateCube()
        {
            Mesh mesh = new()
            {
                Positions =
                [
                // Front
                    new(-0.5f, -0.5f,  0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new( 0.5f,  0.5f,  0.5f),
                new(-0.5f,  0.5f,  0.5f),

            // Back
                new( 0.5f, -0.5f, -0.5f),
                new(-0.5f, -0.5f, -0.5f),
                new(-0.5f,  0.5f, -0.5f),
                new( 0.5f,  0.5f, -0.5f),

            // Left
                new(-0.5f, -0.5f, -0.5f),
                new(-0.5f, -0.5f,  0.5f),
                new(-0.5f,  0.5f,  0.5f),
                new(-0.5f,  0.5f, -0.5f),

            // Right
                new( 0.5f, -0.5f,  0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f,  0.5f, -0.5f),
                new( 0.5f,  0.5f,  0.5f),

            // Top
                new(-0.5f,  0.5f,  0.5f),
                new( 0.5f,  0.5f,  0.5f),
                new( 0.5f,  0.5f, -0.5f),
                new(-0.5f,  0.5f, -0.5f),

            // Bottom
                new(-0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new(-0.5f, -0.5f,  0.5f),
            ],

                Normals =
                [
                // Front
                    Vector3.UnitZ,
                    Vector3.UnitZ,
                    Vector3.UnitZ,
                    Vector3.UnitZ,

            // Back
                    -Vector3.UnitZ,
                    -Vector3.UnitZ,
                    -Vector3.UnitZ,
                    -Vector3.UnitZ,

            // Left
                    -Vector3.UnitX,
                    -Vector3.UnitX,
                    -Vector3.UnitX,
                    -Vector3.UnitX,

            // Right
                    Vector3.UnitX,
                    Vector3.UnitX,
                    Vector3.UnitX,
                    Vector3.UnitX,

            // Top
                    Vector3.UnitY,
                    Vector3.UnitY,
                    Vector3.UnitY,
                    Vector3.UnitY,

            // Bottom
                    -Vector3.UnitY,
                    -Vector3.UnitY,
                    -Vector3.UnitY,
                    -Vector3.UnitY
                ],

                UVs =
                [
                // Front
                    new(0, 1),
                    new(1, 1),
                    new(1, 0),
                    new(0, 0),

            // Back
                    new(0, 1),
                    new(1, 1),
                    new(1, 0),
                    new(0, 0),

            // Left
                    new(0, 1),
                    new(1, 1),
                    new(1, 0),
                    new(0, 0),

            // Right
                    new(0, 1),
                    new(1, 1),
                    new(1, 0),
                    new(0, 0),

            // Top
                    new(0, 1),
                    new(1, 1),
                    new(1, 0),
                    new(0, 0),

            // Bottom
                    new(0, 1),
                    new(1, 1),
                    new(1, 0),
                    new(0, 0),
            ],
                Indices =
            [
                // Front
                0, 2, 1,
                2, 0, 3,

                // Back
                4, 6, 5,
                6, 4, 7,

                // Left
                8, 10, 9,
                10, 8, 11,

                // Right
                12, 14, 13,
                14, 12, 15,

                // Top
                16, 18, 17,
                18, 16, 19,

                // Bottom
                20, 22, 21,
                22, 20, 23
            ]
            };

            mesh.Upload();

            return mesh;
        }
        public static Mesh CreateFullscreenTriangle()
        {
            Mesh mesh = new()
            {
                Positions =
                [
                    new Vector3(-1.0f, -1.0f, 0.0f),
            new Vector3(-1.0f,  3.0f, 0.0f),
            new Vector3( 3.0f, -1.0f, 0.0f)
                ],

                UVs =
                [
                    new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, -1.0f),
            new Vector2(2.0f, 1.0f)
                ],

                Normals =
                [
                    Vector3.UnitZ,
            Vector3.UnitZ,
            Vector3.UnitZ
                ]
            };

            mesh.Upload();

            return mesh;
        }

        public static Mesh CreateQuad()
        {
            Mesh mesh = new()
            {
                Positions =
                [
                    new Vector3(0.0f, 0.0f, 0.0f), // Bottom Left
            new Vector3(1.0f, 0.0f, 0.0f), // Bottom Right
            new Vector3(1.0f, 1.0f, 0.0f), // Top Right
            new Vector3(0.0f, 1.0f, 0.0f), // Top Left
        ],

                UVs =
                [
                    new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
        ],

                Normals =
                [
                    Vector3.UnitZ,
            Vector3.UnitZ,
            Vector3.UnitZ,
            Vector3.UnitZ
                ],

                Indices =
                [
                    0, 1, 2,
                    2, 3, 0
                ]
            };

            mesh.Upload();

            return mesh;
        }

        public static Mesh CreateSphere(float radius = 0.5f, int slices = 64, int stacks = 32)
        {
            Mesh mesh = new();

            List<Vector3> positions = [];
            List<Vector3> normals = [];
            List<Vector2> uvs = [];
            List<uint> indices = [];

            for (int stack = 0; stack <= stacks; stack++)
            {
                float v = (float)stack / stacks;
                float phi = MathF.PI * v;

                float y = MathF.Cos(phi);
                float r = MathF.Sin(phi);

                for (int slice = 0; slice <= slices; slice++)
                {
                    float u = (float)slice / slices;
                    float theta = u * MathF.PI * 2.0f;

                    float x = r * MathF.Cos(theta);
                    float z = r * MathF.Sin(theta);

                    Vector3 normal = new(x, y, z);

                    positions.Add(normal * radius);
                    normals.Add(normal);
                    uvs.Add(new Vector2(u, 1.0f - v));
                }
            }

            int stride = slices + 1;

            for (int stack = 0; stack < stacks; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    uint a = (uint)(stack * stride + slice);
                    uint b = (uint)((stack + 1) * stride + slice);
                    uint c = (uint)(a + 1);
                    uint d = (uint)(b + 1);

                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);

                    indices.Add(c);
                    indices.Add(b);
                    indices.Add(d);
                }
            }

            mesh.Positions = [.. positions];
            mesh.Normals = [.. normals];
            mesh.UVs = [.. uvs];
            mesh.Indices = [.. indices];

            mesh.Upload();

            return mesh;
        }
    }
}
