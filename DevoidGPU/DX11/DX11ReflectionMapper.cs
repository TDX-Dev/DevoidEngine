using SharpDX.D3DCompiler;

namespace DevoidGPU.DX11
{
    public static class DX11ReflectionMapper
    {
        public static ShaderResourceType ConvertResourceType(ShaderInputType type)
        {
            return type switch
            {
                ShaderInputType.Texture => ShaderResourceType.Texture2D,
                ShaderInputType.Sampler => ShaderResourceType.Sampler,
                ShaderInputType.Structured => ShaderResourceType.StructuredBuffer,
                ShaderInputType.UnorderedAccessViewRWStructured => ShaderResourceType.RWStructuredBuffer,
                _ => ShaderResourceType.Texture2D
            };
        }

        public static ShaderVariableType ConvertResourceType(string type)
        {
            return type switch
            {
                "float" => ShaderVariableType.Float,
                "int" => ShaderVariableType.Int,
                "float4" => ShaderVariableType.Vector4,
                "float3" => ShaderVariableType.Vector3,
                "float2" => ShaderVariableType.Vector2,
                "float3x3" => ShaderVariableType.Matrix3x3,
                "float4x4" => ShaderVariableType.Matrix4x4,
                _ => ShaderVariableType.Custom
            };
        }
    }
}