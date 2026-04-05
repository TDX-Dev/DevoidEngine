using Assimp;
using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Utilities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AssimpContext = Assimp.AssimpContext;
using AssimpMesh = Assimp.Mesh;
using AssimpScene = Assimp.Scene;
using Node = Assimp.Node;
using PostProcessSteps = Assimp.PostProcessSteps;

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
                PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.FlipWindingOrder
            );

            var model = ConvertScene(scene, settings, assetPath);

            model.MeshGuids = new Guid[model.Meshes.Length];
            model.MaterialGuids = new Guid[model.Materials.Length];

            for (int i = 0; i < model.Meshes.Length; i++)
            {
                Guid subGuid = AssetDatabase.RegisterSubAsset(
                    guid,
                    (ulong)i,
                    assetPath);

                model.MeshGuids[i] = subGuid;
            }

            for (int i = 0; i < model.Materials.Length; i++)
            {
                Guid subGuid = AssetDatabase.RegisterSubAsset(
                    guid,
                    (ulong)(model.Meshes.Length + i),
                    assetPath);

                model.MaterialGuids[i] = subGuid;
            }


            File.WriteAllBytes(
                outputPath,
                MessagePackSerializer.Serialize(model));
        }

        private ModelAsset ConvertScene(AssimpScene scene, ModelImportSettings settings, string outputPath)
        {
            List<ModelNode> nodes = new();
            List<MeshAsset> meshes = new();
            List<MaterialAsset> materials = new();

            Matrix4x4 axis = AxisHelper.BuildAxisMatrix(
                settings.SourceUp,
                settings.SourceForward);

            foreach (var mat in scene.Materials)
            {
                MaterialAsset matAsset = ConvertMaterial(mat, outputPath);
                materials.Add(matAsset);
            }

            ProcessNode(scene.RootNode, -1, Matrix4x4.Identity, nodes, meshes, scene, settings, axis);

            return new ModelAsset
            {
                Nodes = nodes.ToArray(),
                Meshes = meshes.ToArray(),
                Materials = materials.ToArray()
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

        MaterialAsset ConvertMaterial(Assimp.Material mat, string currentModelPath)
        {
            MaterialAsset asset = new();

            asset.Shader = "PBR/ForwardPBR";

            MaterialProperty roughnessProperty = mat.GetProperty("$mat.roughnessFactor,0,0");
            MaterialProperty metallicProperty = mat.GetProperty("$mat.metallicFactor,0,0");

            asset.Floats["AO"] = 1f;

            if (mat.HasColorDiffuse)
            {
                asset.Vector4s["Albedo"] = mat.ColorDiffuse;
            }

            if (roughnessProperty != null)
            {
                float roughness = roughnessProperty.GetFloatValue();
                asset.Floats["Roughness"] = roughness;
            }
            else
            {
                asset.Floats["Roughness"] = 0.5f;
            }

            if (roughnessProperty != null)
            {
                float metallic = metallicProperty.GetFloatValue();
                asset.Floats["Metallic"] = metallic;
            }
            else
            {
                asset.Floats["Metallic"] = 0f;
            }

            if (mat.HasColorEmissive)
            {
                var e = mat.ColorEmissive;

                Vector3 emissiveColor = e.AsVector3();
                float emissiveStrength = mat.GetProperty("$mat.emissiveIntensity,0,0")?.GetFloatValue() ?? 0f;

                asset.Vector3s["EmissiveColor"] = mat.ColorEmissive.AsVector3();
                asset.Floats["EmissiveStrength"] = emissiveStrength;
            }
            else
            {
                asset.Vector3s["EmissiveColor"] = Vector3.Zero;
                asset.Floats["EmissiveStrength"] = 0f;
            }


            if (mat.HasTextureDiffuse)
            {
                mat.GetMaterialTexture(
                    Assimp.TextureType.Diffuse,
                    0,
                    out var tex);

                Console.WriteLine(tex.FilePath);
                Guid texGuid = ImportTexture(tex.FilePath);

                asset.Textures["MAT_AlbedoMap"] = texGuid;
            }

            if (mat.HasTextureNormal)
            {
                mat.GetMaterialTexture(
                    Assimp.TextureType.Normals,
                    0,
                    out var tex);

                Guid texGuid = ImportTexture(tex.FilePath);

                asset.Textures["MAT_NormalMap"] = texGuid;
            }

            //MaterialProperty[] mps = mat.GetAllProperties();
            //foreach (MaterialProperty mp in mps)
            //{
            //    Console.WriteLine(mp.FullyQualifiedName + " : " + mp.GetFloatValue());
            //}

            return asset;
        }

        Guid ImportTexture(string texturePath)
        {
            Console.WriteLine("[Model Importer]: " + texturePath);

            if (AssetDatabase.TryGetGuid(texturePath, out var guid))
                return guid;

            Console.WriteLine($"Texture not found in AssetDatabase: {texturePath}");
            return Guid.Empty;
        }
    }
}
