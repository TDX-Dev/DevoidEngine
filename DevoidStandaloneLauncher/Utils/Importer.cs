using Assimp;
using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidStandaloneLauncher.Utils
{
    public static class Importer
    {
        public static void LoadModel(string filePath)
        {
            AssimpContext importer = new AssimpContext();


            PostProcessSteps importFlags =
                PostProcessSteps.Triangulate |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.FlipWindingOrder;


            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(filePath, importFlags);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("Processing nodes");
            Console.WriteLine($"Scene has {scene.MeshCount} Meshes");

            var lightLookup = scene.Lights.ToDictionary(l => l.Name, l => l);
            ProcessNode(scene.RootNode, scene, lightLookup);
        }

        static void ProcessNode(Node node, Assimp.Scene scene,
                                Dictionary<string, Assimp.Light> lightLookup)
        {
            LevelSpawnRegistry.Execute(node.Name, node, scene);

            if (lightLookup.TryGetValue(node.Name, out var light))
            {
                LevelSpawnRegistry.ExecuteLight(node.Name, node, light);
            }

            foreach (int meshIndex in node.MeshIndices)
            {
                var mesh = scene.Meshes[meshIndex];
                ProcessMesh(mesh);
            }

            foreach (var child in node.Children)
                ProcessNode(child, scene, lightLookup);
        }

        static void ProcessMesh(Assimp.Mesh mesh)
        {

            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<uint> indices = new();

            Console.WriteLine(mesh.HasNormals);
            Console.WriteLine(mesh.VertexCount);
            Console.WriteLine(mesh.FaceCount);

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                vertices.Add(new Vector3(
                    mesh.Vertices[i].X,
                    mesh.Vertices[i].Y,
                    mesh.Vertices[i].Z));

                if (mesh.HasNormals)
                {
                    normals.Add(new Vector3(
                        mesh.Normals[i].X,
                        mesh.Normals[i].Y,
                        mesh.Normals[i].Z));
                }

                if (mesh.TextureCoordinateChannelCount > 0)
                {
                    var tex = mesh.TextureCoordinateChannels[0][i];
                    uvs.Add(new Vector2(tex.X, tex.Y));
                }
            }

            foreach (var face in mesh.Faces)
            {
                foreach (var index in face.Indices)
                    indices.Add((uint)index);
            }
        }

        public static (Vector3, System.Numerics.Quaternion, Vector3) GetTransform(Node node)
        {
            var m = node.Transform;

            System.Numerics.Matrix4x4 matrix = new System.Numerics.Matrix4x4(
                m.A1, m.B1, m.C1, m.D1,
                m.A2, m.B2, m.C2, m.D2,
                m.A3, m.B3, m.C3, m.D3,
                m.A4, m.B4, m.C4, m.D4
            );

            System.Numerics.Matrix4x4.Decompose(matrix,
                out Vector3 scale,
                out System.Numerics.Quaternion rotation,
                out Vector3 translation);

            return (translation, rotation, scale);
        }

        public static DevoidEngine.Engine.Core.Mesh ConvertMesh(Node node, Assimp.Scene scene)
        {
            var assimpMesh = scene.Meshes[node.MeshIndices[0]];

            Vertex[] vertices = new Vertex[assimpMesh.VertexCount];
            List<int> indices = new();

            for (int i = 0; i < assimpMesh.VertexCount; i++)
            {
                Vector3 position = new Vector3(
                    assimpMesh.Vertices[i].X,
                    assimpMesh.Vertices[i].Y,
                    assimpMesh.Vertices[i].Z);

                Vector3 normal = assimpMesh.HasNormals
                    ? new Vector3(
                        assimpMesh.Normals[i].X,
                        assimpMesh.Normals[i].Y,
                        assimpMesh.Normals[i].Z)
                    : Vector3.Zero;

                Vector2 uv = Vector2.Zero;
                if (assimpMesh.TextureCoordinateChannelCount > 0)
                {
                    var tex = assimpMesh.TextureCoordinateChannels[0][i];
                    uv = new Vector2(tex.X, tex.Y);
                }

                Vector3 tangent = assimpMesh.HasTangentBasis
                    ? new Vector3(
                        assimpMesh.Tangents[i].X,
                        assimpMesh.Tangents[i].Y,
                        assimpMesh.Tangents[i].Z)
                    : Vector3.Zero;

                Vector3 bitangent = assimpMesh.HasTangentBasis
                    ? new Vector3(
                        assimpMesh.BiTangents[i].X,
                        assimpMesh.BiTangents[i].Y,
                        assimpMesh.BiTangents[i].Z)
                    : Vector3.Zero;

                vertices[i] = new DevoidGPU.Vertex(position, normal, uv, tangent, bitangent);
            }

            foreach (var face in assimpMesh.Faces)
            {
                // Triangulate flag ensures 3 indices
                indices.Add(face.Indices[0]);
                indices.Add(face.Indices[1]);
                indices.Add(face.Indices[2]);
            }

            var mesh = new DevoidEngine.Engine.Core.Mesh();
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices.ToArray());

            return mesh;
        }

        public static void ApplyTransform(GameObject go, Node node)
        {
            var m = node.Transform;

            System.Numerics.Matrix4x4 matrix = new System.Numerics.Matrix4x4(
                m.A1, m.B1, m.C1, m.D1,
                m.A2, m.B2, m.C2, m.D2,
                m.A3, m.B3, m.C3, m.D3,
                m.A4, m.B4, m.C4, m.D4
            );

            System.Numerics.Matrix4x4.Decompose(matrix,
                out Vector3 scale,
                out System.Numerics.Quaternion rotation,
                out Vector3 translation);

            //float temp = scale.Z;
            //scale.Z = scale.Y;
            //scale.Y = temp;


            go.transform.Position = translation;
            go.transform.Rotation = rotation;
            go.transform.Scale = scale;
        }
    }
}
