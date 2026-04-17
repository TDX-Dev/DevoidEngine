using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    public class DX11Utils
    {
        public static InputElement[] CreateInputElements(VertexInfo vertexInfo)
        {
            InputElement[] elements = new InputElement[vertexInfo.VertexAttributes.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                VertexAttribute attr = vertexInfo.VertexAttributes[i];

                bool isInstance = attr.StepMode == VertexStepMode.Instance;

                if (isInstance)
                {
                    Console.WriteLine("Bound " + attr.Name + " to slot " + attr.Slot);
                }
                elements[i] = new InputElement(
                    attr.Name,
                    attr.Index,
                    MapDXGIComponentCountToFormat(attr),
                    attr.Offset,
                    attr.Slot,
                    isInstance
                        ? InputClassification.PerInstanceData
                        : InputClassification.PerVertexData,
                    isInstance ? 1 : 0
                );
            }

            return elements;
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