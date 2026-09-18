using Assimp;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using DevoidGPU;
using MessagePack;
using SharpDX.DXGI;
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
        private readonly List<Guid> _processedTextures = [];

        public override void Import(ImportContext importContext, ModelImportSettings settings)
        {
            AssimpContext ctx = new();

            var scene = ctx.ImportFile(importContext.AssetPath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                //PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.FlipUVs | 
                PostProcessSteps.MakeLeftHanded |
                PostProcessSteps.OptimizeMeshes |
                PostProcessSteps.FlipWindingOrder
            );

            Matrix4x4 axis = AxisHelper.BuildAxisMatrix(
                settings.SourceUp,
                settings.SourceForward);

            _meshGuids.Clear();
            _materialGuids.Clear();
            _processedTextures.Clear();

            ImportMaterials(scene, importContext);

            PackedScene packed = BuildPackedScene(scene, importContext, settings, axis);

            packed.MeshGuids = [.. _meshGuids];
            packed.MaterialGuids = [.. _materialGuids];


            File.WriteAllBytes(
                importContext.GetRootOutputPath("packedscene"),
                MessagePackSerializer.Serialize(packed));

            ctx.Dispose();

        }

        public override bool Exists(ImportContext importContext)
        {
            return File.Exists(importContext.GetRootOutputPath(OutputExtension));
        }

        private PackedScene BuildPackedScene(AssimpScene scene, ImportContext ctx, ModelImportSettings settings, Matrix4x4 axis)
        {
            Dictionary<string, PackedLight> lights = [];

            foreach (var light in scene.Lights)
                lights[light.Name] = ConvertLight(light);

            List<PackedSceneNode> nodes = [];


            ProcessNode(scene.RootNode, -1, nodes, scene, ctx, axis, lights);



            return new PackedScene
            {
                Nodes = [.. nodes],
                MeshGuids = [.. _meshGuids],
                MaterialGuids = [.. _materialGuids]
            };
        }

        private void ProcessNode(Node node, int parent, List<PackedSceneNode> nodes, AssimpScene scene, ImportContext ctx, Matrix4x4 axis, Dictionary<string, PackedLight> lights)
        {
            int nodeIndex = nodes.Count;

            Matrix4x4 local = Matrix4x4.Transpose(node.Transform);

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
                Scale = scale
            };

            if (node.MeshIndices.Count > 0)
            {
                MeshAsset mesh = ConvertNodeMesh(node, scene);

                ulong subAssetId = (ulong)nodeIndex;

                Guid meshGuid = Engine.Instance.AssetDatabase.RegisterSubAsset(ctx.Guid, subAssetId);

                int meshIndex = _meshGuids.Count;
                _meshGuids.Add(meshGuid);

                File.WriteAllBytes(ctx.GetOutputPath(subAssetId, "mesh"), MessagePackSerializer.Serialize(mesh));

                packed.Mesh = new()
                {
                    MeshIndex = meshIndex,
                    MaterialGuids = [.. mesh.Surfaces.Select(x => x.MaterialGuid)]
                };
            }

            if (lights.TryGetValue(node.Name, out var light))
                packed.Light = light;

            nodes.Add(packed);

            foreach (var child in node.Children)
            {
                ProcessNode(
                    child,
                    nodeIndex,
                    nodes,
                    scene,
                    ctx,
                    axis,
                    lights);
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

        private static PackedLight ConvertLight(Assimp.Light light)
        {
            float intensityLuminance = (0.2126f * light.ColorDiffuse.X) + (0.7152f * light.ColorDiffuse.Y) + (0.0722f * light.ColorDiffuse.Z);
            intensityLuminance /= 54.351413f;
            Console.WriteLine("Intensity Luminance: " + intensityLuminance);

            PackedLight packed = new()
            {
                Name = light.Name,
                Color = new Vector3(light.ColorDiffuse.X, light.ColorDiffuse.Y, light.ColorDiffuse.Z),

                Range = light.AttenuationLinear > 0
                    ? 1.0f / light.AttenuationLinear
                    : 0f,

                InnerCone = light.AngleInnerCone,
                OuterCone = light.AngleOuterCone,

                Intensity = intensityLuminance
            };


            switch (light.LightType)
            {
                case Assimp.LightSourceType.Directional:
                    packed.Type = LightType.DirectionalLight;
                    packed.Range = 0;
                    break;

                case Assimp.LightSourceType.Point:
                    packed.Type = LightType.PointLight;
                    break;

                case Assimp.LightSourceType.Spot:
                    packed.Type = LightType.SpotLight;
                    break;

                default:
                    packed.Type = LightType.PointLight;
                    break;
            }

            return packed;
        }

        private MeshAsset ConvertNodeMesh(Node node, AssimpScene scene)
        {
            List<MeshSurfaceAsset> surfaces = new(node.MeshIndices.Count);

            foreach (int meshIndex in node.MeshIndices)
            {
                AssimpMesh mesh = scene.Meshes[meshIndex];

                Guid materialGuid = _materialGuids[mesh.MaterialIndex];

                MeshSurfaceAsset surface = ConvertMeshSurface(mesh, materialGuid);

                surfaces.Add(surface);
            }

            return new MeshAsset
            {
                Surfaces = [.. surfaces]
            };
        }

        private static MeshSurfaceAsset ConvertMeshSurface(AssimpMesh mesh, Guid materialGuid)
        {
            int vertexCount = mesh.VertexCount;

            float[] positions = new float[vertexCount * 3];

            for (int i = 0; i < vertexCount; i++)
            {
                var v = mesh.Vertices[i];
                int idx = i * 3;

                positions[idx] = v.X;
                positions[idx + 1] = v.Y;
                positions[idx + 2] = v.Z;
            }

            float[] normals = mesh.HasNormals
                ? new float[vertexCount * 3]
                : [];

            if (mesh.HasNormals)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var n = mesh.Normals[i];
                    int idx = i * 3;

                    normals[idx] = n.X;
                    normals[idx + 1] = n.Y;
                    normals[idx + 2] = n.Z;
                }
            }

            float[] uvs = mesh.HasTextureCoords(0)
                ? new float[vertexCount * 2]
                : [];

            if (mesh.HasTextureCoords(0))
            {
                var uvList = mesh.TextureCoordinateChannels[0];

                for (int i = 0; i < vertexCount; i++)
                {
                    var uv = uvList[i];
                    int idx = i * 2;

                    uvs[idx] = uv.X;
                    uvs[idx + 1] = uv.Y;
                }
            }

            bool hasTangents =
                mesh.Tangents.Count > 0 &&
                mesh.BiTangents.Count > 0;

            float[] tangents = hasTangents
                ? new float[vertexCount * 3]
                : [];

            float[] bitangents = hasTangents
                ? new float[vertexCount * 3]
                : [];

            if (hasTangents)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var tangent = mesh.Tangents[i];
                    var bitangent = mesh.BiTangents[i];

                    int idx = i * 3;

                    tangents[idx] = tangent.X;
                    tangents[idx + 1] = tangent.Y;
                    tangents[idx + 2] = tangent.Z;

                    bitangents[idx] = bitangent.X;
                    bitangents[idx + 1] = bitangent.Y;
                    bitangents[idx + 2] = bitangent.Z;
                }
            }

            int totalIndices = 0;

            for (int i = 0; i < mesh.FaceCount; i++)
                totalIndices += mesh.Faces[i].IndexCount;

            uint[] indices = new uint[totalIndices];

            int indexPtr = 0;

            for (int i = 0; i < mesh.FaceCount; i++)
            {
                var face = mesh.Faces[i];

                for (int j = 0; j < face.IndexCount; j++)
                    indices[indexPtr++] = (uint)face.Indices[j];
            }

            return new MeshSurfaceAsset
            {
                Positions = positions,
                Normals = normals,
                UVs = uvs,
                Tangents = tangents,
                Bitangents = bitangents,
                Indices = indices,
                MaterialGuid = materialGuid
            };
        }
        MaterialAsset ConvertMaterial(Assimp.Material mat, string modelPath)
        {
            MaterialAsset asset = new();

            MaterialProperty roughnessProperty = mat.GetProperty("$mat.roughnessFactor,0,0");
            MaterialProperty metallicProperty = mat.GetProperty("$mat.metallicFactor,0,0");
            MaterialProperty clearCoatFactorProperty = mat.GetProperty("$mat.clearcoat.factor,0,0");
            MaterialProperty clearcoatRoughnessProperty = mat.GetProperty("$mat.clearcoat.roughnessFactor,0,0");
            MaterialProperty transmissionProperty = mat.GetProperty("$mat.transmission.factor,0,0");

            

            asset.Floats["AO"] = 1f;
            asset.Floats["Alpha"] = 1f;

            asset.Floats["Clearcoat"] = clearCoatFactorProperty?.GetFloatValue() ?? 0;
            asset.Floats["ClearcoatRoughness"] = clearcoatRoughnessProperty?.GetFloatValue() ?? 1;

            if (mat.HasColorDiffuse)
            {
                asset.Vector4s["Albedo"] = mat.ColorDiffuse;
            }

            if (transmissionProperty != null)
            {
                asset.Floats["Alpha"] = 0.5f;
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

            if (metallicProperty != null)
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

                Guid texGuid = ImportTexture(tex, modelPath, true);

                asset.Textures["MAT_AlbedoMap"] = texGuid;
            }

            if (mat.HasTextureEmissive)
            {
                mat.GetMaterialTexture(
                    Assimp.TextureType.Emissive,
                    0,
                    out var tex);

                Guid texGuid = ImportTexture(tex, modelPath, true);

                asset.Textures["MAT_EmissiveMap"] = texGuid;
            }

            if (mat.HasTextureNormal)
            {
                mat.GetMaterialTexture(
                    Assimp.TextureType.Normals,
                    0,
                    out var tex);

                Guid texGuid = ImportTexture(tex, modelPath);

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


                Guid texGuid = ImportTexture(tex, modelPath);

                

                asset.Textures["MAT_RoughnessMap"] = texGuid;
            }

            //IEnumerable<TextureSlot> slots = mat.GetAllMaterialTextures();

            //foreach (var slot in slots)
            //{
            //    Console.WriteLine(slot.TextureType);
            //}

            //MaterialProperty[] mps = mat.GetAllProperties();
            //foreach (MaterialProperty mp in mps)
            //{
            //    Console.WriteLine(mp.FullyQualifiedName + " : " + mp.GetFloatValue());
            //    //if (mp.FullyQualifiedName == "$clr.diffuse,0,0")
            //    //{
            //    //    Console.WriteLine("DIFFUSE: " + mp.GetVector4Value());
            //    //}
            //}

            //Console.WriteLine(mat.Opacity);

            return asset;
        }

        WrapMode GetWrap(TextureWrapMode mode)
        {
            return mode switch
            {
                TextureWrapMode.Wrap => WrapMode.Repeat,
                TextureWrapMode.Mirror => WrapMode.Mirror,
                TextureWrapMode.Clamp => WrapMode.ClampToEdge,
                _ => WrapMode.Repeat,
            };
        }

        Guid ImportTexture(TextureSlot tex, string currentModelPath, bool srgb = false)
        {
            string absolutePath = Path.Combine(Path.GetDirectoryName(currentModelPath)!, tex.FilePath);

            absolutePath = Path.GetFullPath(absolutePath);

            string assetPath = Path.GetRelativePath(Engine.Instance.ProjectSystem.AssetPath, absolutePath);

            assetPath = assetPath.Replace('\\', '/');

            if (Engine.Instance.AssetDatabase.TryGetGuid(assetPath, out var guid))
            {
                if (!_processedTextures.Contains(guid))
                {
                    Engine.Instance.AssetDatabase.Reimport(guid, MessagePackSerializer.Serialize<TextureImportSettings>(new TextureImportSettings()
                    {
                        Format = srgb ? TextureFormat.RGBA8_UNorm_SRGB : TextureFormat.RGBA8_UNorm,
                        WrapU = GetWrap(tex.WrapModeU),
                        WrapV = GetWrap(tex.WrapModeV),
                    }));
                    _processedTextures.Add(guid);
                }

                return guid;
            }

            Console.WriteLine($"Texture not found in AssetDatabase: {assetPath}");
            return Guid.Empty;
        }
    }
}
