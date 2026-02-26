using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Core
{
    public class Mesh : IDisposable
    {
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }

        private Vertex[] vertices;
        private int[] indices;

        //public Material Material { get; set; }

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }

        public int VertexCount { get => VertexBuffer.VertexCount; }

        // State
        public bool IsRenderable { get; set; } = true;
        public bool IsHighlighted { get; set; } = false;
        public bool IsStatic { get; private set; } = true;
        public BoundingBox LocalBounds { get; private set; } = BoundingBox.Empty;

        private bool isDisposed = false;

        // Constructors
        public Mesh() { }

        ~Mesh()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;

            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();

            isDisposed = true;
            GC.SuppressFinalize(this);
        }


        public void SetStatic(bool isStatic) => IsStatic = isStatic;

        public void Bind()
        {
            VertexBuffer?.Bind();
            IndexBuffer?.Bind();
        }

        public void Draw()
        {
            if (IndexBuffer  == null)
            {
                Renderer.graphicsDevice.Draw(vertices.Length, 0);
            } else
            {
                Renderer.graphicsDevice.DrawIndexed(IndexBuffer.IndexCount, 0, 0);
            }
        }

        public void SetVertices(Vertex[] vertexArray)
        {
            vertices = vertexArray;
            ComputeLocalBounds(vertexArray);


            VertexBuffer = new VertexBuffer(
                IsStatic ? BufferUsage.Default : BufferUsage.Dynamic,
                Vertex.VertexInfo,
                vertexArray.Length
            );

            //Renderer.graphicsDevice.BufferFactory.CreateVertexBuffer(
            //    IsStatic ? BufferUsage.Default : BufferUsage.Dynamic,
            //    Vertex.VertexInfo,
            //    vertexArray.Length
            //);

            VertexBuffer.SetData(vertexArray);
        }

        public void SetIndices(int[] indexArray)
        {
            indices = indexArray;
            IndexBuffer = new IndexBuffer(BufferUsage.Default, indexArray.Length);
            IndexBuffer.SetData(indexArray);
        }

        private void ComputeLocalBounds(Vertex[] verts)
        {
            if (verts == null || verts.Length == 0)
            {
                LocalBounds = BoundingBox.Empty;
                return;
            }

            Vector3 min = verts[0].Position;
            Vector3 max = verts[0].Position;

            foreach (var v in verts)
            {
                min = Vector3.Min(min, v.Position);
                max = Vector3.Max(max, v.Position);
            }

            LocalBounds = new BoundingBox(min, max);
        }

        // Getters
        public Vertex[] GetVertices() => vertices;
        public int[] GetIndices() => indices;
    }
}
