using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    struct UIRenderData
    {
        public Matrix4x4 Model;
        public Vector4 Id;
    }

    public static class UIRenderer
    {
        private static readonly Shader _basicShader =
            new("Engine/Content/Shaders/UI/basic");

        private static readonly Shader _textShader =
            new("Engine/Content/Shaders/UI/basic.vert.hlsl",
                "Engine/Content/Shaders/UI/sdf_text.frag.hlsl");

        public static Framebuffer RenderOutput;

        private static IUniformBuffer _screenDataBuffer;
        private static IUniformBuffer _uiRenderBuffer;

        private static Mesh _quad;

        public static CameraData ScreenData;
        private static UIRenderData _uiRenderData;

        public static RenderState renderState = new RenderState()
        {
            BlendMode = BlendMode.AlphaBlend,
            CullMode = CullMode.Back,
            DepthTest = DepthTest.LessEqual,
            DepthWrite = false,
        };

        public static void Initialize(int width, int height)
        {
            Console.WriteLine($"Initializing UI Renderer at {width} x {height}");

            CreateFramebuffer(width, height);
            CreateCameraData(width, height);
            CreateBuffers();
            CreateQuad();
        }

        private static void CreateFramebuffer(int width, int height)
        {
            RenderOutput = new Framebuffer();

            RenderOutput.AttachRenderTexture(new Texture2D(new TextureDescription
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA16_Float,
                IsRenderTarget = true
            }));

            RenderOutput.AttachDepthTexture(new Texture2D(new TextureDescription
            {
                Width = width,
                Height = height,
                Format = TextureFormat.Depth24_Stencil8,
                IsDepthStencil = true
            }));
        }

        public static void CreateCameraData(int width, int height)
        {
            Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(
                0f, width,
                height, 0f,
                -1f, 1f
            );

            ScreenData = new CameraData
            {
                View = Matrix4x4.Identity,
                Projection = ortho,
                Position = Vector3.Zero,
                NearClip = -1f,
                FarClip = 1f,
                ScreenSize = new Vector2(width, height)
            };

            Matrix4x4.Invert(ortho, out ScreenData.InverseProjection);
        }

        private static void CreateBuffers()
        {
            _screenDataBuffer =
                Renderer.graphicsDevice.BufferFactory.CreateUniformBuffer(
                    Marshal.SizeOf<CameraData>(), BufferUsage.Dynamic);

            _uiRenderBuffer =
                Renderer.graphicsDevice.BufferFactory.CreateUniformBuffer(
                    Marshal.SizeOf<UIRenderData>(), BufferUsage.Dynamic);

            _screenDataBuffer.SetData(ScreenData);
        }

        private static void CreateQuad()
        {
            _quad = new Mesh();
            _quad.SetVertices(Primitives.GetQuadVertex());
        }


        public static void BeginRender()
        {
            RenderOutput.Bind();
            RenderOutput.Clear();

            var device = Renderer.graphicsDevice;

            device.SetRasterizerState(CullMode.None);
            device.SetPrimitiveType(PrimitiveType.Triangles);
            device.SetBlendState(BlendMode.AlphaBlend);
            device.SetViewport(0, 0, Renderer.Width, Renderer.Height);

            _screenDataBuffer.Bind(0, ShaderStage.Vertex);
            _uiRenderBuffer.Bind(1, ShaderStage.Vertex | ShaderStage.Fragment);
        }

        public static void EndRender()
        {
            // Intentionally empty – pipeline handles presentation
        }

        public static void Render(List<RenderItem> renderItems)
        {
            for (int i = 0; i < renderItems.Count; i++)
            {
                var renderItem = renderItems[i];

            }
        }


        private static Matrix4x4 BuildModel(UITransform t)
        {
            return
                Matrix4x4.CreateScale(t.size.X, t.size.Y, 1f) *
                Matrix4x4.CreateTranslation(t.position.X, t.position.Y, 0f);
        }

        private static Matrix4x4 BuildTranslationModel(UITransform t)
        {
            return Matrix4x4.CreateTranslation(t.position.X, t.position.Y, 0f);
        }


        private static void PrepareQuadDraw(Shader shader)
        {
            IInputLayout layout = Renderer.GetInputLayout(_quad, shader);
            layout.Bind();
            _quad.Bind();
            shader.Use();
        }


        public static void DrawRect(UITransform transform, int id)
        {
            _uiRenderData.Model = BuildModel(transform);
            _uiRenderData.Id = new Vector4(id, 0, 0, 0);
            _uiRenderBuffer.SetData(_uiRenderData);

            PrepareQuadDraw(_basicShader);
            Renderer.graphicsDevice.Draw(_quad.VertexCount, 0);
        }

        public static void DrawRect(Matrix4x4 model, int id)
        {
            _uiRenderData.Model = model;
            _uiRenderData.Id = new Vector4(id, 0, 0, 0);
            _uiRenderBuffer.SetData(_uiRenderData);

            PrepareQuadDraw(_basicShader);
            Renderer.graphicsDevice.Draw(_quad.VertexCount, 0);
        }

        public static void DrawText(UITransform transform, Mesh mesh, Texture2D atlas)
        {
            _uiRenderData.Model = BuildTranslationModel(transform);
            _uiRenderData.Id = Vector4.Zero;
            _uiRenderBuffer.SetData(_uiRenderData);

            IInputLayout layout = Renderer.GetInputLayout(mesh, _textShader);
            layout.Bind();

            mesh.Bind();

            _textShader.Use();
            atlas.Bind(0);
            atlas.BindSampler(0);

            Renderer.graphicsDevice.DrawIndexed(mesh.IndexBuffer.IndexCount, 0, 0);
        }
        public static void Resize(int width, int height)
        {
            CreateCameraData(width, height);
            _screenDataBuffer.SetData(ScreenData);
            RenderOutput.Resize(width, height);
        }
    }
}
