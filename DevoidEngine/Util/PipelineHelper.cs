using DevoidEngine.Core;
using DevoidEngine.Logging;
using DevoidEngine.Serialization;
using DevoidGPU;

namespace DevoidEngine.Util
{
    public static class PipelineHelper
    {

        public static GraphicsPipelineDescription CreatePipelineDescription(PipelineStateDescriptor descriptor, ShaderPass shaderPass, VertexInfo vertexLayout, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            BlendStateDescription bsd = ParseBlend(descriptor.Blend);
            DepthStencilState dss = ParseDepth(descriptor.DepthCompareMode, descriptor.WriteDepth);
            RasterizerState rs = ParseRasterizerState(descriptor.CullMode, descriptor.FillMode);
            IShader vertexShader = shaderPass.VS;
            IShader fragmentShader = shaderPass.FS;


            GraphicsPipelineDescription pipeline = new()
            {
                Blend = bsd,
                DepthStencil = dss,
                Rasterizer = rs,
                Topology = primitiveType,
                VertexLayout = vertexLayout,
                VertexShader = vertexShader,
                PixelShader = fragmentShader,
            };
            return pipeline;
        }

        static RasterizerState ParseRasterizerState(CullMode? cullMode, FillMode? fillMode)
        {
            RasterizerState rasterizerState = new()
            {
                CullMode = cullMode ?? CullMode.Back,
                FillMode = fillMode ?? FillMode.Solid
            };
            return rasterizerState;
        }
        static DepthStencilState ParseDepth(DepthTest? depthMode, bool? writeDepth)
        {
            DepthStencilState depthStencilState;

            depthMode ??= DepthTest.LessEqual;
            bool depthWrite = writeDepth ?? true;

            switch (depthMode)
            {
                case DepthTest.Disabled:
                    depthStencilState = new()
                    {
                        DepthTest = false,
                        DepthWrite = depthWrite,
                        DepthFunc = CompareFunc.Never
                    };
                    break;
                case DepthTest.Less:
                    depthStencilState = new()
                    {
                        DepthTest = true,
                        DepthWrite = depthWrite,
                        DepthFunc = CompareFunc.Less
                    };
                    break;
                case DepthTest.LessEqual:
                    depthStencilState = new()
                    {
                        DepthTest = true,
                        DepthWrite = depthWrite,
                        DepthFunc = CompareFunc.LessEqual
                    };
                    break;
                case DepthTest.Always:
                    depthStencilState = new()
                    {
                        DepthTest = true,
                        DepthWrite = depthWrite,
                        DepthFunc = CompareFunc.Always
                    };
                    break;
                case DepthTest.Equal:
                    depthStencilState = new()
                    {
                        DepthTest = true,
                        DepthWrite = depthWrite,
                        DepthFunc = CompareFunc.Equal
                    };
                    break;
                default:
                    DevoidLog.Warning(LogCategory.Core, "Material depth evaluation parameter was invalid or not supported.");
                    depthStencilState = new()
                    {
                        DepthTest = true,
                        DepthWrite = depthWrite,
                        DepthFunc = CompareFunc.LessEqual
                    };
                    break;
            }

            return depthStencilState;
        }
        static BlendStateDescription ParseBlend(BlendMode? blend)
        {
            BlendState state;

            blend ??= BlendMode.Opaque;

            switch (blend)
            {
                case BlendMode.Opaque:
                    state = new BlendState
                    {
                        Enable = false,
                        WriteMask = ColorMask.All
                    };
                    break;

                case BlendMode.AlphaBlend:
                    state = new BlendState
                    {
                        Enable = true,
                        SrcColor = BlendFactor.SrcAlpha,
                        DstColor = BlendFactor.InvSrcAlpha,
                        ColorOp = BlendOp.Add,
                        SrcAlpha = BlendFactor.One,
                        DstAlpha = BlendFactor.InvSrcAlpha,
                        AlphaOp = BlendOp.Add,
                        WriteMask = ColorMask.All
                    };
                    break;

                case BlendMode.Additive:
                    state = new BlendState
                    {
                        Enable = true,
                        SrcColor = BlendFactor.SrcAlpha,
                        DstColor = BlendFactor.One,
                        ColorOp = BlendOp.Add,
                        SrcAlpha = BlendFactor.One,
                        DstAlpha = BlendFactor.One,
                        AlphaOp = BlendOp.Add,
                        WriteMask = ColorMask.All
                    };
                    break;

                default:
                    DevoidLog.Warning(LogCategory.Core, "Material blend parameter is invalid or not supported, defaulting to opaque.");
                    state = new BlendState
                    {
                        Enable = false,
                        WriteMask = ColorMask.All
                    };
                    break;
            }

            BlendStateDescription blendStateDescription = new()
            {
                AlphaToCoverage = false,
                IndependentBlend = false,
                BlendStates = [state]
            };
            return blendStateDescription;
        }
    }
}
