using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11InputLayout : IInputLayout
    {
        public InputLayout NativeLayout { get; }

        public VertexInfo VertexInfo { get; }

        private readonly DeviceContext deviceContext;

        public DX11InputLayout(Device device, DeviceContext deviceContext, VertexInfo vertexInfo, DX11Shader shader)
        {
            this.deviceContext = deviceContext;

            VertexInfo = vertexInfo;

            NativeLayout = new InputLayout(
                device,
                shader.ByteCode,
                DX11Utils.CreateInputElements(vertexInfo)
            );
        }


        public void Bind()
        {
            deviceContext.InputAssembler.InputLayout = NativeLayout;
        }


    }
}