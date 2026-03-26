using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    public class DX11Utils
    {
        public static InputElement[] CreateInputElements(VertexInfo vertexInfo)
        {
            InputElement[] InputLayoutElements = new InputElement[vertexInfo.VertexAttributes.Length];
            for (int i = 0; i < InputLayoutElements.Length; i++)
            {
                VertexAttribute attribute = vertexInfo.VertexAttributes[i];
                InputLayoutElements[i] = new InputElement()
                {
                    SemanticName = attribute.Name,
                    SemanticIndex = attribute.Index,
                    Format = MapDXGIComponentCountToFormat(attribute),
                    AlignedByteOffset = attribute.Offset,
                    Slot = 0
                };

            }

            return InputLayoutElements;
        }

        public static Format MapDXGIComponentCountToFormat(VertexAttribute attr)
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


    }
}