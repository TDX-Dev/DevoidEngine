using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System;
using System.Diagnostics;

public class UniformBuffer : IDisposable
{
    private readonly IUniformBuffer _buffer;
    private bool _disposed;

    public int SizeInBytes => _buffer.SizeInBytes;

    public UniformBuffer(int sizeInBytes, BufferUsage usage = BufferUsage.Dynamic)
    {
        _buffer = Renderer.GraphicsDevice
            .BufferFactory
            .CreateUniformBuffer(sizeInBytes, usage);

        GPUTracker.BufferCount++;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UniformBuffer));
    }

    public void SetData<T>(T data) where T : struct
    {
        ThrowIfDisposed();
        _buffer.SetData(data);
    }

    public void SetData(byte[] data)
    {
        ThrowIfDisposed();
        _buffer.SetData(data);
    }

    public void SetData(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed();
        _buffer.SetData(data);
    }

    public void SetData(IntPtr ptr, int size)
    {
        ThrowIfDisposed();
        _buffer.SetData(ptr, size);
    }

    public void Bind(int slot = 0, ShaderStage stage = ShaderStage.Fragment)
    {
        ThrowIfDisposed();
        _buffer.Bind(slot, stage);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _buffer.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);

        GPUTracker.BufferCount--;
    }

    ~UniformBuffer()
    {
        Dispose();
    }
}