using DevoidEngine.Assets;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Core
{
    public sealed class Mesh : AssetType
    {
        public ResourceUsage Usage { get; }

        public bool HasBones => false;

        public VertexInfo VertexInfo => HasBones ? Vertex.VertexInfo : Vertex.VertexInfo;

        public Vector3[]? Positions { get => positions; set => positions = value; }
        public Vector2[]? UVs { get => uvs; set => uvs = value; }
        public Vector3[]? Normals { get => normals; set => normals = value; }
        public Vector4[]? Tangents { get => tangents; set => tangents = value; }

        public uint[]? Indices { get => indices; set => indices = value; }

        public BoundingBox LocalBounds { get; private set; } = null!;

        private IndexBuffer? IB;
        private VertexBuffer<Vertex>? VB;
        //private readonly VertexBuffer<Vertex>? VB_Skinned;

        private Vector3[]? positions;
        private Vector2[]? uvs;
        private Vector3[]? normals;
        private Vector4[]? tangents;
        private uint[]? indices;

        public Mesh(ResourceUsage usage = ResourceUsage.Default)
        {
            Usage = usage;
            positions = [];
            normals = [];
            uvs = [];
            tangents = [];
        }

        public void SetVertices(Vector3[] positions)
        {
            Positions = positions;
        }

        public void Upload(bool computeLocalBounds = true)
        {
            if (Positions == null || Positions.Length == 0)
                throw new InvalidOperationException("Mesh must have positions");


            int count = Positions.Length;

            Vertex[] vertices = new Vertex[count];

            for (int i = 0; i < count; i++)
            {
                var pos = Positions[i];

                var normal = (Normals != null && Normals.Length == count)
                    ? Normals[i]
                    : Vector3.UnitY;

                var uv = (UVs != null && UVs.Length == count)
                    ? UVs[i]
                    : Vector2.Zero;

                var tangent = (Tangents != null && Tangents.Length == count)
                    ? Tangents[i]
                    : new Vector4(1, 0, 0, 1); // safe default

                vertices[i] = new Vertex(pos, normal, uv, tangent);
            }

            if (computeLocalBounds)
                ComputeLocalBounds();

            if (VB == null)
            {
                VB = new VertexBuffer<Vertex>(Engine.GraphicsDevice, vertices.AsSpan(), Vertex.VertexInfo, Usage);
            }
            else
            {
                VB.Update(vertices);
            }

            if (indices != null && indices.Length > 0)
            {
                if (IB == null)
                {
                    IB = new IndexBuffer(Engine.GraphicsDevice, indices.AsSpan(), Usage);
                }
                else
                {
                    IB.Update(indices);
                }
            }

            //Positions = null;
            Normals = null;
            Tangents = null;
            UVs = null;
            //Indices = null;
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

        public override void Dispose()
        {
            VB?.Dispose();
            IB?.Dispose();
            Positions = [];
            Normals = [];
            UVs = [];
            Tangents = [];
            Indices = [];
        }
    }
}
