using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using AssimpScene = Assimp.Scene;
using Node = Assimp.Node;
using AssimpMesh = Assimp.Mesh;
using PostProcessSteps = Assimp.PostProcessSteps;
using AssimpContext = Assimp.AssimpContext;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public class ModelImporter : AssetImporter<ModelImportSettings>
    {
        public override string Name => "ModelImporter";

        public override IReadOnlyList<string> Extensions =>
            new[] { ".fbx", ".gltf", ".glb", ".obj" };

        public override string OutputExtension => "model";

        public override ModelImportSettings DefaultSettings()
        {
            return new ModelImportSettings();
        }

        public override void Import(
            string assetPath,
            Guid guid,
            ModelImportSettings settings,
            string outputPath)
        {
            Console.WriteLine($"Importing model {assetPath}");

            AssimpContext ctx = new();

            var scene = ctx.ImportFile(assetPath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.CalculateTangentSpace);

            var model = ConvertScene(scene, settings);

            File.WriteAllBytes(
                outputPath,
                MessagePackSerializer.Serialize(model));
        }

        private ModelAsset ConvertScene(AssimpScene scene, ModelImportSettings settings)
        {
            List<ModelNode> nodes = new();
            List<MeshAsset> meshes = new();

            Matrix4x4 axis = AxisHelper.BuildAxisMatrix(
                settings.SourceUp,
                settings.SourceForward);

            ProcessNode(scene.RootNode, -1, Matrix4x4.Identity, nodes, meshes, scene, settings, axis);

            return new ModelAsset
            {
                Nodes = nodes.ToArray(),
                Meshes = meshes.ToArray(),
                Materials = []
            };
        }

        void ProcessNode(
            Node node,
            int parent,
            Matrix4x4 parentTransform,
            List<ModelNode> nodes,
            List<MeshAsset> meshes,
            AssimpScene scene,
            ModelImportSettings settings,
            Matrix4x4 axis
        )
        {
            int nodeIndex = nodes.Count;

            Matrix4x4 local = Matrix4x4.Transpose(node.Transform);

            // apply axis conversion ONLY on root
            if (parent == -1)
            {
                local = local * axis;
            }

            Matrix4x4 world = local * parentTransform;

            Matrix4x4.Decompose(
                world,
                out Vector3 scale,
                out Quaternion rotation,
                out Vector3 translation
            );

            List<int> nodeMeshes = new();

            foreach (int meshIndex in node.MeshIndices)
            {
                var mesh = ConvertMesh(scene.Meshes[meshIndex]);

                meshes.Add(mesh);

                nodeMeshes.Add(meshes.Count - 1);
            }

            nodes.Add(new ModelNode
            {
                Name = node.Name,
                Parent = parent,
                Translation = translation,
                Rotation = rotation,
                Scale = scale,
                MeshIndices = nodeMeshes.ToArray()
            });

            foreach (var child in node.Children)
            {
                ProcessNode(child, nodeIndex, world, nodes, meshes, scene, settings, axis);
            }
        }

        MeshAsset ConvertMesh(AssimpMesh mesh)
        {
            MeshAsset asset = new();

            asset.Positions = mesh.Vertices
                .SelectMany(v => new float[] { v.X, v.Y, v.Z })
                .ToArray();

            asset.Normals = mesh.Normals
                .SelectMany(v => new float[] { v.X, v.Y, v.Z })
                .ToArray();

            asset.UVs = mesh.TextureCoordinateChannels[0]
                .SelectMany(v => new float[] { v.X, v.Y })
                .ToArray();

            asset.Tangents = mesh.Tangents
                .SelectMany(v => new float[] { v.X, v.Y, v.Z })
                .ToArray();

            asset.Bitangents = mesh.BiTangents
                .SelectMany(v => new float[] { v.X, v.Y, v.Z })
                .ToArray();

            asset.Indices = mesh.Faces
                .SelectMany(f => f.Indices)
                .Select(i => (uint)i)
                .ToArray();

            asset.MaterialIndex = mesh.MaterialIndex;

            return asset;
        }
    }
}
