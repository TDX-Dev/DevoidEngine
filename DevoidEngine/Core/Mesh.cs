using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class Mesh
    {
        public Vector3[]? Positions { get => positions; set => positions = value; }
        public Vector2[]? UVs { get => uvs; set => uvs = value; }
        public Vector3[]? Normals { get => normals; set => normals = value; }
        public Vector4[]? Tangents { get => tangents; set => tangents = value; }

        public uint[]? Indices { get => indices; set => indices = value; }

        private IndexBuffer? IB;
        private VertexBuffer<Vertex>? VB;
        //private readonly VertexBuffer<Vertex>? VB_Skinned;

        private Vector3[]? positions;
        private Vector2[]? uvs;
        private Vector3[]? normals;
        private Vector4[]? tangents;
        private uint[]? indices;

        public Mesh()
        {
            positions = [];
            normals = [];
            uvs = [];
            tangents = [];
        }

        public void SetVertices(Vector3[] positions)
        {
            Positions = positions;
        }

        public void Upload()
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

                //var tangent = (Tangents != null && Tangents.Length == count)
                //    ? Tangents[i]
                //    : new Vector4(1, 0, 0, 1); // safe default

                vertices[i] = new Vertex(pos, normal, uv);
            }

            VB = new VertexBuffer<Vertex>(Engine.GraphicsDevice, vertices.AsSpan(), Vertex.VertexInfo, ResourceUsage.Default);
            if (indices != null && indices.Length > 0)
            {
                IB = new IndexBuffer(Engine.GraphicsDevice, indices.AsSpan());
            }
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
    }
}
