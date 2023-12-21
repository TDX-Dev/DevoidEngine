using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace EmberaEngine.Engine.Utilities
{
    public class IndexBuffer : IDisposable
    {
        public static readonly int MaxIndexCount = 2500000;
        public static readonly int MinIndexCount = 1;

        private bool isdisposed = false;

        public readonly int IndexBufferObject;
        public readonly int IndexCount;
        public readonly bool IsStatic;

        public IndexBuffer(int indexCount, bool isStatic = true)
        {
            this.isdisposed = false;

            //if (indexCount > MaxIndexCount || indexCount < MinIndexCount)
            //{
            //    throw new ArgumentException();
            //}

            this.IndexCount = indexCount;
            this.IsStatic = isStatic;

            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            if (!isStatic)
            {
                hint = BufferUsageHint.StreamDraw;
            }

            this.IndexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.IndexCount * sizeof(int), IntPtr.Zero, hint);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
        }

        public void SetData(int[] data, int count)
        {
            if (data is null)
            {
                throw new ArgumentException();
            }

            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }

            if (count <= 0 || count > IndexCount || count > data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, count * sizeof(int), data);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        ~IndexBuffer()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (!this.isdisposed)
            {
                return;
            }
            this.isdisposed = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(this.IndexBufferObject);
            GC.SuppressFinalize(this);

        }

    }
}
