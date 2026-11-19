using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    // A set of helper functions to convert from RHI representation to
    // Internal DX11 representation.
    public static class Dx11StateMapper
    {
        internal static Format ToDxgiTextureFormat(TextureFormat textureFormat)
        {
            return textureFormat switch
            {
                TextureFormat.Rgba8UNorm => Format.R8G8B8A8_UNorm,
                TextureFormat.Rgba8UNormSrgb => Format.R8G8B8A8_UNorm_SRgb,
                TextureFormat.Bgra8UNorm => Format.B8G8R8A8_UNorm,
                TextureFormat.Rgba16Float => Format.R16G16B16A16_Float,
                TextureFormat.Rgba32Float => Format.R32G32B32A32_Float,

                TextureFormat.Rg16Float => Format.R16G16_Float,

                TextureFormat.Rg8UNorm => Format.R8G8_UNorm,

                TextureFormat.R16Float => Format.R16_Float,
                TextureFormat.R32Float => Format.R32_Float,

                TextureFormat.R32UInt => Format.R32_UInt,

                TextureFormat.R8UInt => Format.R8_UInt,
                TextureFormat.R8UNorm => Format.R8_UNorm,

                TextureFormat.Depth24Stencil8 => Format.D24_UNorm_S8_UInt,
                TextureFormat.Depth32Float => Format.D32_Float,


                TextureFormat.Rgba8SInt => Format.R8G8B8A8_SInt,
                TextureFormat.Rgba16SInt => Format.R16G16B16A16_SInt,
                TextureFormat.Rgba32SInt => Format.R32G32B32A32_SInt,

                TextureFormat.Rg8SInt => Format.R8G8_SInt,
                TextureFormat.Rg16SInt => Format.R16G16_SInt,
                TextureFormat.Rg32SInt => Format.R32G32_SInt,

                TextureFormat.R8SInt => Format.R8_SInt,
                TextureFormat.R16SInt => Format.R16_SInt,
                TextureFormat.R32SInt => Format.R32_SInt,



                _ => Format.Unknown
            };
        }



        internal static TextureFormat ToTextureFormat(Format format)
        {
            return format switch
            {
                Format.R8G8B8A8_UNorm => TextureFormat.Rgba8UNorm,
                Format.R8G8B8A8_UNorm_SRgb => TextureFormat.Rgba8UNormSrgb,
                Format.B8G8R8A8_UNorm => TextureFormat.Bgra8UNorm,
                Format.R16G16B16A16_Float => TextureFormat.Rgba16Float,
                Format.R32G32B32A32_Float => TextureFormat.Rgba32Float,

                Format.R16G16_Float => TextureFormat.Rg16Float,

                Format.R16_Float => TextureFormat.R16Float,
                Format.R32_Float => TextureFormat.R32Float,

                Format.R32_UInt => TextureFormat.R32UInt,

                Format.R8_UInt => TextureFormat.R8UInt,
                Format.R8_UNorm => TextureFormat.R8UNorm,

                Format.D24_UNorm_S8_UInt => TextureFormat.Depth24Stencil8,
                Format.D32_Float => TextureFormat.Depth32Float,

                Format.R8G8B8A8_SInt => TextureFormat.Rgba8SInt,
                Format.R16G16B16A16_SInt => TextureFormat.Rgba16SInt,
                Format.R32G32B32A32_SInt => TextureFormat.Rgba32SInt,

                Format.R8G8_SInt => TextureFormat.Rg8SInt,
                Format.R16G16_SInt => TextureFormat.Rg16SInt,
                Format.R32G32_SInt => TextureFormat.Rg32SInt,

                Format.R8_SInt => TextureFormat.R8SInt,
                Format.R16_SInt => TextureFormat.R16SInt,
                Format.R32_SInt => TextureFormat.R32SInt,

                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported format: {format}")
            };
        }
        internal static Format ResolveResourceFormat(TextureDescription desc)
        {
            var dx11Format = ToDxgiTextureFormat(desc.Format);

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
            ResourceOptionFlags flags = ResourceOptionFlags.None;

            if (description.Dimension == TextureDimension.TextureCube)
                flags |= ResourceOptionFlags.TextureCube;

            if (description.MipLevels > 1 &&
                description.Usage.HasFlag(TextureUsage.RenderTarget) &&
                description.Usage.HasFlag(TextureUsage.ShaderResource))
            {
                flags |= ResourceOptionFlags.GenerateMipMaps;
            }

            return flags;
        }
        internal static ShaderResourceViewDimension ResolveSrvDimension(TextureDescription description)
        {
            switch (description.Dimension)
            {
                case TextureDimension.Texture3D:
                    return ShaderResourceViewDimension.Texture3D;

                case TextureDimension.TextureCube:
                    return ShaderResourceViewDimension.TextureCube;

                default:
                    // MSAA 2D textures
                    if (description.Samples.Count > 1)
                    {
                        if (description.ArraySize > 1)
                            return ShaderResourceViewDimension.Texture2DMultisampledArray;

                        return ShaderResourceViewDimension.Texture2DMultisampled;
                    }

                    // Normal 2D textures
                    if (description.ArraySize > 1)
                        return ShaderResourceViewDimension.Texture2DArray;

                    return ShaderResourceViewDimension.Texture2D;
            }
        }
        internal static int BytesPerComponent(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.Rgba8UNorm => 1,
                TextureFormat.Rgba8UNormSrgb => 1,
                TextureFormat.Bgra8UNorm => 1,
                TextureFormat.Rg16Float => 2,
                TextureFormat.Rgba16Float => 2,
                TextureFormat.Rgba32Float => 4,

                TextureFormat.R16Float => 2,
                TextureFormat.R32Float => 4,

                TextureFormat.R32UInt => 4,

                TextureFormat.R8UInt => 1,
                TextureFormat.R8UNorm => 1,

                TextureFormat.Depth24Stencil8 => 4, // 24 bits depth + 8 bits stencil = 4 bytes
                TextureFormat.Depth32Float => 4,


                TextureFormat.R8SInt => 1,
                TextureFormat.Rg8SInt => 1,
                TextureFormat.Rgba8SInt => 1,
                TextureFormat.R16SInt => 2,
                TextureFormat.Rg16SInt => 2,
                TextureFormat.Rgba16SInt => 2,
                TextureFormat.R32SInt => 4,
                TextureFormat.Rg32SInt => 4,
                TextureFormat.Rgba32SInt => 4,

                _ => throw new NotSupportedException($"Unsupported texture format: {format}")
            };
        }

        internal static int BytesPerPixel(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.R8UNorm => 1,
                TextureFormat.R8UInt => 1,
                TextureFormat.R8SInt => 1,

                TextureFormat.R16Float => 2,
                TextureFormat.R32Float => 4,
                TextureFormat.R16SInt => 2,

                TextureFormat.R32UInt => 4,
                TextureFormat.R32SInt => 4,

                TextureFormat.Rg16Float => 4,
                TextureFormat.Rg8SInt => 2,

                TextureFormat.Rg8UNorm => 2,
                TextureFormat.Rg16SInt => 4,

                TextureFormat.Rg32SInt => 8,

                TextureFormat.Rgba8UNorm => 4,
                TextureFormat.Rgba8UNormSrgb => 4,
                TextureFormat.Bgra8UNorm => 4,
                TextureFormat.Rgba8SInt => 4,

                TextureFormat.Rgba16Float => 8,
                TextureFormat.Rgba16SInt => 8,

                TextureFormat.Rgba32Float => 16,
                TextureFormat.Rgba32SInt => 16,

                TextureFormat.Depth24Stencil8 => 4,
                TextureFormat.Depth32Float => 4,

                _ => throw new NotSupportedException(
                    $"Unsupported texture format: {format}")
            };
        }

        internal static ShaderResourceType ConvertResourceType(ShaderInputType type)
        {
            return type switch
            {
                ShaderInputType.Texture => ShaderResourceType.Texture2D,
                ShaderInputType.Sampler => ShaderResourceType.Sampler,
                ShaderInputType.Structured => ShaderResourceType.StructuredBuffer,
                ShaderInputType.UnorderedAccessViewRWStructured => ShaderResourceType.RwStructuredBuffer,
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
                "float3x3" => ShaderVariableType.Matrix3X3,
                "float4x4" => ShaderVariableType.Matrix4X4,
                _ => ShaderVariableType.Custom
            };
        }
        public static PrimitiveTopology ToDxPrimitiveType(PrimitiveType type) =>
            type switch
            {
                PrimitiveType.TriangleStrip => PrimitiveTopology.TriangleStrip,
                PrimitiveType.LineStrip => PrimitiveTopology.LineStrip,
                PrimitiveType.Lines => PrimitiveTopology.LineList,
                PrimitiveType.Triangles => PrimitiveTopology.TriangleList,
                _ => PrimitiveTopology.Undefined
            };
        public static BlendOption ToDxBlend(BlendFactor factor)
        {
            return factor switch
            {
                BlendFactor.Zero => BlendOption.Zero,
                BlendFactor.One => BlendOption.One,
                BlendFactor.SrcAlpha => BlendOption.SourceAlpha,
                BlendFactor.InvSrcAlpha => BlendOption.InverseSourceAlpha,
                BlendFactor.SrcColor => BlendOption.SourceColor,
                _ => BlendOption.One
            };
        }
        public static BlendOperation ToDxBlendOp(BlendOp op)
        {
            return op switch
            {
                BlendOp.Add => BlendOperation.Add,
                _ => BlendOperation.Add
            };
        }
        public static ColorWriteMaskFlags ToDxColorMask(ColorMask mask)
        {
            ColorWriteMaskFlags flags = 0;

            if ((mask & ColorMask.R) != 0) flags |= ColorWriteMaskFlags.Red;
            if ((mask & ColorMask.G) != 0) flags |= ColorWriteMaskFlags.Green;
            if ((mask & ColorMask.B) != 0) flags |= ColorWriteMaskFlags.Blue;
            if ((mask & ColorMask.A) != 0) flags |= ColorWriteMaskFlags.Alpha;

            return flags;
        }
        public static SharpDX.Direct3D11.FillMode ToDxFillMode(FillMode mode)
        {
            return mode switch
            {
                FillMode.Solid => SharpDX.Direct3D11.FillMode.Solid,
                FillMode.Wireframe => SharpDX.Direct3D11.FillMode.Wireframe,
                _ => SharpDX.Direct3D11.FillMode.Solid
            };
        }

        public static SharpDX.Direct3D11.CullMode ToDxCullMode(CullMode mode)
        {
            return mode switch
            {
                CullMode.None => SharpDX.Direct3D11.CullMode.None,
                CullMode.Front => SharpDX.Direct3D11.CullMode.Front,
                CullMode.Back => SharpDX.Direct3D11.CullMode.Back,
                _ => SharpDX.Direct3D11.CullMode.Back
            };
        }

        public static Comparison ToDxDepthComparison(CompareFunc func)
        {
            return func switch
            {
                CompareFunc.Never => Comparison.Never,
                CompareFunc.Less => Comparison.Less,
                CompareFunc.Equal => Comparison.Equal,
                CompareFunc.LessEqual => Comparison.LessEqual,
                CompareFunc.Greater => Comparison.Greater,
                CompareFunc.NotEqual => Comparison.NotEqual,
                CompareFunc.GreaterEqual => Comparison.GreaterEqual,
                CompareFunc.Always => Comparison.Always,
                _ => Comparison.LessEqual
            };
        }


        public static SharpDX.Direct3D11.ResourceUsage ToDxResourceUsage(ResourceUsage usage)
        {
            return usage switch
            {
                ResourceUsage.Default => SharpDX.Direct3D11.ResourceUsage.Default,

                ResourceUsage.Immutable => SharpDX.Direct3D11.ResourceUsage.Immutable,

                ResourceUsage.Dynamic => SharpDX.Direct3D11.ResourceUsage.Dynamic,

                ResourceUsage.Staging => SharpDX.Direct3D11.ResourceUsage.Staging,

                _ => throw new ArgumentOutOfRangeException(nameof(usage))
            };
        }
        public static CpuAccessFlags ToDxCpuAccess(CpuAccess access)
        {
            CpuAccessFlags flags = CpuAccessFlags.None;

            if (access.HasFlag(CpuAccess.Read))
                flags |= CpuAccessFlags.Read;

            if (access.HasFlag(CpuAccess.Write))
                flags |= CpuAccessFlags.Write;

            return flags;
        }
        public static InputElement[] CreateInputElements(VertexInfo vertexInfo)
        {
            InputElement[] elements = new InputElement[vertexInfo.VertexAttributes.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                VertexAttribute attr = vertexInfo.VertexAttributes[i];

                bool isInstance = attr.StepMode == VertexStepMode.Instance;

                elements[i] = new InputElement(
                    attr.Name,
                    attr.Index,
                    MapDxgiComponentCountToFormat(attr),
                    attr.Offset,
                    attr.Slot,
                    isInstance ? InputClassification.PerInstanceData : InputClassification.PerVertexData,
                    isInstance ? 1 : 0
                );
            }

            return elements;
        }
        public static Format MapDxgiComponentCountToFormat(VertexAttribute attr)
        {
            return attr.Type switch
            {
                VertexAttribType.Float => attr.ComponentCount switch
                {
                    1 => Format.R32_Float,
                    2 => Format.R32G32_Float,
                    3 => Format.R32G32B32_Float,
                    4 => Format.R32G32B32A32_Float,
                    _ => throw new ArgumentException("Unsupported float component count")
                },

                VertexAttribType.UnsignedByte => attr.ComponentCount switch
                {
                    4 when attr.Normalized => Format.R8G8B8A8_UNorm,
                    4 => Format.R8G8B8A8_UInt,
                    _ => throw new ArgumentException("Unsupported byte component count")
                },

                VertexAttribType.Int => attr.ComponentCount switch
                {
                    1 => Format.R32_SInt,
                    2 => Format.R32G32_SInt,
                    3 => Format.R32G32B32_SInt,
                    4 => Format.R32G32B32A32_SInt,
                    _ => throw new ArgumentException("Unsupported int component count")
                },

                _ => throw new ArgumentException("Unsupported attribute type")
            };
        }
        public static Format ToDxIndexFormat(IndexFormat format)
        {
            return format == IndexFormat.UInt16 ? SharpDX.DXGI.Format.R16_UInt : SharpDX.DXGI.Format.R32_UInt;
        }

        public static TextureAddressMode ToDxTextureAddressMode(WrapMode wrap)
        {

            return wrap switch
            {
                WrapMode.Repeat => TextureAddressMode.Wrap,
                WrapMode.ClampToEdge => TextureAddressMode.Clamp,
                WrapMode.ClampToBorder => TextureAddressMode.Border,
                WrapMode.Mirror => TextureAddressMode.MirrorOnce,
                WrapMode.MirrorRepeat => TextureAddressMode.Mirror,
                _ => throw new ArgumentException("Invalid argument: " + nameof(wrap))
            };
        }

        public static ulong Align16(ulong size)
        {
            return (size + 15ul) & ~15ul;
        }

        public static Filter ToDxFilter(
            FilterMode min,
            FilterMode mag,
            FilterMode mip,
            int anisotropy
        )
        {
            if (anisotropy > 1)
                return Filter.Anisotropic;

            bool minLinear = min == FilterMode.Linear;
            bool magLinear = mag == FilterMode.Linear;
            bool mipLinear = mip == FilterMode.Linear;

            if (!minLinear && !magLinear && !mipLinear)
                return Filter.MinMagMipPoint;

            if (!minLinear && !magLinear && mipLinear)
                return Filter.MinMagPointMipLinear;

            if (!minLinear && magLinear && !mipLinear)
                return Filter.MinPointMagLinearMipPoint;

            if (!minLinear && magLinear && mipLinear)
                return Filter.MinPointMagMipLinear;

            if (minLinear && !magLinear && !mipLinear)
                return Filter.MinLinearMagMipPoint;

            if (minLinear && !magLinear && mipLinear)
                return Filter.MinLinearMagPointMipLinear;

            if (minLinear && magLinear && !mipLinear)
                return Filter.MinMagLinearMipPoint;

            return Filter.MinMagMipLinear;
        }
    }
}
