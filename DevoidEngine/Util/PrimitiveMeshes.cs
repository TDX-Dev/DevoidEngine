using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Util
{
    public static class PrimitiveMeshes
    {
        private static Mesh? cube;
        private static Mesh? fullscreenMesh;

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
    }
}
