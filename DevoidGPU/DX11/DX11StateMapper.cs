using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{

    public static class DX11StateMapper
    {
        public static SharpDX.Direct3D11.CullMode ToDXCullMode(CullMode mode)
        {
            return mode switch
            {
                CullMode.None => SharpDX.Direct3D11.CullMode.None,
                CullMode.Front => SharpDX.Direct3D11.CullMode.Front,
                CullMode.Back => SharpDX.Direct3D11.CullMode.Back,
                _ => SharpDX.Direct3D11.CullMode.Back
            };
        }

        public static BlendOption ToDXBlendOption(BlendMode mode)
        {
            return mode switch
            {
                BlendMode.Opaque => BlendOption.One,       // disables blending
                BlendMode.AlphaBlend => BlendOption.SourceAlpha,
                BlendMode.Additive => BlendOption.SourceAlpha,
                BlendMode.Multiply => BlendOption.DestinationColor,
                _ => BlendOption.One
            };
        }

        public static SharpDX.Direct3D11.FillMode ToDXFillMode(FillMode mode)
        {
            return mode switch
            {
                FillMode.Solid => SharpDX.Direct3D11.FillMode.Solid,
                FillMode.Wireframe => SharpDX.Direct3D11.FillMode.Wireframe,
                _ => SharpDX.Direct3D11.FillMode.Solid
            };
        }

        public static Comparison ToDXComparison(CompareFunc func)
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

        public static Comparison ToDXDepthFunc(DepthTest func)
        {
            return func switch
            {
                DepthTest.Disabled => Comparison.Never,
                DepthTest.Less => Comparison.Less,
                DepthTest.Equal => Comparison.Equal,
                DepthTest.LessEqual => Comparison.LessEqual,
                DepthTest.Greater => Comparison.Greater,
                DepthTest.NotEqual => Comparison.NotEqual,
                DepthTest.GreaterEqual => Comparison.GreaterEqual,
                DepthTest.Always => Comparison.Always,
                _ => Comparison.LessEqual
            };
        }

        public static ShaderStage ToDXShaderStage(ShaderType func)
        {
            return func switch
            {
                ShaderType.Vertex => ShaderStage.Vertex,
                ShaderType.Fragment => ShaderStage.Fragment,
                ShaderType.Geometry => ShaderStage.Geometry,
                ShaderType.Compute => ShaderStage.Compute,
                _ => throw new NotImplementedException()
            };
        }

        public static StencilOperation ToDXStencilOp(StencilOp op) => op switch
        {
            StencilOp.Keep => StencilOperation.Keep,
            StencilOp.Zero => StencilOperation.Zero,
            StencilOp.Replace => StencilOperation.Replace,
            StencilOp.Invert => StencilOperation.Invert,
            StencilOp.Incr => StencilOperation.Increment,
            StencilOp.Decr => StencilOperation.Decrement,
            _ => StencilOperation.Keep
        };

        public static ResourceUsage ToDXResourceUsage(BufferUsage usage)
        {
            return usage switch
            {
                BufferUsage.Static => ResourceUsage.Immutable,
                BufferUsage.Dynamic => ResourceUsage.Dynamic,
                BufferUsage.Staging => ResourceUsage.Staging,
                BufferUsage.Default => ResourceUsage.Default,
                _ => ResourceUsage.Default
            };
        }

        public static SamplerStateDescription ToDXSamplerStateDescription(this SamplerDescription desc)
        {
            return new SamplerStateDescription
            {
                Filter = ToDXFilter(desc.MinFilter, desc.MagFilter, desc.MipFilter, desc.MaxAnisotropy),
                AddressU = ToDXWrap(desc.WrapU),
                AddressV = ToDXWrap(desc.WrapV),
                AddressW = ToDXWrap(desc.WrapW),
                MipLodBias = desc.MipLODBias,
                MinimumLod = desc.MinLOD,
                MaximumLod = desc.MaxLOD,
                MaximumAnisotropy = desc.MaxAnisotropy,
                ComparisonFunction = Comparison.Never // default, can extend if needed
            };
        }



        public static Filter ToDXFilter(TextureFilter min, TextureFilter mag, MipFilter mip, int maxAniso)
        {
            if (maxAniso > 1)
                return Filter.Anisotropic;

            return (min, mag, mip) switch
            {
                (TextureFilter.Nearest, TextureFilter.Nearest, MipFilter.None) => Filter.MinMagMipPoint,
                (TextureFilter.Nearest, TextureFilter.Nearest, MipFilter.Linear) => Filter.MinMagPointMipLinear,

                (TextureFilter.Linear, TextureFilter.Linear, MipFilter.None) => Filter.MinMagLinearMipPoint,
                (TextureFilter.Linear, TextureFilter.Linear, MipFilter.Linear) => Filter.MinMagMipLinear,

                (TextureFilter.Nearest, TextureFilter.Linear, MipFilter.None) => Filter.MinPointMagLinearMipPoint,
                (TextureFilter.Nearest, TextureFilter.Linear, MipFilter.Linear) => Filter.MinPointMagMipLinear,

                (TextureFilter.Linear, TextureFilter.Nearest, MipFilter.None) => Filter.MinLinearMagMipPoint,
                (TextureFilter.Linear, TextureFilter.Nearest, MipFilter.Linear) => Filter.MinLinearMagPointMipLinear,

                _ => Filter.MinMagMipLinear
            };
        }



        public static TextureAddressMode ToDXWrap(TextureWrapMode wrap) =>
            wrap switch
            {
                TextureWrapMode.Repeat => TextureAddressMode.Wrap,
                TextureWrapMode.ClampToEdge => TextureAddressMode.Clamp,
                TextureWrapMode.MirroredRepeat => TextureAddressMode.Mirror,
                _ => TextureAddressMode.Clamp
            };

        public static PrimitiveTopology ToDXPrimitiveType(PrimitiveType type) =>
            type switch
            {
                PrimitiveType.TriangleStrip => PrimitiveTopology.TriangleStrip,
                PrimitiveType.LineStrip => PrimitiveTopology.LineStrip,
                PrimitiveType.Lines => PrimitiveTopology.LineList,
                PrimitiveType.Triangles => PrimitiveTopology.TriangleList,
                _ => PrimitiveTopology.Undefined
            };

    }

}