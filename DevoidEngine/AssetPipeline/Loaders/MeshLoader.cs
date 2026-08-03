using DevoidEngine.Assets;
using DevoidEngine.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class MeshLoader : IAssetLoader<Mesh>
    {
        public string RuntimeExtension => "mesh";
        public Mesh Load(byte[] data)
        {
            MeshAsset asset;

            try
            {
                asset = MessagePackSerializer.Deserialize<MeshAsset>(data.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Mesh Loader]: Error Loading Mesh {e.Message}");
                throw;
            }

            Mesh mesh = new();

            int vertexCount = asset.Positions.Length / 3;

            mesh.Positions = new Vector3[vertexCount];
            mesh.Normals = new Vector3[vertexCount];
            mesh.UVs = new Vector2[vertexCount];
            mesh.Tangents = new Vector4[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                mesh.Positions[i] = new Vector3(
                    asset.Positions[i * 3 + 0],
                    asset.Positions[i * 3 + 1],
                    asset.Positions[i * 3 + 2]);

                if (asset.Normals.Length >= (i + 1) * 3)
                {
                    mesh.Normals[i] = new Vector3(
                        asset.Normals[i * 3 + 0],
                        asset.Normals[i * 3 + 1],
                        asset.Normals[i * 3 + 2]);
                }

                if (asset.UVs.Length >= (i + 1) * 2)
                {
                    mesh.UVs[i] = new Vector2(
                        asset.UVs[i * 2 + 0],
                        asset.UVs[i * 2 + 1]);
                }

                if (asset.Tangents.Length >= (i + 1) * 3)
                {

                    Vector3 tangent = new(
                        asset.Tangents[i * 3 + 0],
                        asset.Tangents[i * 3 + 1],
                        asset.Tangents[i * 3 + 2]);

                    Vector3 bitangent = Vector3.UnitY;

                    if (asset.Bitangents.Length >= (i + 1) * 3)
                    {
                        bitangent = new Vector3(
                            asset.Bitangents[i * 3 + 0],
                            asset.Bitangents[i * 3 + 1],
                            asset.Bitangents[i * 3 + 2]);
                    }

                    float handedness =
                        Vector3.Dot(
                            Vector3.Cross(mesh.Normals[i], tangent),
                            bitangent) < 0f ? -1f : 1f;

                    mesh.Tangents[i] = new Vector4(
                        tangent,
                        handedness);
                }
            }

            mesh.Indices = asset.Indices;

            mesh.Upload();

            return mesh;
        }
    }
}
