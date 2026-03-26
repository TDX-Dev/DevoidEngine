namespace DevoidGPU
{
    static class Utils
    {
        public static int GetTypeSize(VertexAttribType type)
        {
            switch (type)
            {
                case VertexAttribType.Float: return sizeof(float);       // 4
                case VertexAttribType.Int: return sizeof(int);         // 4
                case VertexAttribType.UnsignedByte: return sizeof(byte);        // 1
                default: throw new NotSupportedException($"Unsupported attrib type {type}");
            }
        }
    }
}