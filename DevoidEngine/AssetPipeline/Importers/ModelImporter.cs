using Assimp;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.Util;
using MessagePack;
using System.Numerics;

using AssimpContext = Assimp.AssimpContext;
using AssimpMesh = Assimp.Mesh;
using AssimpScene = Assimp.Scene;
using PostProcessSteps = Assimp.PostProcessSteps;

namespace DevoidEngine.AssetPipeline.Importers
{
    public class ModelImporter : AssetImporter<ModelImportSettings>
    {
        public override string Name => "ModelImporter";

        public override IReadOnlyList<string> Extensions =>
            [".fbx", ".gltf", ".glb", ".obj"];

        public override string OutputExtension => "packedscene";
        public override int Priority => 200;

        public override ModelImportSettings DefaultSettings()
        {
            return new ModelImportSettings();
        }

        private readonly List<Guid> _meshGuids = [];
        private readonly List<Guid> _materialGuids = [];

        public override void Import(ImportContext importContext, ModelImportSettings settings)
        {
            Console.WriteLine($"Model Path: {importContext.AssetPath}");

            AssimpContext ctx = new();

            var scene = ctx.ImportFile(importContext.AssetPath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.FlipWindingOrder
            );

            Matrix4x4 axis = AxisHelper.BuildAxisMatrix(
                settings.SourceUp,
                settings.SourceForward);

            _meshGuids.Clear();
            _materialGuids.Clear();

            ImportMaterials(scene, importContext);

            ImportMeshes(scene, importContext);

            PackedScene packed =
                BuildPackedScene(scene, settings, axis);

            packed.MeshGuids = [.. _meshGuids];
            packed.MaterialGuids = [.. _materialGuids];

            File.WriteAllBytes(
                importContext.GetRootOutputPath("packedscene"),
                MessagePackSerializer.Serialize(packed));

        }

        public override bool Exists(ImportContext importContext)
        {
            return File.Exists(importContext.GetRootOutputPath(OutputExtension));
        }

        private PackedScene BuildPackedScene(
    AssimpScene scene,
    ModelImportSettings settings,
    Matrix4x4 axis)
        {
            List<PackedSceneNode> nodes = [];

            ProcessNode(
                scene.RootNode,
                -1,
                nodes,
                scene,
                axis);

            return new PackedScene
            {
                Nodes = [.. nodes],
                MeshGuids = [.. _meshGuids],
                MaterialGuids = [.. _materialGuids]
            };
        }

        private void ProcessNode(
    Node node,
    int parent,
    List<PackedSceneNode> nodes,
    AssimpScene scene,
    Matrix4x4 axis)
        {
            int nodeIndex = nodes.Count;

            Matrix4x4 local = Matrix4x4.Transpose(node.Transform);

            if (parent == -1)
                local = axis * local;

            Matrix4x4.Decompose(
                local,
                out Vector3 scale,
                out Quaternion rotation,
                out Vector3 translation);

            PackedSceneNode packed = new()
            {
                Name = node.Name,
                Parent = parent,
                Translation = translation,
                Rotation = rotation,
                Scale = scale,
                MeshIndices = [.. node.MeshIndices]
            };

            nodes.Add(packed);

            foreach (var child in node.Children)
            {
                ProcessNode(
                    child,
                    nodeIndex,
                    nodes,
                    scene,
                    axis);
            }
        }

        private void ImportMaterials(
    AssimpScene scene,
    ImportContext ctx)
        {
            for (int i = 0; i < scene.MaterialCount; i++)
            {
                Guid guid = Engine.Instance.AssetDatabase.RegisterSubAsset(
                    ctx.Guid,
                    (ulong)(100000 + i)
                );

                _materialGuids.Add(guid);

                MaterialAsset material =
                    ConvertMaterial(
                        scene.Materials[i],
                        ctx.AssetPath);

                File.WriteAllBytes(
                    ctx.GetOutputPath((ulong)(100000 + i), "material"),
                    MessagePackSerializer.Serialize(material));
            }
        }
        private void ImportMeshes(
    AssimpScene scene,
    ImportContext ctx)
        {
            for (int i = 0; i < scene.MeshCount; i++)
            {
                Guid guid = Engine.Instance.AssetDatabase.RegisterSubAsset(
                    ctx.Guid,
                    (ulong)i);

                _meshGuids.Add(guid);

                MeshAsset mesh = ConvertMesh(scene.Meshes[i]);

                //mesh.Material = _materialGuids[mesh.MaterialIndex];

                File.WriteAllBytes(
                    ctx.GetOutputPath((ulong)i, "mesh"),
                    MessagePackSerializer.Serialize(mesh));
            }
        }

        MeshAsset ConvertMesh(AssimpMesh mesh)
        {
            MeshAsset asset = new()
            {
                Positions = [.. mesh.Vertices.SelectMany(v => new float[] { v.X, v.Y, v.Z })],

                Normals = [.. mesh.Normals.SelectMany(v => new float[] { v.X, v.Y, v.Z })],

                UVs = [.. mesh.TextureCoordinateChannels[0].SelectMany(v => new float[] { v.X, v.Y })],

                Tangents = [.. mesh.Tangents.SelectMany(v => new float[] { v.X, v.Y, v.Z })],

                Bitangents = [.. mesh.BiTangents.SelectMany(v => new float[] { v.X, v.Y, v.Z })],

                Indices = [.. mesh.Faces
                    .SelectMany(f => f.Indices)
                    .Select(i => (uint)i)],

                MaterialIndex = mesh.MaterialIndex,
                Material = _materialGuids[mesh.MaterialIndex]
            };

            return asset;
        }
        MaterialAsset ConvertMaterial(Assimp.Material mat, string modelPath)
        {
            MaterialAsset asset = new();

            MaterialProperty roughnessProperty = mat.GetProperty("$mat.roughnessFactor,0,0");
            MaterialProperty metallicProperty = mat.GetProperty("$mat.metallicFactor,0,0");
            MaterialProperty transmissionProperty = mat.GetProperty("$mat.transmission.factor,0,0");


            // This is where im deciding if a material is glass or not.
            if (transmissionProperty != null)
            {
                asset.Shader = "PBR/ForwardPBRGlass";
            }
            else
            {
                asset.Shader = "PBR/ForwardPBR";
            }

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

                asset.Vector3s["EmissiveColor"] = emissiveColor;
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
                Guid texGuid = ImportTexture(tex.FilePath, modelPath);

                asset.Textures["MAT_AlbedoMap"] = texGuid;
            }

            if (mat.HasTextureNormal)
            {
                mat.GetMaterialTexture(
                    Assimp.TextureType.Normals,
                    0,
                    out var tex);

                Guid texGuid = ImportTexture(tex.FilePath, modelPath);

                asset.Textures["MAT_NormalMap"] = texGuid;

                MaterialProperty normalScale = mat.GetProperty("$tex.scale,6,0");

                if (normalScale != null)
                {
                    asset.Floats["NormalStrength"] = normalScale.GetFloatValue();
                    Console.WriteLine("Normal scale set: " + normalScale.GetFloatValue());
                }
                else
                    asset.Floats["NormalStrength"] = 1;

                asset.Ints["useNormalMap"] = 1;
            }

            if (mat.PBR.HasTextureRoughness)
            {
                mat.GetMaterialTexture(
                    Assimp.TextureType.Roughness,
                    0,
                    out var tex);

                Guid texGuid = ImportTexture(tex.FilePath, modelPath);

                asset.Textures["MAT_RoughnessMap"] = texGuid;
            }

            //IEnumerable<TextureSlot> slots = mat.GetAllMaterialTextures();

            //foreach (var slot in slots)
            //{
            //    Console.WriteLine(slot.TextureType);
            //}

            MaterialProperty[] mps = mat.GetAllProperties();
            foreach (MaterialProperty mp in mps)
            {
                Console.WriteLine(mp.FullyQualifiedName + " : " + mp.GetFloatValue());
            }

            //Console.WriteLine(mat.Opacity);

            return asset;
        }

        Guid ImportTexture(string texturePath, string currentModelPath)
        {
            string resolvedPath = Path.Combine(Path.GetDirectoryName(currentModelPath)!, texturePath);
            resolvedPath = resolvedPath.Replace('\\', '/');   // normalize

            Console.WriteLine("[Model Importer]: " + resolvedPath);

            if (Engine.Instance.AssetDatabase.TryGetGuid(resolvedPath, out var guid))
                return guid;

            Console.WriteLine($"Texture not found in AssetDatabase: {resolvedPath}");
            return Guid.Empty;
        }
    }
}
