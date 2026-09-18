using DevoidEngine.Assets;
using DevoidEngine.Core;
using MessagePack;
using System.Numerics;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal sealed class MeshLoader : IAssetLoader<Mesh>
    {
        public string RuntimeExtension => "mesh";

        public Mesh Load(byte[] data)
        {
            MeshAsset asset;

            try
            {
                asset = MessagePackSerializer.Deserialize<MeshAsset>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Mesh Loader]: Error Loading Mesh {e.Message}");
                throw;
            }

            Mesh mesh = new();

            MeshSurface[] surfaces = new MeshSurface[asset.Surfaces.Length];

            for (int surfaceIndex = 0; surfaceIndex < asset.Surfaces.Length; surfaceIndex++)
            {
                MeshSurfaceAsset assetSurface = asset.Surfaces[surfaceIndex];

                int vertexCount = assetSurface.Positions.Length / 3;

                Vector3[] positions = new Vector3[vertexCount];
                Vector3[] normals = new Vector3[vertexCount];
                Vector2[] uvs = new Vector2[vertexCount];
                Vector4[] tangents = new Vector4[vertexCount];

                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    int positionIndex = vertexIndex * 3;

                    positions[vertexIndex] = new Vector3(
                        assetSurface.Positions[positionIndex],
                        assetSurface.Positions[positionIndex + 1],
                        assetSurface.Positions[positionIndex + 2]);

                    if (assetSurface.Normals.Length >= positionIndex + 3)
                    {
                        normals[vertexIndex] = new Vector3(
                            assetSurface.Normals[positionIndex],
                            assetSurface.Normals[positionIndex + 1],
                            assetSurface.Normals[positionIndex + 2]);
                    }

                    int uvIndex = vertexIndex * 2;

                    if (assetSurface.UVs.Length >= uvIndex + 2)
                    {
                        uvs[vertexIndex] = new Vector2(
                            assetSurface.UVs[uvIndex],
                            assetSurface.UVs[uvIndex + 1]);
                    }

                    if (assetSurface.Tangents.Length >= positionIndex + 3)
                    {
                        Vector3 tangent = new(
                            assetSurface.Tangents[positionIndex],
                            assetSurface.Tangents[positionIndex + 1],
                            assetSurface.Tangents[positionIndex + 2]);

                        Vector3 bitangent = Vector3.UnitY;

                        if (assetSurface.Bitangents.Length >= positionIndex + 3)
                        {
                            bitangent = new Vector3(
                                assetSurface.Bitangents[positionIndex],
                                assetSurface.Bitangents[positionIndex + 1],
                                assetSurface.Bitangents[positionIndex + 2]);
                        }

                        float handedness =
                            Vector3.Dot(
                                Vector3.Cross(
                                    normals[vertexIndex],
                                    tangent),
                                bitangent) < 0f
                                    ? -1f
                                    : 1f;

                        tangents[vertexIndex] = new Vector4(
                            tangent,
                            handedness);
                    }
                }

                MeshSurface surface = new();

                surface.SetGeometry(positions, normals, uvs, tangents, assetSurface.Indices);
                surfaces[surfaceIndex] = surface;
            }

            mesh.Surfaces = surfaces;
            mesh.Upload();

            return mesh;
        }
    }
}
