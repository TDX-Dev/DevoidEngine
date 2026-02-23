using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidGPU;
using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Imgui
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImShaderData
    {
        public Matrix4x4 ProjectionMatrix;
        public float DPIScaling;
        Vector3 padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImGuiVertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(ImGuiVertex),
            new VertexAttribute("POSITION", 0, 2, 0, VertexAttribType.Float),
            new VertexAttribute("TEXCOORD", 0, 2, 2 * sizeof(float), VertexAttribType.Float),
            new VertexAttribute("COLOR", 0, 4, 4 * sizeof(float), VertexAttribType.UnsignedByte, normalized: true)
        );
    }


    public class ImGuiRenderer
    {
        IGraphicsDevice graphicsDevice;

        Shader guiShader;
        ImShaderData ImShaderData = new ImShaderData();
        IUniformBuffer ShaderConstantBuffer;

        IVertexBuffer VertexBuffer;
        IIndexBuffer IndexBuffer;
        IInputLayout InputLayout;

        private int vertexBufferSize = 2000;
        private int indexBufferSize = 4000;

        private ITexture2D _fontTexture;
        private ISampler _defaultSampler;

        private float _scaleFactor = 1.0f;

        private bool _fontsConfigured = false;
        private int _fontsCount = -1;
        private ImFontPtr DefaultFont;

        public Action? OnGUI;


        public ImGuiRenderer(IGraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.guiShader = new Shader("Engine/Content/Shaders/Imgui/gui");
            this.ShaderConstantBuffer = graphicsDevice.BufferFactory.CreateUniformBuffer<ImShaderData>(BufferUsage.Dynamic);

            this.VertexBuffer = graphicsDevice.BufferFactory.CreateVertexBuffer(BufferUsage.Dynamic, ImGuiVertex.VertexInfo, vertexBufferSize);
            this.IndexBuffer = graphicsDevice.BufferFactory.CreateIndexBuffer(indexBufferSize, BufferUsage.Dynamic, true);
            this.InputLayout = graphicsDevice.CreateInputLayout(ImGuiVertex.VertexInfo, guiShader.vShader);

            this._defaultSampler = Renderer.graphicsDevice.CreateSampler(new SamplerDescription()
            {
                MinFilter = TextureFilter.Linear,
                MagFilter = TextureFilter.Linear,
                MipFilter = MipFilter.None,
                WrapU = TextureWrapMode.ClampToEdge,
                WrapV = TextureWrapMode.ClampToEdge,
                WrapW = TextureWrapMode.ClampToEdge,
                MaxAnisotropy = 1,
                MipLODBias = 0.0f,
                MinLOD = 0.0f,
                MaxLOD = 0.0f
            });
        }

        private unsafe void DumpImGuiVertices(ImDrawDataPtr drawData, int count = 5)
        {
            if (drawData.CmdListsCount == 0)
                return;

            // Take first command list
            var vtxBuffer = drawData.CmdLists[0].VtxBuffer;
            int total = Math.Min(count, vtxBuffer.Size);

            Console.WriteLine($"[ImGui Debug] Dumping {total} vertices... (stride = {sizeof(ImDrawVert)})");

            for (int i = 0; i < total; i++)
            {
                ImDrawVertPtr v = vtxBuffer[i];
                Vector2 pos = v.pos;
                Vector2 uv = v.uv;
                uint col = v.col;

                byte r = (byte)((col >> 0) & 0xFF);
                byte g = (byte)((col >> 8) & 0xFF);
                byte b = (byte)((col >> 16) & 0xFF);
                byte a = (byte)((col >> 24) & 0xFF);

                Console.WriteLine(
                    $"V{i}: pos=({pos.X:F1}, {pos.Y:F1}), uv=({uv.X:F2}, {uv.Y:F2}), col=RGBA({r},{g},{b},{a}) [0x{col:X8}]"
                );
            }
        }

        public ImFontPtr AddFontFromFile(string path, float sizePixels)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            return io.Fonts.AddFontFromFileTTF(path, sizePixels);
        }

        public void AddDefaultFont()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.AddFontDefault();
        }

        public void SetDefaultFont(ImFontPtr font)
        {
            this.DefaultFont = font;

        }

        public unsafe ImFontPtr LoadIconFont(string path, int size, (ushort, ushort) range)
        {
            ImFontConfigPtr configuration = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());

            configuration.GlyphOffset = new System.Numerics.Vector2(0, 6);
            configuration.GlyphMinAdvanceX = size;
            configuration.MergeMode = true;
            configuration.PixelSnapH = true;

            GCHandle rangeHandle = GCHandle.Alloc(new ushort[]
            {
                range.Item1,
                range.Item2,
        0
            }, GCHandleType.Pinned);

            try
            {
                return ImGui.GetIO().Fonts.AddFontFromFileTTF(path, (float)size, configuration, rangeHandle.AddrOfPinnedObject()); ;
            }
            finally
            {
                configuration.Destroy();

                if (rangeHandle.IsAllocated)
                {
                    rangeHandle.Free();
                }
            }

        }


        public void Initialize()
        {
            ImGui.SetCurrentContext(ImGui.CreateContext());

            ImGuiIOPtr io = ImGui.GetIO();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            io.DisplaySize = new Vector2(graphicsDevice.MainSurface.Width, graphicsDevice.MainSurface.Height);

            ConfigureFontAtlas();

        }

        public void ConfigureFontAtlas()
        {

            ImGuiIOPtr io = ImGui.GetIO();

            if (io.Fonts.Fonts.Size == 0)
            {
                AddDefaultFont();
            }

            io.Fonts.Build();

            // Bake the font atlas
            io.Fonts.GetTexDataAsRGBA32(out nint pixels, out int width, out int height, out int bpp);

            int size = width * height * bpp;
            byte[] managedPixels = new byte[size];
            Marshal.Copy(pixels, managedPixels, 0, size);

            _fontTexture = Renderer.graphicsDevice.TextureFactory.CreateTexture2D(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA8_UNorm,
                GenerateMipmaps = false,
                IsDepthStencil = false,
                IsRenderTarget = false,
                IsMutable = false,
                MipLevels = 0,
            });
            _fontTexture.SetData(managedPixels);

            io.Fonts.SetTexID(_fontTexture.GetHandle());

        }

        public void UpdateDisplay()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new Vector2(graphicsDevice.MainSurface.Width, graphicsDevice.MainSurface.Height);
        }

        public void CreateDockspace()
        {

            ImGuiViewportPtr viewport = ImGui.GetMainViewport();

            ImGui.SetNextWindowPos(viewport.WorkPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(viewport.WorkSize, ImGuiCond.Always);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration;

            ImGui.Begin("DockSpaceWindow", windowFlags);
            ImGui.PopStyleVar(3);

            uint dockspaceId = ImGui.GetID("MyDockspace");
            ImGui.DockSpace(dockspaceId, System.Numerics.Vector2.Zero,
                            ImGuiDockNodeFlags.PassthruCentralNode);


            ImGui.End();

        }


        public void UpdateInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MousePos = InternalInputState.MousePosition;
            io.MouseDown[0] = InternalInputState.GetMouseButton((int)MouseButton.Left);
            io.MouseDown[1] = InternalInputState.GetMouseButton((int)MouseButton.Right);
            io.MouseDown[2] = InternalInputState.GetMouseButton((int)MouseButton.Middle);
            io.MouseWheel = InternalInputState.MouseScroll.Y;

            io.MouseWheelH = InternalInputState.MouseScroll.X;


        }

        public void UpdatePerFrameParameters(float delta)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.DeltaTime = delta;

            if (io.Fonts.Fonts.Size != _fontsCount)
            {
                ConfigureFontAtlas();
                _fontsCount = io.Fonts.Fonts.Size;
            }
        }


        public void PerFrame(float delta = 1 / 60f)
        {
            UpdatePerFrameParameters(delta);
            UpdateDisplay();

            ImGui.NewFrame();
            ImGui.PushFont(DefaultFont);

            CreateDockspace();

            OnGUI?.Invoke();


            ImGui.PopFont();
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
        }

        public void RenderImDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0) return;

            ImGuiIOPtr io = ImGui.GetIO();

            int fbWidth = (int)(io.DisplaySize.X * io.DisplayFramebufferScale.X);
            int fbHeight = (int)(io.DisplaySize.Y * io.DisplayFramebufferScale.Y);
            if (fbWidth <= 0 || fbHeight <= 0) return;

            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            var mvp = Matrix4x4.CreateOrthographicOffCenter(
                0.0f, io.DisplaySize.X,
                io.DisplaySize.Y, 0.0f,
                -1.0f, 1.0f);

            ImShaderData.ProjectionMatrix = mvp;
            ImShaderData.DPIScaling = 1.0f;

            graphicsDevice.MainSurface.Bind();
            graphicsDevice.MainSurface.ClearColor(new Vector4(0, 0, 0, 1));
            graphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            guiShader.Use();
            ShaderConstantBuffer.SetData(ImShaderData);
            ShaderConstantBuffer.Bind(0, ShaderStage.Vertex);

            graphicsDevice.MainSurface.Bind();

            graphicsDevice.SetViewport(0, 0, fbWidth, fbHeight);
            graphicsDevice.SetBlendState(BlendMode.AlphaBlend);
            graphicsDevice.SetDepthState(DepthTest.Disabled, false);
            graphicsDevice.SetRasterizerState(CullMode.None);
            graphicsDevice.SetScissorState(true);

            InputLayout.Bind();
            VertexBuffer.Bind();
            IndexBuffer.Bind();

            var clipOffset = drawData.DisplayPos;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                int vtxSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vtxSize > vertexBufferSize)
                {
                    VertexBuffer.Dispose();
                    vertexBufferSize = (int)Math.Max(vertexBufferSize * 1.5f, vtxSize);
                    VertexBuffer = graphicsDevice.BufferFactory.CreateVertexBuffer(BufferUsage.Dynamic, ImGuiVertex.VertexInfo, vertexBufferSize);
                    VertexBuffer.Bind();
                }

                int idxSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (idxSize > indexBufferSize)
                {
                    IndexBuffer.Dispose();
                    indexBufferSize = (int)Math.Max(indexBufferSize * 1.5f, idxSize);
                    IndexBuffer = graphicsDevice.BufferFactory.CreateIndexBuffer(indexBufferSize, BufferUsage.Dynamic, true);

                    IndexBuffer.Bind();

                }

                VertexBuffer.UpdatePartial(cmdList.VtxBuffer.Data, 0, cmdList.VtxBuffer.Size);
                IndexBuffer.UpdatePartial(cmdList.IdxBuffer.Data, 0, cmdList.IdxBuffer.Size);


                for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmd_i];

                    var cr = pcmd.ClipRect;
                    int x1 = (int)Math.Floor(cr.X - clipOffset.X);
                    int y1 = (int)Math.Floor(cr.Y - clipOffset.Y);
                    int x2 = (int)Math.Ceiling(cr.Z - clipOffset.X);
                    int y2 = (int)Math.Ceiling(cr.W - clipOffset.Y);

                    x1 = Math.Clamp(x1, 0, fbWidth);
                    y1 = Math.Clamp(y1, 0, fbHeight);
                    x2 = Math.Clamp(x2, 0, fbWidth);
                    y2 = Math.Clamp(y2, 0, fbHeight);

                    if (x2 <= x1 || y2 <= y1)
                        continue;

                    graphicsDevice.SetScissorRectangle(x1, y1, x2 - x1, y2 - y1);

                    var texture = (ITexture2D)graphicsDevice.GetTexture(pcmd.TextureId);
                    texture.Bind();
                    _defaultSampler.Bind();

                    graphicsDevice.DrawIndexed((int)pcmd.ElemCount, (int)pcmd.IdxOffset, (int)pcmd.VtxOffset);
                }
            }
            graphicsDevice.SetScissorState(false);
            graphicsDevice.SetBlendState(BlendMode.Opaque);
            graphicsDevice.SetDepthState(DepthTest.LessEqual, true);
            graphicsDevice.SetRasterizerState(CullMode.Back);
        }



    }
}
