using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Util
{
    public static class PrimitiveMeshes
    {
        private static Mesh? quad;
        private static Mesh? centeredQuad;
        private static Mesh? centeredCircle;
        private static Mesh? cube;
        private static Mesh? wireCube;
        private static Mesh? invCube;
        private static Mesh? fullscreenMesh;
        private static Mesh? uvsphereMesh;
        private static Mesh? cylinderMesh;
        private static Mesh? icoSphere;

        public static Mesh GetQuad()
        {
            quad ??= CreateQuad();
            return quad;
        }

        public static Mesh GetCenteredQuad()
        {
            centeredQuad ??= CreateCenteredQuad();
            return centeredQuad;
        }

        public static Mesh GetCenteredCircle()
        {
            centeredCircle ??= CreateCircle();
            return centeredCircle;
        }

        public static Mesh GetCube()
        {
            cube ??= CreateCube();
            return cube;
        }

        public static Mesh GetWireCube()
        {
            wireCube ??= CreateWireCube();
            return wireCube;
        }

        public static Mesh GetInvertedUVCube()
        {
            invCube ??= CreateInvertedUVCube();
            return invCube;
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

        public static Mesh GetIcoSphere()
        {
            icoSphere ??= CreateIcosphere();
            return icoSphere;
        }

        public static Mesh GetCylinder()
        {
            cylinderMesh ??= CreateCylinder();
            return cylinderMesh;
        }

        private static Mesh CreateMesh(
            Vector3[] positions,
            Vector3[]? normals = null,
            Vector2[]? uvs = null,
            Vector4[]? tangents = null,
            uint[]? indices = null)
        {
            Mesh mesh = new();
            MeshSurface surface = new(
                positions,
                normals,
                uvs,
                tangents,
                indices
            );

            mesh.Surfaces = [surface];
            mesh.Upload();

            return Engine.Instance.AssetManager.RegisterPersistentResource(mesh);
        }

        public static Mesh CreateCube()
        {
            Vector3[] positions =
            [
                // Front
                new(-0.5f, -0.5f, 0.5f),
                new( 0.5f, -0.5f, 0.5f),
                new( 0.5f,  0.5f, 0.5f),
                new(-0.5f,  0.5f, 0.5f),

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
                new(0.5f, -0.5f,  0.5f),
                new(0.5f, -0.5f, -0.5f),
                new(0.5f,  0.5f, -0.5f),
                new(0.5f,  0.5f,  0.5f),

                // Top
                new(-0.5f, 0.5f,  0.5f),
                new( 0.5f, 0.5f,  0.5f),
                new( 0.5f, 0.5f, -0.5f),
                new(-0.5f, 0.5f, -0.5f),

                // Bottom
                new(-0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new(-0.5f, -0.5f,  0.5f)
            ];

            Vector3[] normals =
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
            ];

            Vector2[] uvs =
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
                new(0, 0)
            ];

            uint[] indices =
            [
                // Front
                0, 1, 2,
                2, 3, 0,

                // Back
                4, 5, 6,
                6, 7, 4,

                // Left
                8, 9, 10,
                10, 11, 8,

                // Right
                12, 13, 14,
                14, 15, 12,

                // Top
                16, 17, 18,
                18, 19, 16,

                // Bottom
                20, 21, 22,
                22, 23, 20
            ];

            return CreateMesh(positions, normals, uvs, indices: indices);
        }

        public static Mesh CreateWireCube()
        {
            Vector3[] positions =
            [
                // Front corners
                new(-0.5f, -0.5f,  0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new( 0.5f,  0.5f,  0.5f),
                new(-0.5f,  0.5f,  0.5f),

                // Back corners
                new(-0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f,  0.5f, -0.5f),
                new(-0.5f,  0.5f, -0.5f)
            ];

            Vector3[] normals =
            [
                Vector3.Zero,
                Vector3.Zero,
                Vector3.Zero,
                Vector3.Zero,
                Vector3.Zero,
                Vector3.Zero,
                Vector3.Zero,
                Vector3.Zero
            ];

            Vector2[] uvs =
            [
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero
            ];

            uint[] indices =
            [
                // Front face loop
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                // Back face loop
                4, 5,
                5, 6,
                6, 7,
                7, 4,

                // Connecting edges
                0, 4,
                1, 5,
                2, 6,
                3, 7
            ];

            return CreateMesh(positions, normals, uvs, indices: indices);
        }

        public static Mesh CreateInvertedUVCube()
        {
            Vector3[] positions =
            [
                // Front
                new(-0.5f, -0.5f, 0.5f),
                new( 0.5f, -0.5f, 0.5f),
                new( 0.5f,  0.5f, 0.5f),
                new(-0.5f,  0.5f, 0.5f),

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
                new(0.5f, -0.5f,  0.5f),
                new(0.5f, -0.5f, -0.5f),
                new(0.5f,  0.5f, -0.5f),
                new(0.5f,  0.5f,  0.5f),

                // Top
                new(-0.5f, 0.5f,  0.5f),
                new( 0.5f, 0.5f,  0.5f),
                new( 0.5f, 0.5f, -0.5f),
                new(-0.5f, 0.5f, -0.5f),

                // Bottom
                new(-0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new(-0.5f, -0.5f,  0.5f)
            ];

            Vector3[] normals =
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
            ];

            Vector2[] uvs =
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
                new(0, 0)
            ];

            uint[] indices =
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
            ];

            return CreateMesh(positions, normals, uvs, indices: indices);
        }

        public static Mesh CreateFullscreenTriangle()
        {
            Vector3[] positions =
            [
                new(-1.0f, -1.0f, 0.0f),
                new(-1.0f,  3.0f, 0.0f),
                new( 3.0f, -1.0f, 0.0f)
            ];

            Vector2[] uvs =
            [
                new(0.0f,  1.0f),
                new(0.0f, -1.0f),
                new(2.0f,  1.0f)
            ];

            Vector3[] normals =
            [
                Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitZ
            ];

            return CreateMesh(positions, normals, uvs);
        }

        public static Mesh CreateQuad()
        {
            Vector3[] positions =
            [
                new(0.0f, 0.0f, 0.0f),
                new(1.0f, 0.0f, 0.0f),
                new(1.0f, 1.0f, 0.0f),
                new(0.0f, 1.0f, 0.0f)
            ];

            Vector2[] uvs =
            [
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            ];

            Vector3[] normals =
            [
                Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitZ
            ];

            uint[] indices =
            [
                0, 1, 2,
                2, 3, 0
            ];

            return CreateMesh(positions, normals, uvs, indices: indices);
        }

        public static Mesh CreateCenteredQuad()
        {
            Vector3[] positions =
            [
                new(-0.5f, -0.5f, 0),
                new( 0.5f, -0.5f, 0),
                new( 0.5f,  0.5f, 0),
                new(-0.5f,  0.5f, 0)
            ];

            Vector2[] uvs =
            [
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            ];

            Vector3[] normals =
            [
                Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitZ
            ];

            uint[] indices =
            [
                0, 1, 2,
                2, 3, 0
            ];

            return CreateMesh(positions, normals, uvs, indices: indices);
        }

        public static Mesh CreateSphere(
            float radius = 0.5f,
            int slices = 64,
            int stacks = 32)
        {
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
                    uint c = a + 1;
                    uint d = b + 1;

                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);

                    indices.Add(c);
                    indices.Add(b);
                    indices.Add(d);
                }
            }

            return CreateMesh(
                [.. positions],
                [.. normals],
                [.. uvs],
                indices: [.. indices]
            );
        }

        public static Mesh CreateCapsule(
            float radius = 0.5f,
            float height = 2.0f,
            int slices = 32,
            int hemisphereStacks = 8,
            int cylinderStacks = 1)
        {
            List<Vector3> positions = [];
            List<Vector3> normals = [];
            List<Vector2> uvs = [];
            List<uint> indices = [];

            float cylinderHeight = MathF.Max(
                0.0f,
                height - radius * 2.0f
            );

            void AddRing(
                float y,
                float ringRadius,
                Vector3 sphereCenter,
                float v,
                bool cylinder)
            {
                for (int slice = 0; slice <= slices; slice++)
                {
                    float u = (float)slice / slices;
                    float theta = u * MathF.PI * 2.0f;

                    float x = MathF.Cos(theta) * ringRadius;
                    float z = MathF.Sin(theta) * ringRadius;

                    positions.Add(new Vector3(x, y, z));

                    Vector3 normal;

                    if (cylinder)
                    {
                        normal = Vector3.Normalize(
                            new Vector3(x, 0, z)
                        );
                    }
                    else
                    {
                        normal = Vector3.Normalize(
                            new Vector3(x, y, z) - sphereCenter
                        );
                    }

                    normals.Add(normal);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int stack = 1; stack < hemisphereStacks; stack++)
            {
                float t = (float)stack / hemisphereStacks;
                float phi = t * MathF.PI * 0.5f;
                float ringRadius = MathF.Sin(phi) * radius;
                float y = cylinderHeight * 0.5f + MathF.Cos(phi) * radius;

                AddRing(
                    y,
                    ringRadius,
                    new Vector3(0, cylinderHeight * 0.5f, 0),
                    t * 0.25f,
                    false
                );
            }

            for (int i = 0; i <= cylinderStacks; i++)
            {
                float t = (float)i / cylinderStacks;
                float y = cylinderHeight * 0.5f - t * cylinderHeight;

                AddRing(
                    y,
                    radius,
                    Vector3.Zero,
                    0.25f + t * 0.5f,
                    true
                );
            }

            for (int stack = 1; stack < hemisphereStacks; stack++)
            {
                float t = (float)stack / hemisphereStacks;
                float phi = t * MathF.PI * 0.5f;
                float ringRadius = MathF.Cos(phi) * radius;
                float y = -cylinderHeight * 0.5f - MathF.Sin(phi) * radius;

                AddRing(
                    y,
                    ringRadius,
                    new Vector3(0, -cylinderHeight * 0.5f, 0),
                    0.75f + t * 0.25f,
                    false
                );
            }

            int vertsPerRing = slices + 1;
            int ringCount =
                (hemisphereStacks - 1) +
                (cylinderStacks + 1) +
                (hemisphereStacks - 1);

            int topPole = positions.Count;

            positions.Add(
                new Vector3(
                    0,
                    cylinderHeight * 0.5f + radius,
                    0
                )
            );

            normals.Add(Vector3.UnitY);
            uvs.Add(new Vector2(0.5f, 0));

            int bottomPole = positions.Count;

            positions.Add(
                new Vector3(
                    0,
                    -cylinderHeight * 0.5f - radius,
                    0
                )
            );

            normals.Add(-Vector3.UnitY);
            uvs.Add(new Vector2(0.5f, 1));

            for (int i = 0; i < slices; i++)
            {
                indices.Add((uint)topPole);
                indices.Add((uint)i);
                indices.Add((uint)(i + 1));
            }

            for (int ring = 0; ring < ringCount - 1; ring++)
            {
                int row0 = ring * vertsPerRing;
                int row1 = row0 + vertsPerRing;

                for (int i = 0; i < slices; i++)
                {
                    uint a = (uint)(row0 + i);
                    uint b = (uint)(row0 + i + 1);
                    uint c = (uint)(row1 + i);
                    uint d = (uint)(row1 + i + 1);

                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);

                    indices.Add(b);
                    indices.Add(d);
                    indices.Add(c);
                }
            }

            int lastRing = (ringCount - 1) * vertsPerRing;

            for (int i = 0; i < slices; i++)
            {
                indices.Add((uint)(lastRing + i));
                indices.Add((uint)bottomPole);
                indices.Add((uint)(lastRing + i + 1));
            }

            return CreateMesh(
                [.. positions],
                [.. normals],
                [.. uvs],
                indices: [.. indices]
            );
        }

        public static Mesh CreateCylinder(
            float radius = 0.5f,
            float height = 1.0f,
            int slices = 32,
            int stacks = 1)
        {
            List<Vector3> positions = [];
            List<Vector3> normals = [];
            List<Vector2> uvs = [];
            List<uint> indices = [];

            float halfHeight = height * 0.5f;

            for (int stack = 0; stack <= stacks; stack++)
            {
                float v = (float)stack / stacks;
                float y = -halfHeight + v * height;

                for (int slice = 0; slice <= slices; slice++)
                {
                    float u = (float)slice / slices;
                    float theta = u * MathF.PI * 2.0f;

                    float x = MathF.Cos(theta) * radius;
                    float z = MathF.Sin(theta) * radius;

                    positions.Add(new Vector3(x, y, z));

                    normals.Add(
                        Vector3.Normalize(
                            new Vector3(x, 0.0f, z)
                        )
                    );

                    uvs.Add(new Vector2(u, 1.0f - v));
                }
            }

            int vertsPerRing = slices + 1;

            for (int stack = 0; stack < stacks; stack++)
            {
                int row0 = stack * vertsPerRing;
                int row1 = row0 + vertsPerRing;

                for (int slice = 0; slice < slices; slice++)
                {
                    uint a = (uint)(row0 + slice);
                    uint b = (uint)(row0 + slice + 1);
                    uint c = (uint)(row1 + slice);
                    uint d = (uint)(row1 + slice + 1);

                    indices.Add(a);
                    indices.Add(c);
                    indices.Add(b);

                    indices.Add(b);
                    indices.Add(c);
                    indices.Add(d);
                }
            }

            int topCenter = positions.Count;

            positions.Add(new Vector3(0, halfHeight, 0));
            normals.Add(Vector3.UnitY);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int slice = 0; slice <= slices; slice++)
            {
                float u = (float)slice / slices;
                float theta = u * MathF.PI * 2.0f;

                float x = MathF.Cos(theta) * radius;
                float z = MathF.Sin(theta) * radius;

                positions.Add(new Vector3(x, halfHeight, z));
                normals.Add(Vector3.UnitY);

                uvs.Add(
                    new Vector2(
                        x / (radius * 2.0f) + 0.5f,
                        z / (radius * 2.0f) + 0.5f
                    )
                );
            }

            int topRing = topCenter + 1;

            for (int slice = 0; slice < slices; slice++)
            {
                indices.Add((uint)topCenter);
                indices.Add((uint)(topRing + slice));
                indices.Add((uint)(topRing + slice + 1));
            }

            int bottomCenter = positions.Count;

            positions.Add(new Vector3(0, -halfHeight, 0));
            normals.Add(-Vector3.UnitY);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int slice = 0; slice <= slices; slice++)
            {
                float u = (float)slice / slices;
                float theta = u * MathF.PI * 2.0f;

                float x = MathF.Cos(theta) * radius;
                float z = MathF.Sin(theta) * radius;

                positions.Add(new Vector3(x, -halfHeight, z));
                normals.Add(-Vector3.UnitY);

                uvs.Add(
                    new Vector2(
                        x / (radius * 2.0f) + 0.5f,
                        z / (radius * 2.0f) + 0.5f
                    )
                );
            }

            int bottomRing = bottomCenter + 1;

            for (int slice = 0; slice < slices; slice++)
            {
                indices.Add((uint)bottomCenter);
                indices.Add((uint)(bottomRing + slice + 1));
                indices.Add((uint)(bottomRing + slice));
            }

            return CreateMesh(
                [.. positions],
                [.. normals],
                [.. uvs],
                indices: [.. indices]
            );
        }

        public static Mesh CreateCircle(int segments = 16)
        {
            Vector3[] positions = new Vector3[segments + 1];
            Vector2[] uvs = new Vector2[segments + 1];
            Vector3[] normals = new Vector3[segments + 1];
            uint[] indices = new uint[segments * 3];

            positions[0] = Vector3.Zero;
            uvs[0] = new Vector2(0.5f, 0.5f);
            normals[0] = Vector3.UnitZ;

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * MathF.Tau;
                float x = MathF.Cos(angle);
                float y = MathF.Sin(angle);
                int vertex = i + 1;

                positions[vertex] = new Vector3(
                    x * 0.5f,
                    y * 0.5f,
                    0
                );

                uvs[vertex] = new Vector2(
                    x * 0.5f + 0.5f,
                    y * 0.5f + 0.5f
                );

                normals[vertex] = Vector3.UnitZ;
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int index = i * 3;

                indices[index + 0] = 0;
                indices[index + 1] = (uint)next + 1;
                indices[index + 2] = (uint)i + 1;
            }

            return CreateMesh(
                positions,
                normals,
                uvs,
                indices: indices
            );
        }

        public static Mesh CreateIcosphere(
            int subdivisions = 2,
            float radius = 0.5f)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(subdivisions);

            float t = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;

            List<Vector3> positions =
            [
                new(-1,  t, 0),
                new( 1,  t, 0),
                new(-1, -t, 0),
                new( 1, -t, 0),
                new(0, -1,  t),
                new(0,  1,  t),
                new(0, -1, -t),
                new(0,  1, -t),
                new( t, 0, -1),
                new( t, 0,  1),
                new(-t, 0, -1),
                new(-t, 0,  1)
            ];

            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] =
                    Vector3.Normalize(positions[i]) * radius;
            }

            List<uint> indices =
            [
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,

                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,

                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,

                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            ];

            for (int subdivision = 0; subdivision < subdivisions; subdivision++)
            {
                Dictionary<(int, int), uint> midpointCache = [];
                List<uint> newIndices = new(indices.Count * 4);

                uint GetMidpoint(int a, int b)
                {
                    int min = Math.Min(a, b);
                    int max = Math.Max(a, b);
                    var key = (min, max);

                    if (midpointCache.TryGetValue(key, out uint index))
                    {
                        return index;
                    }

                    Vector3 midpoint = Vector3.Normalize(
                        (positions[a] + positions[b]) * 0.5f
                    ) * radius;

                    index = (uint)positions.Count;
                    positions.Add(midpoint);
                    midpointCache[key] = index;

                    return index;
                }

                for (int i = 0; i < indices.Count; i += 3)
                {
                    uint a = indices[i];
                    uint b = indices[i + 1];
                    uint c = indices[i + 2];

                    uint ab = GetMidpoint((int)a, (int)b);
                    uint bc = GetMidpoint((int)b, (int)c);
                    uint ca = GetMidpoint((int)c, (int)a);

                    newIndices.Add(a);
                    newIndices.Add(ab);
                    newIndices.Add(ca);

                    newIndices.Add(b);
                    newIndices.Add(bc);
                    newIndices.Add(ab);

                    newIndices.Add(c);
                    newIndices.Add(ca);
                    newIndices.Add(bc);

                    newIndices.Add(ab);
                    newIndices.Add(bc);
                    newIndices.Add(ca);
                }

                indices = newIndices;
            }

            List<Vector3> normals = new(positions.Count);
            List<Vector2> uvs = new(positions.Count);

            foreach (Vector3 position in positions)
            {
                Vector3 normal = Vector3.Normalize(position);

                normals.Add(normal);

                float u =
                    0.5f +
                    MathF.Atan2(normal.Z, normal.X) /
                    (2.0f * MathF.PI);

                float v =
                    0.5f -
                    MathF.Asin(normal.Y) /
                    MathF.PI;

                uvs.Add(new Vector2(u, v));
            }

            return CreateMesh(
                [.. positions],
                [.. normals],
                [.. uvs],
                indices: [.. indices]
            );
        }
    }
}