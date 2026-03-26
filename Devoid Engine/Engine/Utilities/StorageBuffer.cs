using DevoidEngine.Engine.Rendering;
using DevoidGPU;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Utilities
{
    public sealed class StorageBuffer<T> where T : struct
    {
        private readonly IShaderStorageBuffer _buffer;

        public int ElementCapacity { get; }
        public int ElementSize { get; }
        public int SizeInBytes => _buffer.SizeInBytes;

        public StorageBuffer(int elementCapacity,
                             BufferUsage usage = BufferUsage.Dynamic,
                             bool allowRW = false)
        {
            ElementCapacity = elementCapacity;
            ElementSize = Marshal.SizeOf<T>();

            _buffer = Renderer.GraphicsDevice
                .BufferFactory
                .CreateShaderStorageBuffer(
                    elementCapacity * ElementSize,
                    ElementSize,
                    usage,
                    allowRW);
        }

        // Full update
        public void SetData(ReadOnlySpan<T> data)
        {
            var bytes = MemoryMarshal.AsBytes(data);
            _buffer.SetData(bytes);
        }

        // Partial update by element offset
        public void SetData(ReadOnlySpan<T> data, int elementOffset)
        {
            var bytes = MemoryMarshal.AsBytes(data);
            int byteOffset = elementOffset * ElementSize;

            _buffer.UpdatePartial(bytes, byteOffset);
        }

        // List overload (no allocation)
        public void SetData(List<T> data, int count, int elementOffset = 0)
        {
            var span = CollectionsMarshal.AsSpan(data).Slice(0, count);
            SetData(span, elementOffset);
        }

        public void Bind(int slot = 0, ShaderStage stages = ShaderStage.Fragment)
        {
            _buffer.Bind(slot, stages);
        }

        public void BindMutable(int slot = 0)
        {
            _buffer.BindMutable(slot);
        }

        public void UnBindMutable(int slot = 0)
        {
            _buffer.UnBindMutable(slot);
        }

        public void UnBind(int slot = 0, ShaderStage stages = ShaderStage.All)
        {
            _buffer.UnBind(slot, stages);
        }
    }
}