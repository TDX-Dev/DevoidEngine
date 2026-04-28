namespace DevoidGPU
{
    public static class Utils
    {
        public static int GetVertexTypeSize(VertexAttribType type)
        {
            return type switch
            {
                VertexAttribType.Float => sizeof(float),// 4
                VertexAttribType.Int => sizeof(int),// 4
                VertexAttribType.UnsignedByte => sizeof(byte),// 1
                _ => throw new NotSupportedException($"Unsupported attrib type {type}"),
            };
        }

    }
}
