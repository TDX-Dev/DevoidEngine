namespace DevoidGPU
{
    public interface IShaderStorageBuffer
    {
        BufferUsage Usage { get; }

        int SizeInBytes { get; }
        void SetData(ReadOnlySpan<byte> data);
        void UpdatePartial(ReadOnlySpan<byte> data, int byteOffset);
        void Bind(int slot, ShaderStage stages);
        void UnBind(int slot, ShaderStage stages);
        void BindMutable(int slot);
        void UnBindMutable(int slot);
    }
}