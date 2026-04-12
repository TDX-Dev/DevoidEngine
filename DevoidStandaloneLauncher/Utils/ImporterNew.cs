using Assimp;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Threading.Channels;

namespace DevoidStandaloneLauncher.Utils
{
    public static class Importer
    {
        static Dictionary<int, DevoidEngine.Engine.Core.Mesh> meshCache = new();
        static Dictionary<int, MaterialInstance> materialCache = new();
        public static void LoadModel(string filePath)
        {
            Assimp.AssimpContext importer = new AssimpContext();


            PostProcessSteps importFlags =
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.FlipWindingOrder;
            //PostProcessSteps.JoinIdenticalVertices;
            filePath = Path.GetFullPath(filePath);

            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(filePath, importFlags);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

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

            foreach (var child in node.Children)
                ProcessNode(child, scene, lightLookup);
        }
        public static (Vector3, Quaternion, Vector3) GetTransform(Node node)
        {
            Matrix4x4 matrix = Matrix4x4.Transpose(node.Transform);

            Matrix4x4.Decompose(matrix,
                out Vector3 scale,
                out Quaternion rotation,
                out Vector3 translation);

            return (translation, rotation, scale);
        }

        public static DevoidEngine.Engine.Core.Mesh ConvertMesh(Node node, Assimp.Scene scene)
        {
            int meshIndex = node.MeshIndices[0];

            if (meshCache.TryGetValue(meshIndex, out var cachedMesh))
                return cachedMesh;

            var assimpMesh = scene.Meshes[meshIndex];

            if (assimpMesh.VertexCount == 0 || assimpMesh.FaceCount == 0)
                return null;

            int vertexCount = assimpMesh.VertexCount;
            Vertex[] vertices = new Vertex[vertexCount];
            int indexCount = assimpMesh.FaceCount * 3;
            int[] indices = new int[indexCount];

            bool hasNormals = assimpMesh.HasNormals;
            bool hasUVs = assimpMesh.TextureCoordinateChannelCount > 0;
            bool uvIs1D = hasUVs && assimpMesh.UVComponentCount[0] == 1;
            bool hasTangents = assimpMesh.HasTangentBasis;

            Vector3 min = new Vector3(
                assimpMesh.Vertices[0].X,
                assimpMesh.Vertices[0].Y,
                assimpMesh.Vertices[0].Z);
            Vector3 max = new Vector3(
                assimpMesh.Vertices[0].X,
                assimpMesh.Vertices[0].Y,
                assimpMesh.Vertices[0].Z);

            for (int i = 0; i < assimpMesh.VertexCount; i++)
            {
                Vector3 position = new Vector3(
                    assimpMesh.Vertices[i].X,
                    assimpMesh.Vertices[i].Y,
                    assimpMesh.Vertices[i].Z);

                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);

                Vector3 normal = hasNormals
                    ? new Vector3(
                        assimpMesh.Normals[i].X,
                        assimpMesh.Normals[i].Y,
                        assimpMesh.Normals[i].Z)
                    : Vector3.Zero;

                Vector2 uv = Vector2.Zero;
                if (hasUVs)
                {
                    var tex = assimpMesh.TextureCoordinateChannels[0][i];

                    uv = uvIs1D ? new Vector2(tex.X, 0) : new Vector2(tex.X, tex.Y);
                }

                Vector3 tangent = hasTangents
                    ? new Vector3(
                        assimpMesh.Tangents[i].X,
                        assimpMesh.Tangents[i].Y,
                        assimpMesh.Tangents[i].Z)
                    : Vector3.Zero;

                Vector3 bitangent = hasTangents
                    ? new Vector3(
                        assimpMesh.BiTangents[i].X,
                        assimpMesh.BiTangents[i].Y,
                        assimpMesh.BiTangents[i].Z)
                    : Vector3.Zero;

                vertices[i] = new DevoidGPU.Vertex(position, normal, uv, tangent, bitangent);
            }

            int idx = 0;
            foreach (var face in assimpMesh.Faces)
            {
                indices[idx++] = face.Indices[0];
                indices[idx++] = face.Indices[1];
                indices[idx++] = face.Indices[2];
            }

            var mesh = new DevoidEngine.Engine.Core.Mesh();
            mesh.LocalBounds = new DevoidEngine.Engine.Utilities.BoundingBox(min, max);
            mesh.SetVertices(vertices, false);
            mesh.SetIndices(indices);

            meshCache[meshIndex] = mesh;
            return mesh;
        }

        //public static AnimationPlayer CreateAnimationPlayer(GameObject root, Assimp.Animation anim)
        //{
        //    var clip = ConvertAnimation(anim);

        //    var player = new AnimationPlayer();
        //    player.Play(clip);

        //    // Bind directly using your hierarchy
        //    player.Bind(
        //        path => ResolveVec3(root, path),
        //        path => ResolveQuat(root, path),
        //        path => null // no float support yet
        //    );

        //    return player;
        //}

        static string NormalizeName(string name)
        {
            int colon = name.IndexOf(':');
            if (colon != -1)
                return name.Substring(0, colon);

            return name;
        }

        static Action<Vector3> ResolveVec3(GameObject root, string path)
        {
            char seperator = ':';

            string[] segments = path.Split(seperator);
            string nodeName = segments[0];
            string nodeModifier = segments[1];
            string property = segments[2];

            var go = FindNodeByName(root, nodeName);
            if (go == null)
            {
                return null;
            }

            var t = go.Transform;

            if (property == "position")
                return v =>
                {
                    t.LocalPosition = v;
                };

            if (property == "scale")
                return v => t.LocalScale = v;

            return null;
        }

        //static AnimationClip ConvertAnimation(Assimp.Animation anim)
        //{
        //    var clip = new AnimationClip();

        //    float tps = anim.TicksPerSecond != 0 ? (float)anim.TicksPerSecond : 25f;

        //    clip.TicksPerSecond = tps;
        //    clip.Length = (float)(anim.DurationInTicks / tps);

        //    foreach (var channel in anim.NodeAnimationChannels)
        //    {
        //        string node = channel.NodeName;

        //        // POSITION
        //        if (channel.PositionKeyCount > 0)
        //        {
        //            var track = new AnimationTrack<Vector3>(Vector3.Lerp);

        //            foreach (var key in channel.PositionKeys)
        //            {
        //                track.Keyframes.Add(new Keyframe<Vector3>
        //                {
        //                    Time = (float)(key.Time / tps),
        //                    Value = new Vector3(key.Value.X, key.Value.Y, key.Value.Z)
        //                });
        //            }

        //            clip.Vec3Channels.Add(new AnimationChannel<Vector3>
        //            {
        //                Path = node + ":position",
        //                Track = track
        //            });
        //        }

        //        // ROTATION
        //        if (channel.RotationKeyCount > 0)
        //        {
        //            var track = new AnimationTrack<Quaternion>(Quaternion.Slerp);

        //            foreach (var key in channel.RotationKeys)
        //            {
        //                track.Keyframes.Add(new Keyframe<Quaternion>
        //                {
        //                    Time = (float)(key.Time / tps),
        //                    Value = new Quaternion(
        //                        key.Value.X,
        //                        key.Value.Y,
        //                        key.Value.Z,
        //                        key.Value.W)
        //                });
        //            }

        //            clip.QuatChannels.Add(new AnimationChannel<Quaternion>
        //            {
        //                Path = node + ":rotation",
        //                Track = track
        //            });
        //        }

        //        // SCALE
        //        if (channel.ScalingKeyCount > 0)
        //        {
        //            var track = new AnimationTrack<Vector3>(Vector3.Lerp);

        //            foreach (var key in channel.ScalingKeys)
        //            {
        //                track.Keyframes.Add(new Keyframe<Vector3>
        //                {
        //                    Time = (float)(key.Time / tps),
        //                    Value = new Vector3(key.Value.X, key.Value.Y, key.Value.Z)
        //                });
        //            }

        //            clip.Vec3Channels.Add(new AnimationChannel<Vector3>
        //            {
        //                Path = node + ":scale",
        //                Track = track
        //            });
        //        }
        //    }

        //    return clip;
        //}

        //static Action<Quaternion> ResolveQuat(GameObject root, string path)
        //{
        //    var parts = path.Split(':');
        //    var nodeName = parts[0];

        //    var go = FindNodeByName(root, nodeName);
        //    if (go == null) return null;

        //    var t = go.Transform;

        //    return v => t.LocalRotation = v;
        //}

        static GameObject FindNodeByName(GameObject root, string name)
        {
            if (NormalizeName(root.Name) == NormalizeName(name))
                return root;

            foreach (var child in root.children)
            {
                var found = FindNodeByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }


        public static DevoidEngine.Engine.Core.MaterialInstance ConvertMaterial(Node node, Assimp.Scene scene, string baseModelPath)
        {



            var assimpMat = GetMaterial(node, scene);
            if (assimpMat == null) return null;

            int meshIndex = node.MeshIndices[0];
            var mesh = scene.Meshes[meshIndex];
            int matIndex = mesh.MaterialIndex;

            if (materialCache.TryGetValue(matIndex, out var cachedMat))
                return cachedMat;

            baseModelPath = Path.GetFullPath(baseModelPath);

            DevoidEngine.Engine.Core.MaterialInstance devoidMaterial = RenderingDefaults.GetMaterial();

            MaterialProperty roughnessProperty = assimpMat.GetProperty("$mat.roughnessFactor,0,0");

            Texture2D diffuseTexture = GetTexture(assimpMat, Assimp.TextureType.Diffuse, baseModelPath);
            Texture2D roughnessTexture = GetTexture(assimpMat, Assimp.TextureType.Shininess, baseModelPath);
            Texture2D normalTexture = GetTexture(assimpMat, Assimp.TextureType.Normals, baseModelPath);
            Texture2D emissiveTexture = GetTexture(assimpMat, Assimp.TextureType.Emissive, baseModelPath);

            if (diffuseTexture != null)
            {
                devoidMaterial.SetTexture("MAT_AlbedoMap", diffuseTexture);
            }

            if (normalTexture != null)
            {
                devoidMaterial.SetInt("useNormalMap", 1);
                if (assimpMat.HasBumpScaling)
                {
                    devoidMaterial.SetFloat("NormalStrength", assimpMat.BumpScaling);
                }
                else
                {
                    devoidMaterial.SetFloat("NormalStrength", 1);
                }
                devoidMaterial.SetTexture("MAT_NormalMap", normalTexture);
            }

            if (roughnessTexture != null)
            {
                devoidMaterial.SetTexture("MAT_RoughnessMap", roughnessTexture);
            }

            if (emissiveTexture != null)
            {
                devoidMaterial.SetTexture("MAT_EmissiveMap", emissiveTexture);
                Console.WriteLine("Emissive Map Loaded");
            }

            if (assimpMat.HasColorDiffuse)
            {
                var c = assimpMat.ColorDiffuse;
                devoidMaterial.SetVector4("Albedo", c);
            }
            else
            {
                devoidMaterial.SetVector4("Albedo", new Vector4(1, 1, 1, 1));
            }

            //if (node.Name == "Glassy:Model")
            //{
            //    MaterialProperty[] mps = assimpMat.GetAllProperties();
            //    foreach (var mp in mps)
            //    {
            //        Console.WriteLine(mp.Name);
            //        Console.WriteLine(mp.FullyQualifiedName);
            //        Console.WriteLine(mp.GetFloatValue());
            //    }
            //}

            if (roughnessProperty != null)
            {
                float roughness = assimpMat.GetProperty("$mat.roughnessFactor,0,0").GetFloatValue();
                devoidMaterial.SetFloat("Roughness", roughness);
            }
            else
            {
                devoidMaterial.SetFloat("Roughness", 0.5f);
            }



            //Console.WriteLine(assimpMat.BumpScaling);

            //if (assimpMat.HasColorSpecular)
            //{
            //    var c = assimpMat.ColorSpecular;
            //    devoidMaterial.SetVector3("SpecularColor", new Vector3(c.R, c.G, c.B));

            //}

            //MaterialProperty[] mps = assimpMat.GetAllProperties();
            //if (node.Name == "Glassy:Model")
            //{
            //    Console.WriteLine(node.Name);
            //    foreach (var mp in mps)
            //    {
            //        Console.WriteLine(mp.Name);
            //        Console.WriteLine(mp.GetFloatValue());
            //    }
            //}

            ////devoidMaterial.SetVector3("SpecularColor", new Vector3(1, 1, 1));
            if (assimpMat.HasReflectivity)
                devoidMaterial.SetFloat("Metallic", assimpMat.Reflectivity);
            else
                devoidMaterial.SetFloat("Metallic", 0f);

            devoidMaterial.SetFloat("AO", 1f);

            Vector3 emissiveColor = Vector3.Zero;
            float emissiveStrength = 1.0f;

            if (assimpMat.HasColorEmissive)
            {
                var e = assimpMat.ColorEmissive;

                emissiveColor = e.AsVector3();

                // optional: normalize color and extract strength
                emissiveStrength = MathF.Max(e.X, MathF.Max(e.Y, e.Z));

                if (emissiveStrength > 0)
                    emissiveColor /= emissiveStrength;
            }

            devoidMaterial.SetVector3("EmissiveColor", emissiveColor);
            devoidMaterial.SetFloat("EmissiveStrength", emissiveStrength);

            //if (assimpMat.HasOpacity)
            //{
            //    Console.WriteLine($"{node.Name} Possibly Glass: {assimpMat.Opacity}");
            //    //devoidMaterial.BaseMaterial.BlendMode = DevoidGPU.BlendMode.AlphaBlend;
            //}
            materialCache[matIndex] = devoidMaterial;
            return devoidMaterial;
        }

        static void SetWrap(Assimp.TextureWrapMode U, Assimp.TextureWrapMode V, Texture texture)
        {
            if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(DevoidGPU.TextureWrapMode.Repeat, DevoidGPU.TextureWrapMode.Repeat);
            }
            else if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Clamp)
            {
                texture.SetWrapMode(DevoidGPU.TextureWrapMode.Repeat, DevoidGPU.TextureWrapMode.ClampToEdge);
            }
            else if (U == Assimp.TextureWrapMode.Clamp && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(DevoidGPU.TextureWrapMode.ClampToEdge, DevoidGPU.TextureWrapMode.Repeat);
            }
        }

        static Dictionary<string, Texture2D> textureCache = new();
        public static Texture2D GetTexture(Assimp.Material mat, Assimp.TextureType type, string basePath)
        {
            if (mat.GetMaterialTextureCount(type) == 0)
                return null;

            mat.GetMaterialTexture(type, 0, out TextureSlot slot);

            if (!Path.IsPathFullyQualified(basePath))
                return null;

            string modelDirectory = Path.GetDirectoryName(basePath);
            string fullPath = Path.Combine(modelDirectory, slot.FilePath);

            // Normalize path (important for cache hits)
            fullPath = Path.GetFullPath(fullPath);

            if (textureCache.TryGetValue(fullPath, out var cached))
                return cached;

            Texture2D tex;

            if (type == Assimp.TextureType.Normals)
            {
                tex = Helper.LoadNormalMap(fullPath, TextureFilter.Linear);
            }
            else if (type != Assimp.TextureType.Diffuse)
            {
                tex = Helper.LoadImageAsDataTex(fullPath, TextureFilter.Linear);
            }
            else
            {
                tex = Helper.LoadImageAsTex(fullPath, TextureFilter.Linear);
            }

            SetWrap(slot.WrapModeU, slot.WrapModeV, tex);


            textureCache[fullPath] = tex;

            return tex;
        }

        public static Assimp.Material GetMaterial(Node node, Assimp.Scene scene)
        {
            if (node.MeshIndices.Count == 0)
                return null;

            var mesh = scene.Meshes[node.MeshIndices[0]];

            if (mesh.MaterialIndex < 0 || mesh.MaterialIndex >= scene.MaterialCount)
                return null;

            return scene.Materials[mesh.MaterialIndex];
        }

        public static void ClearCaches()
        {
            meshCache.Clear();
            materialCache.Clear();
            textureCache.Clear();
        }

        public static void ApplyTransform(GameObject go, Node node)
        {
            Matrix4x4 matrix = Matrix4x4.Transpose(node.Transform);

            Matrix4x4.Decompose(matrix,
                out Vector3 scale,
                out Quaternion rotation,
                out Vector3 translation);

            go.Transform.Position = translation;
            go.Transform.Rotation = rotation;
            go.Transform.Scale = scale;
        }
    }
}