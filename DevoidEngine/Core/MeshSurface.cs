using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Core
{
    public sealed class MeshSurface : IDisposable
    {
        public MaterialInstance? Material { get; set; }

        public BoundingBox LocalBounds { get; private set; }
        public VertexInfo VertexInfo { get; private set; } = Vertex.VertexInfo;

        private IndexBuffer? IB;
        private VertexBuffer<Vertex>? VB;

        public Vector3[]? Positions;
        public Vector2[]? UVs;
        public Vector3[]? Normals;
        public Vector4[]? Tangents;
        public uint[]? Indices;

        public MeshSurface()
        {
            Positions = [];
            Normals = [];
            UVs = [];
            Tangents = [];
        }

        public MeshSurface(Vector3[] positions, Vector3[]? normals, Vector2[]? uvs, Vector4[]? tangents, uint[]? indices)
        {
            Positions = positions;
            Normals = normals;
            UVs = uvs;
            Tangents = tangents;
            Indices = indices;
        }

        public void SetGeometry(Vector3[] positions, Vector3[]? normals, Vector2[]? uvs, Vector4[]? tangents, uint[]? indices)
        {
            this.Positions = positions;
            this.Normals = normals;
            this.UVs = uvs;
            this.Tangents = tangents;
            this.Indices = indices;
        }

        public void Upload(bool computeLocalBounds = true)
        {
            if (Positions == null || Positions.Length == 0)
                throw new InvalidOperationException("Surface must have positions");

            int count = Positions.Length;

            Vertex[] vertices = new Vertex[count];

            for (int i = 0; i < count; i++)
            {
                Vector3 position = Positions[i];

                Vector3 normal = Normals != null && Normals.Length == count
                    ? Normals[i]
                    : Vector3.UnitY;

                Vector2 uv = UVs != null && UVs.Length == count
                    ? UVs[i]
                    : Vector2.Zero;

                Vector4 tangent = Tangents != null && Tangents.Length == count
                    ? Tangents[i]
                    : new Vector4(1, 0, 0, 1);

                vertices[i] = new Vertex(position, normal, uv, tangent);
            }

            if (computeLocalBounds)
                ComputeLocalBounds();

            if (VB == null)
            {
                VB = new VertexBuffer<Vertex>(
                    Engine.GraphicsDevice,
                    vertices.AsSpan(),
                    Vertex.VertexInfo,
                    ResourceUsage.Default);
            }
            else
            {
                VB.Update(vertices);
            }

            if (Indices != null && Indices.Length > 0)
            {
                if (IB == null)
                {
                    IB = new IndexBuffer(
                        Engine.GraphicsDevice,
                        Indices.AsSpan(),
                        ResourceUsage.Default);
                }
                else
                {
                    IB.Update(Indices);
                }
            }

            Normals = null;
            Tangents = null;
            UVs = null;
        }

        private void ComputeLocalBounds()
        {
            if (Positions == null || Positions.Length == 0)
            {
                LocalBounds = BoundingBox.Empty;
                return;
            }

            Vector3 min = Positions[0];
            Vector3 max = Positions[0];

            for (int i = 1; i < Positions.Length; i++)
            {
                min = Vector3.Min(min, Positions[i]);
                max = Vector3.Max(max, Positions[i]);
            }

            LocalBounds = new BoundingBox(min, max);
        }

        public void Draw(ICommandList cmd)
        {
            if (VB == null)
                return;

            cmd.SetVertexBuffer(VB.GPU);

            if (IB != null)
            {
                cmd.SetIndexBuffer(IB.GPU);
                cmd.DrawIndexed(IB.Count, 0, 0);
            }
            else
            {
                cmd.Draw(VB.Count, 0);
            }
        }

        public void DrawInstanced(ICommandList cmd, int instanceCount)
        {
            if (VB == null || IB == null)
                return;

            cmd.SetVertexBuffer(VB.GPU);
            cmd.SetIndexBuffer(IB.GPU);

            cmd.DrawInstancedIndexed(
                IB.Count,
                instanceCount,
                0,
                0,
                0);
        }

        public void Dispose()
        {
            VB?.Dispose();
            IB?.Dispose();

            VB = null;
            IB = null;

            Positions = null;
            Normals = null;
            UVs = null;
            Tangents = null;
            Indices = null;
        }
    }
}
