namespace DevoidGPU
{
    public readonly struct MappedTexture
    {
        public readonly IntPtr Data;
        public readonly int RowPitch;
        public readonly int DepthPitch;

        public MappedTexture(
            IntPtr data,
            int rowPitch,
            int depthPitch)
        {
            Data = data;
            RowPitch = rowPitch;
            DepthPitch = depthPitch;
        }
    }
}
