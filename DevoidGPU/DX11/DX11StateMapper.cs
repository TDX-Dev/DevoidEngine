using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    // A set of helper functions to convert from RHI representation to
    // Internal DX11 representation.
    public static class DX11StateMapper
    {
        internal static Format ToDXGITextureFormat(TextureFormat textureFormat)
        {
            return textureFormat switch
            {
                TextureFormat.RGBA8_UNorm => Format.R8G8B8A8_UNorm,
                TextureFormat.RGBA8_UNorm_SRGB => Format.R8G8B8A8_UNorm_SRgb,
                TextureFormat.BGRA8_UNorm => Format.B8G8R8A8_UNorm,
                TextureFormat.RGBA16_Float => Format.R16G16B16A16_Float,
                TextureFormat.RGBA32_Float => Format.R32G32B32A32_Float,

                TextureFormat.RG16_Float => Format.R16G16_Float,

                TextureFormat.R16_Float => Format.R16_Float,
                TextureFormat.R32_Float => Format.R32_Float,

                TextureFormat.R8_UInt => Format.R8_UInt,
                TextureFormat.R8_UNorm => Format.R8_UNorm,

                TextureFormat.Depth24_Stencil8 => Format.D24_UNorm_S8_UInt,
                TextureFormat.Depth32_Float => Format.D32_Float,
                _ => Format.Unknown
            };
        }
        internal static TextureFormat ToTextureFormat(Format format)
        {
            return format switch
            {
                Format.R8G8B8A8_UNorm => TextureFormat.RGBA8_UNorm,
                Format.R8G8B8A8_UNorm_SRgb => TextureFormat.RGBA8_UNorm_SRGB,
                Format.B8G8R8A8_UNorm => TextureFormat.BGRA8_UNorm,
                Format.R16G16B16A16_Float => TextureFormat.RGBA16_Float,
                Format.R32G32B32A32_Float => TextureFormat.RGBA32_Float,

                Format.R16G16_Float => TextureFormat.RG16_Float,

                Format.R16_Float => TextureFormat.R16_Float,
                Format.R32_Float => TextureFormat.R32_Float,

                Format.R8_UInt => TextureFormat.R8_UInt,
                Format.R8_UNorm => TextureFormat.R8_UNorm,

                Format.D24_UNorm_S8_UInt => TextureFormat.Depth24_Stencil8,
                Format.D32_Float => TextureFormat.Depth32_Float,

                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported format: {format}")
            };
        }
        internal static Format ResolveResourceFormat(TextureDescription desc)
        {
            var dx11Format = ToDXGITextureFormat(desc.Format);

            if (!desc.Usage.HasFlag(TextureUsage.DepthStencil))
                return dx11Format;

            if (dx11Format == Format.D24_UNorm_S8_UInt)
                return Format.R24G8_Typeless;

            if (dx11Format == Format.D32_Float)
                return Format.R32_Typeless;

            return dx11Format;
        }
        internal static BindFlags ConvertBindFlags(TextureUsage usage)
        {
            BindFlags flags = 0;

            if (usage.HasFlag(TextureUsage.ShaderResource))
                flags |= BindFlags.ShaderResource;

            if (usage.HasFlag(TextureUsage.RenderTarget))
                flags |= BindFlags.RenderTarget;

            if (usage.HasFlag(TextureUsage.DepthStencil))
                flags |= BindFlags.DepthStencil;

            if (usage.HasFlag(TextureUsage.UnorderedAccess))
                flags |= BindFlags.UnorderedAccess;

            return flags;
        }
        internal static ResourceOptionFlags ResolveTextureOptionFlags(TextureDescription description)
        {
            if (description.Dimension == TextureDimension.TextureCube)
                return ResourceOptionFlags.TextureCube;

            return ResourceOptionFlags.None;
        }
        internal static ShaderResourceViewDimension ResolveSRVDimension(TextureDescription description)
        {
            switch (description.Dimension)
            {
                case TextureDimension.Texture3D:
                    return ShaderResourceViewDimension.Texture3D;

                case TextureDimension.TextureCube:
                    return ShaderResourceViewDimension.TextureCube;

                default:
                    if (description.ArraySize > 1)
                        return ShaderResourceViewDimension.Texture2DArray;

                    return ShaderResourceViewDimension.Texture2D;
            }
        }
        internal static int BytesPerComponent(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.RGBA8_UNorm => 1,
                TextureFormat.RGBA8_UNorm_SRGB => 1,
                TextureFormat.BGRA8_UNorm => 1,
                TextureFormat.RG16_Float => 2,
                TextureFormat.RGBA16_Float => 2,
                TextureFormat.RGBA32_Float => 4,
                
                TextureFormat.R16_Float => 2,
                TextureFormat.R32_Float => 4,

                TextureFormat.R8_UInt => 1,
                TextureFormat.R8_UNorm => 1,

                TextureFormat.Depth24_Stencil8 => 4, // 24 bits depth + 8 bits stencil = 4 bytes
                TextureFormat.Depth32_Float => 4,
                _ => throw new NotSupportedException($"Unsupported texture format: {format}")
            };
        }
        internal static ShaderResourceType ConvertResourceType(ShaderInputType type)
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
        internal static ShaderVariableType ConvertResourceType(string type)
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
        //internal static SharpDX.D3DCompiler.ShaderStage ToDXShaderStage(ShaderStage func)
        //{
        //    return func switch
        //    {
        //        ShaderStage.Vertex => ShaderStage.Vertex,
        //        ShaderStage.Fragment => ShaderStage.Fragment,
        //        ShaderStage.Geometry => ShaderStage.Geometry,
        //        ShaderStage.Compute => ShaderStage.Compute,
        //        _ => throw new NotImplementedException()
        //    };
        //}
    }
}
