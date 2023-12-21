using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace EmberaEngine.Engine.Utilities
{
    public class VertexBuffer : IDisposable
    {
        public static readonly int MaxVertices = 1000000;
        public static readonly int MinVertices = 1;

        public readonly int VertexBufferObject;
        public readonly VertexInfo VertexInfo;
        public readonly int VertexCount;
        public readonly bool IsStatic;


        private bool isdisposed = false;
        private bool isinitialized = false;

        public VertexBuffer(VertexInfo vertexInfo, int vertexCount, bool isStatic = true)
        {
            this.isdisposed = false;
            this.isinitialized = true;

            this.VertexInfo = vertexInfo;
            this.VertexCount = vertexCount;
            this.IsStatic = isStatic;
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            if (!isStatic)
            {
                hint = BufferUsageHint.StreamDraw;
            }

            VertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexInfo.SizeInBytes * vertexCount, IntPtr.Zero, hint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public VertexBuffer(VertexInfo vertexInfo, Vertex[] data, bool isStatic = true)
        {
            this.isdisposed = false;

            this.VertexInfo = vertexInfo;
            this.VertexCount = data.Length;
            this.IsStatic = isStatic;
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            if (!isStatic)
            {
                hint = BufferUsageHint.StreamDraw;
            }

            VertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexInfo.SizeInBytes * data.Length, IntPtr.Zero, hint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            SetData(data, data.Length);

            if (isStatic) GL.DeleteBuffer(VertexBufferObject);
            GL.BindVertexArray(0);
        }

        public void SetData<T>(T[] data, int count) where T : struct
        {
            if (typeof(T) != VertexInfo.Type)
            {
                throw new ArgumentException();
            }

            if (data is null)
            {
                throw new ArgumentException();
            }

            if (data.Length <= 0)
            {
               throw new ArgumentException();
            }

            if (count <= 0 || count > this.VertexCount || count > data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, count * VertexInfo.SizeInBytes, data);
        }

        ~VertexBuffer()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (isdisposed)
            {
                return;
            }

            GraphicsObjectCollector.AddVBOToDispose(VertexBufferObject);

            this.isdisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
