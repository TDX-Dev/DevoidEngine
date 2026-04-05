using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using DevoidGPU;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class ModelLoader : IAssetLoader<Model>
    {
        public Model Load(ReadOnlySpan<byte> data)
        {
            var asset = MessagePackSerializer.Deserialize<ModelAsset>(data.ToArray());

            Model model = new();

            model.Nodes = asset.Nodes;

            model.Meshes = new Mesh[asset.Meshes.Length];
            model.Materials = new Material[asset.Materials.Length];
            model.MeshMaterialIndices = new int[asset.Meshes.Length];

            for (int i = 0; i < asset.Meshes.Length; i++)
            {
                var meshAsset = asset.Meshes[i];

                var mesh = BuildMesh(meshAsset);

                mesh.Guid = asset.MeshGuids[i];

                model.Meshes[i] = mesh;

                model.MeshMaterialIndices[i] = meshAsset.MaterialIndex;
            }

            for (int i = 0; i < asset.Materials.Length; i++)
            {
                var mat = BuildMaterial(asset.Materials[i]);

                mat.Guid = asset.MaterialGuids[i];

                model.Materials[i] = mat;
            }

            return model;
        }

        Material BuildMaterial(MaterialAsset asset)
        {
            Shader shader = ShaderLibrary.GetShader(asset.Shader)
                ?? ShaderLibrary.GetShader("PBR/ForwardPBR")!;

            Material material = new Material(shader);

            foreach (var (name, guid) in asset.Textures)
            {
                Texture2D? tex = AssetManager.Load<Texture2D>(guid);
                if (tex != null)
                    material.SetTexture(name, tex);
            }
            foreach (var (name, value) in asset.Ints)
                material.SetInt(name, value);

            foreach (var (name, value) in asset.Floats)
                material.SetFloat(name, value);

            foreach (var (name, value) in asset.Vector2s)
                material.SetVector2(name, value);

            foreach (var (name, value) in asset.Vector3s)
                material.SetVector3(name, value);

            foreach (var (name, value) in asset.Vector4s)
                material.SetVector4(name, value);

            foreach (var (name, value) in asset.Matrices)
                material.SetMatrix4x4(name, value);

            return material;
        }

        Mesh BuildMesh(MeshAsset data)
        {
            Mesh mesh = new Mesh();
            int numElemPerVertex = 3;
            int numElemPerUV = 2;
            int numElemPerNormal = 3;

            int vertexCount = data.Positions.Length / numElemPerVertex;

            Vertex[] vertex = new Vertex[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 position = new Vector3(
                    data.Positions[i * numElemPerVertex],
                    data.Positions[i * numElemPerVertex + 1],
                    data.Positions[i * numElemPerVertex + 2]);

                Vector2 uv = new Vector2(
                    data.UVs[i * numElemPerUV],
                    data.UVs[i * numElemPerUV + 1]);

                Vector3 normal = new Vector3(
                    data.Normals[i * numElemPerNormal],
                    data.Normals[i * numElemPerNormal + 1],
                    data.Normals[i * numElemPerNormal + 2]);

                Vector3 tangent = new Vector3(
                    data.Tangents[i * numElemPerNormal],
                    data.Tangents[i * numElemPerNormal + 1],
                    data.Tangents[i * numElemPerNormal + 2]);

                Vector3 bitangent = new Vector3(
                    data.Bitangents[i * numElemPerNormal],
                    data.Bitangents[i * numElemPerNormal + 1],
                    data.Bitangents[i * numElemPerNormal + 2]);

                vertex[i] = new Vertex(position, normal, uv, tangent, bitangent);
            }

            mesh.SetVertices(vertex);

            int[] indices = new int[data.Indices.Length];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = (int)data.Indices[i];

            mesh.SetIndices(indices);

            return mesh;
        }
    }
}
