using System.Numerics;

namespace DevoidGPU
{
    public interface IGraphicsDevice
    {
        IPresentSurface MainSurface { get; }

        // Factories
        IBufferFactory BufferFactory { get; }
        IShaderFactory ShaderFactory { get; }
        ITextureFactory TextureFactory { get; }


        void Initialize(nint hwnd, PresentationParameters parameters);

        void SetViewport(int x, int y, int width, int height);

        void UnbindAllShaderResources();

        void SetBlendState(BlendMode mode, bool alphaToCoverage = false);
        void SetDepthState(DepthTest func, bool writeEnable);
        void SetRasterizerState(CullMode cullMode, FillMode fillMode = FillMode.Solid, bool scissorEnabled = false);
        void SetScissorState(bool enabled);
        void SetScissorRectangle(int x = 0, int y = 0, int width = 0, int height = 0);
        void SetStencilState(bool enable, CompareFunc compare, int reference, StencilOp stencilFail = StencilOp.Keep, StencilOp depthFail = StencilOp.Keep, StencilOp pass = StencilOp.Keep);
        void SetPrimitiveType(PrimitiveType type);

        // Camera's vertical and horizontal axis might be different per gapi.
        Matrix4x4 AdjustProjectionMatrix(Matrix4x4 projection);
        void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation);
        void Draw(int vertexCount, int startVertex);

        public void BindFramebuffer(IFramebuffer fb);

        // Optionally move this to its own factory when similar items are added.
        IInputLayout CreateInputLayout(VertexInfo vertexInfo, IShader vertexShader);
        ISampler CreateSampler(SamplerDescription samplerDescription);
        ITexture GetTexture(IntPtr handle);


    }
}
