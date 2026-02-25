using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    public class DX11TextureFormat
    {
        internal static Format ToDXGIFormat(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.RGBA8_UNorm => Format.R8G8B8A8_UNorm,
                TextureFormat.RGBA8_UNorm_SRGB => Format.R8G8B8A8_UNorm_SRgb,
                TextureFormat.BGRA8_UNorm => Format.B8G8R8A8_UNorm,
                TextureFormat.RGBA16_Float => Format.R16G16B16A16_Float,
                TextureFormat.RGBA32_Float => Format.R32G32B32A32_Float,

                TextureFormat.R8_UInt => Format.R8_UInt,
                TextureFormat.R8_UNorm => Format.R8_UNorm,


                TextureFormat.Depth24_Stencil8 => Format.D24_UNorm_S8_UInt,
                TextureFormat.Depth32_Float => Format.D32_Float,
                _ => Format.Unknown
            };
        }

        public static TextureFormat DXGIFormatToTextureFormat(SharpDX.DXGI.Format format)
        {
            return format switch
            {
                SharpDX.DXGI.Format.R8G8B8A8_UNorm => TextureFormat.RGBA8_UNorm,
                SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb => TextureFormat.RGBA8_UNorm_SRGB,
                SharpDX.DXGI.Format.B8G8R8A8_UNorm => TextureFormat.BGRA8_UNorm,

                SharpDX.DXGI.Format.R16G16_Float => TextureFormat.RG16_Float,
                SharpDX.DXGI.Format.R16G16B16A16_Float => TextureFormat.RGBA16_Float,
                SharpDX.DXGI.Format.R32G32B32A32_Float => TextureFormat.RGBA32_Float,

                SharpDX.DXGI.Format.R8_UInt => TextureFormat.R8_UInt,
                SharpDX.DXGI.Format.R8_UNorm => TextureFormat.R8_UNorm,


                SharpDX.DXGI.Format.D24_UNorm_S8_UInt => TextureFormat.Depth24_Stencil8,
                SharpDX.DXGI.Format.D32_Float => TextureFormat.Depth32_Float,

                _ => TextureFormat.Unknown
            };
        }

        internal static int FormatToComponentCount(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.RGBA8_UNorm => 4,
                TextureFormat.RGBA8_UNorm_SRGB => 4,
                TextureFormat.BGRA8_UNorm => 4,
                TextureFormat.RG16_Float => 2,
                TextureFormat.RGBA16_Float => 4,
                TextureFormat.RGBA32_Float => 4,
                TextureFormat.R8_UInt => 1,
                TextureFormat.R8_UNorm => 1,
                TextureFormat.Depth24_Stencil8 => 2, // depth + stencil
                TextureFormat.Depth32_Float => 1,
                _ => throw new NotSupportedException($"Unsupported texture format: {format}")
            };
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

                TextureFormat.R8_UInt => 1,
                TextureFormat.R8_UNorm => 1,

                TextureFormat.Depth24_Stencil8 => 4, // 24 bits depth + 8 bits stencil = 4 bytes
                TextureFormat.Depth32_Float => 4,
                _ => throw new NotSupportedException($"Unsupported texture format: {format}")
            };
        }

        internal static int RowPitch(TextureFormat format, int width)
        {
            return FormatToComponentCount(format) * BytesPerComponent(format) * width;
        }


    }
}
