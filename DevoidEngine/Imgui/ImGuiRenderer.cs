using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using DevoidGPU;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Window = DevoidEngine.Core.Window;

namespace DevoidEngine.Imgui
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImShaderData
    {
        public Matrix4x4 ProjectionMatrix;
        public float DpiScaling;
        private Vector3 padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImGuiVertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;

        public static readonly VertexInfo VertexInfo = new(
            typeof(ImGuiVertex),
            new VertexAttribute("POSITION", 0, 2, 0, 0, VertexAttribType.Float),
            new VertexAttribute("TEXCOORD", 0, 2, 2 * sizeof(float), 0, VertexAttribType.Float),
            new VertexAttribute("COLOR", 0, 4, 4 * sizeof(float), 0, VertexAttribType.UnsignedByte, VertexStepMode.Vertex, normalized: true)
        );
    }

    public class ImGuiRenderer : IInputLayer
    {
        private static readonly Keys[] ImGuiKeys =
        [
            // Navigation & Editing
            Keys.Tab, Keys.Left, Keys.Right, Keys.Up, Keys.Down,
            Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End, Keys.Insert,
            Keys.Delete, Keys.Backspace, Keys.Space, Keys.Enter, Keys.Escape,
            Keys.CapsLock, Keys.ScrollLock, Keys.NumLock, Keys.PrintScreen, Keys.Pause,

            // Modifiers
            Keys.LeftShift, Keys.RightShift,
            Keys.LeftControl, Keys.RightControl,
            Keys.LeftAlt, Keys.RightAlt,
            Keys.LeftSuper, Keys.RightSuper,

            // Alphabet (A-Z)
            Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I,
            Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R,
            Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,

            // Number Row (0-9)
            Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4,
            Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,

            // Function Keys (F1-F12)
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6,
            Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,

            // Keypad
            Keys.KeyPad0, Keys.KeyPad1, Keys.KeyPad2, Keys.KeyPad3, Keys.KeyPad4,
            Keys.KeyPad5, Keys.KeyPad6, Keys.KeyPad7, Keys.KeyPad8, Keys.KeyPad9,
            Keys.KeyPadDecimal, Keys.KeyPadDivide, Keys.KeyPadMultiply,
            Keys.KeyPadSubtract, Keys.KeyPadAdd, Keys.KeyPadEnter,

            // Punctuation & Symbols
            Keys.Minus, Keys.Equal, Keys.LeftBracket, Keys.RightBracket,
            Keys.Semicolon, Keys.Apostrophe, Keys.GraveAccent, Keys.Comma,
            Keys.Period, Keys.Slash, Keys.Backslash
        ];

        private readonly Shader _guiShader;
        private readonly IPipeline _guiPipeline;
        private ImShaderData _imShaderData = new();
        private readonly UniformBuffer _shaderConstantBuffer;

        private VertexBuffer<ImGuiVertex> _vertexBuffer;
        private IndexBuffer _indexBuffer;

        private readonly IDescriptorSet _guiDescriptor;

        private int _vertexBufferSize = 2000;
        private int _indexBufferSize = 4000;

        private Texture? _fontTexture;
        private readonly Sampler _defaultSampler;

        private readonly RenderTarget _imguiRenderTarget;
        private Texture? _imguiRenderTexture;

        private int _prevWidth;
        private int _prevHeight;

        private int _fontsCount = -1;
        private ImFontPtr _defaultFont;
        private readonly List<nint> _glyphRanges = [];
        public Action? OnGui { get; set; }
        public float FooterHeight { get; set; } = 24f;
        public float ToolbarHeight { get; set; } = 0f;

        public ImGuiRenderer()
        {
            _guiShader = new("IMGUI GUI SHADER", "Content/Shaders/imgui.vert.hlsl", "Content/Shaders/imgui.frag.hlsl");
            _guiDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(_guiShader.GetActivePass().DescriptorLayout);

            _guiPipeline = Engine.GraphicsDevice.CreateGraphicsPipeline(
                PipelineHelper.CreatePipelineDescription(
                    new()
                    {
                        Blend = BlendMode.AlphaBlend,
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Solid,
                        DepthCompareMode = DepthTest.Less,
                        PrimitiveType = PrimitiveType.Triangles,
                        WriteDepth = true
                    },
                    _guiShader.GetActivePass(),
                    ImGuiVertex.VertexInfo
                )
            );

            _shaderConstantBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<ImShaderData>());

            _vertexBuffer = new VertexBuffer<ImGuiVertex>(Engine.GraphicsDevice, _vertexBufferSize, ImGuiVertex.VertexInfo, ResourceUsage.Dynamic);
            _indexBuffer = new IndexBuffer(Engine.GraphicsDevice, _indexBufferSize, ResourceUsage.Dynamic, IndexFormat.UInt16);

            _defaultSampler = Sampler.Create(new SamplerDescription()
            {
                MinFilter = FilterMode.Linear,
                MagFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                AddressU = WrapMode.ClampToEdge,
                AddressV = WrapMode.ClampToEdge,
                AddressW = WrapMode.ClampToEdge,
                MaxAnisotropy = 1,
                MipLodBias = 0.0f,
                MinLod = 0.0f,
                MaxLod = 0.0f
            });

            _imguiRenderTarget = RenderTarget.Create(1);
        }

        private unsafe void DumpImGuiVertices(ImDrawDataPtr drawData, int count = 5)
        {
            if (drawData.CmdListsCount == 0)
                return;

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

                Console.WriteLine($"V{i}: pos=({pos.X:F1}, {pos.Y:F1}), uv=({uv.X:F2}, {uv.Y:F2}), col=RGBA({r},{g},{b},{a}) [0x{col:X8}]");
            }
        }

        public ImFontPtr AddFontFromFile(string path, float sizePixels)
        {
            return ImGui.GetIO().Fonts.AddFontFromFileTTF(path, sizePixels);
        }

        public void AddDefaultFont()
        {
            ImGui.GetIO().Fonts.AddFontDefault();
        }

        public void SetDefaultFont(ImFontPtr font)
        {
            _defaultFont = font;
        }

        public unsafe ImFontPtr LoadIconFont(
            string path,
            int size,
            (ushort Min, ushort Max) range)
        {
            ImFontConfigPtr config = new(ImGuiNative.ImFontConfig_ImFontConfig())
            {
                GlyphOffset = new Vector2(0, 4),
                GlyphMinAdvanceX = size,
                MergeMode = true,
                PixelSnapH = true
            };

            nint ranges = Marshal.AllocHGlobal(3 * sizeof(ushort));

            ushort* rangePtr = (ushort*)ranges;

            rangePtr[0] = range.Min;
            rangePtr[1] = range.Max;
            rangePtr[2] = 0;

            _glyphRanges.Add(ranges);

            try
            {
                return ImGui.GetIO().Fonts.AddFontFromFileTTF(
                    path,
                    size,
                    config,
                    ranges);
            }
            finally
            {
                config.Destroy();
            }
        }

        private unsafe void UpdateMonitors()
        {
            ImGuiPlatformIOPtr platformIo = ImGui.GetPlatformIO();

            if (platformIo.NativePtr->Monitors.Data != IntPtr.Zero)
            {
                Marshal.FreeHGlobal((nint)platformIo.NativePtr->Monitors.Data);
            }

            int count = Monitors.GetMonitors().Count;
            if (count == 0)
            {
                platformIo.NativePtr->Monitors = new ImVector();
                return;
            }

            IntPtr data = Marshal.AllocHGlobal(Unsafe.SizeOf<ImGuiPlatformMonitor>() * count);
            platformIo.NativePtr->Monitors = new ImVector(count, count, data);

            var monitors = Monitors.GetMonitors();
            for (int i = 0; i < monitors.Count; i++)
            {
                MonitorInfo monitor = monitors[i];
                ImGuiPlatformMonitorPtr imguiMonitor = platformIo.Monitors[i];

                imguiMonitor.MainPos = new Vector2(monitor.ClientArea.Min.X, monitor.ClientArea.Min.Y);
                imguiMonitor.MainSize = new Vector2(monitor.ClientArea.Size.X, monitor.ClientArea.Size.Y);
                imguiMonitor.WorkPos = new Vector2(monitor.WorkArea.Min.X, monitor.WorkArea.Min.Y);
                imguiMonitor.WorkSize = new Vector2(monitor.WorkArea.Size.X, monitor.WorkArea.Size.Y);
                imguiMonitor.DpiScale = 1.0f;
            }
        }

        public unsafe void Initialize(WindowSurface window)
        {
            ImGui.SetCurrentContext(ImGui.CreateContext());
            ImGuiIOPtr io = ImGui.GetIO();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

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
            io.Fonts.GetTexDataAsRGBA32(out nint pixels, out int width, out int height, out int bpp);

            int size = width * height * bpp;
            byte[] managedPixels = new byte[size];
            Marshal.Copy(pixels, managedPixels, 0, size);

            _fontTexture?.Dispose(); // Optimize: Prevents memory leak when fonts change/rebuild

            _fontTexture = Texture.Create2D(width, height, TextureFormat.RGBA8UNorm, TextureUsage.ShaderResource);
            _fontTexture.GPU.Update(managedPixels);

            io.Fonts.SetTexID((nint)_fontTexture.Id);
        }

        public void UpdateDisplay(Window window)
        {
            if (window.ClientSize.X == 0 || window.ClientSize.Y == 0)
                return;

            ImGui.GetIO().DisplaySize = new(window.ClientSize.X, window.ClientSize.Y);

            if (window.ClientSize.X != _prevWidth || window.ClientSize.Y != _prevHeight)
            {
                _imguiRenderTexture?.Dispose(); // Optimize: Resolves memory leak upon window resize

                _imguiRenderTexture = Texture.Create2D(
                    window.ClientSize.X,
                    window.ClientSize.Y,
                    TextureFormat.RGBA8UNorm,
                    TextureUsage.RenderTarget | TextureUsage.ShaderResource
                );

                _prevWidth = window.ClientSize.X;
                _prevHeight = window.ClientSize.Y;
            }

            if (_imguiRenderTexture != null)
            {
                _imguiRenderTarget.SetColorAttachment(0, _imguiRenderTexture);
            }
        }

        public void SetCustomToolbarHeight(float height) => ToolbarHeight = height;

        public void CreateDockspace()
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();

            ImGui.SetNextWindowPos(
                new Vector2(viewport.WorkPos.X, viewport.WorkPos.Y + ToolbarHeight),
                ImGuiCond.Always
            );

            ImGui.SetNextWindowSize(
                new Vector2(viewport.WorkSize.X, viewport.WorkSize.Y - ToolbarHeight - FooterHeight),
                ImGuiCond.Always
            );

            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            ImGuiWindowFlags windowFlags =
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoDecoration;

            ImGui.Begin("DockSpaceWindow", windowFlags);
            ImGui.PopStyleVar(3);

            uint dockspaceId = ImGui.GetID("MyDockspace");
            ImGui.DockSpace(dockspaceId, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

            ImGui.End();
        }

        private static ImGuiKey MapKey(Keys key)
        {
            return key switch
            {
                // Navigation & Editing
                Keys.Tab => ImGuiKey.Tab,
                Keys.Left => ImGuiKey.LeftArrow,
                Keys.Right => ImGuiKey.RightArrow,
                Keys.Up => ImGuiKey.UpArrow,
                Keys.Down => ImGuiKey.DownArrow,
                Keys.PageUp => ImGuiKey.PageUp,
                Keys.PageDown => ImGuiKey.PageDown,
                Keys.Home => ImGuiKey.Home,
                Keys.End => ImGuiKey.End,
                Keys.Insert => ImGuiKey.Insert,
                Keys.Delete => ImGuiKey.Delete,
                Keys.Backspace => ImGuiKey.Backspace,
                Keys.Space => ImGuiKey.Space,
                Keys.Enter => ImGuiKey.Enter,
                Keys.Escape => ImGuiKey.Escape,
                Keys.CapsLock => ImGuiKey.CapsLock,
                Keys.ScrollLock => ImGuiKey.ScrollLock,
                Keys.NumLock => ImGuiKey.NumLock,
                Keys.PrintScreen => ImGuiKey.PrintScreen,
                Keys.Pause => ImGuiKey.Pause,

                // Modifiers
                Keys.LeftShift => ImGuiKey.LeftShift,
                Keys.RightShift => ImGuiKey.RightShift,
                Keys.LeftControl => ImGuiKey.LeftCtrl,
                Keys.RightControl => ImGuiKey.RightCtrl,
                Keys.LeftAlt => ImGuiKey.LeftAlt,
                Keys.RightAlt => ImGuiKey.RightAlt,
                Keys.LeftSuper => ImGuiKey.LeftSuper,
                Keys.RightSuper => ImGuiKey.RightSuper,

                // Alphabet
                Keys.A => ImGuiKey.A,
                Keys.B => ImGuiKey.B,
                Keys.C => ImGuiKey.C,
                Keys.D => ImGuiKey.D,
                Keys.E => ImGuiKey.E,
                Keys.F => ImGuiKey.F,
                Keys.G => ImGuiKey.G,
                Keys.H => ImGuiKey.H,
                Keys.I => ImGuiKey.I,
                Keys.J => ImGuiKey.J,
                Keys.K => ImGuiKey.K,
                Keys.L => ImGuiKey.L,
                Keys.M => ImGuiKey.M,
                Keys.N => ImGuiKey.N,
                Keys.O => ImGuiKey.O,
                Keys.P => ImGuiKey.P,
                Keys.Q => ImGuiKey.Q,
                Keys.R => ImGuiKey.R,
                Keys.S => ImGuiKey.S,
                Keys.T => ImGuiKey.T,
                Keys.U => ImGuiKey.U,
                Keys.V => ImGuiKey.V,
                Keys.W => ImGuiKey.W,
                Keys.X => ImGuiKey.X,
                Keys.Y => ImGuiKey.Y,
                Keys.Z => ImGuiKey.Z,

                // Number Row
                Keys.D0 => ImGuiKey._0,
                Keys.D1 => ImGuiKey._1,
                Keys.D2 => ImGuiKey._2,
                Keys.D3 => ImGuiKey._3,
                Keys.D4 => ImGuiKey._4,
                Keys.D5 => ImGuiKey._5,
                Keys.D6 => ImGuiKey._6,
                Keys.D7 => ImGuiKey._7,
                Keys.D8 => ImGuiKey._8,
                Keys.D9 => ImGuiKey._9,

                // Function Keys
                Keys.F1 => ImGuiKey.F1,
                Keys.F2 => ImGuiKey.F2,
                Keys.F3 => ImGuiKey.F3,
                Keys.F4 => ImGuiKey.F4,
                Keys.F5 => ImGuiKey.F5,
                Keys.F6 => ImGuiKey.F6,
                Keys.F7 => ImGuiKey.F7,
                Keys.F8 => ImGuiKey.F8,
                Keys.F9 => ImGuiKey.F9,
                Keys.F10 => ImGuiKey.F10,
                Keys.F11 => ImGuiKey.F11,
                Keys.F12 => ImGuiKey.F12,

                // Keypad
                Keys.KeyPad0 => ImGuiKey.Keypad0,
                Keys.KeyPad1 => ImGuiKey.Keypad1,
                Keys.KeyPad2 => ImGuiKey.Keypad2,
                Keys.KeyPad3 => ImGuiKey.Keypad3,
                Keys.KeyPad4 => ImGuiKey.Keypad4,
                Keys.KeyPad5 => ImGuiKey.Keypad5,
                Keys.KeyPad6 => ImGuiKey.Keypad6,
                Keys.KeyPad7 => ImGuiKey.Keypad7,
                Keys.KeyPad8 => ImGuiKey.Keypad8,
                Keys.KeyPad9 => ImGuiKey.Keypad9,
                Keys.KeyPadDecimal => ImGuiKey.KeypadDecimal,
                Keys.KeyPadDivide => ImGuiKey.KeypadDivide,
                Keys.KeyPadMultiply => ImGuiKey.KeypadMultiply,
                Keys.KeyPadSubtract => ImGuiKey.KeypadSubtract,
                Keys.KeyPadAdd => ImGuiKey.KeypadAdd,
                Keys.KeyPadEnter => ImGuiKey.KeypadEnter,

                // Punctuation & Symbols
                Keys.Minus => ImGuiKey.Minus,
                Keys.Equal => ImGuiKey.Equal,
                Keys.LeftBracket => ImGuiKey.LeftBracket,
                Keys.RightBracket => ImGuiKey.RightBracket,
                Keys.Semicolon => ImGuiKey.Semicolon,
                Keys.Apostrophe => ImGuiKey.Apostrophe,
                Keys.GraveAccent => ImGuiKey.GraveAccent,
                Keys.Comma => ImGuiKey.Comma,
                Keys.Period => ImGuiKey.Period,
                Keys.Slash => ImGuiKey.Slash,
                Keys.Backslash => ImGuiKey.Backslash,

                _ => ImGuiKey.None
            };
        }

        public void UpdateInput(Window window)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            var mouse = window.MouseState;
            var keyboard = window.KeyboardState;

            io.MousePos = new Vector2(mouse.Position.X, mouse.Position.Y);
            io.MouseDown[0] = mouse.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = mouse.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = mouse.IsButtonDown(MouseButton.Middle);
            io.MouseWheel = mouse.ScrollDelta.Y;
            io.MouseWheelH = mouse.ScrollDelta.X;

            foreach (Keys key in ImGuiKeys)
            {
                io.AddKeyEvent(MapKey(key), keyboard.IsKeyDown(key));
            }

            io.AddKeyEvent(ImGuiKey.ModCtrl, keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl));
            io.AddKeyEvent(ImGuiKey.ModShift, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
            io.AddKeyEvent(ImGuiKey.ModAlt, keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt));
            io.AddKeyEvent(ImGuiKey.ModSuper, keyboard.IsKeyDown(Keys.LeftSuper) || keyboard.IsKeyDown(Keys.RightSuper));
        }

        public void OnTextInput(char c)
        {
            ImGui.GetIO().AddInputCharacter(c);
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

        public void BeginFrame(WindowSurface surface, float delta)
        {
            UpdatePerFrameParameters(delta);
            UpdateDisplay(surface.Window);
            UpdateInput(surface.Window);

            ImGui.NewFrame();
            ImGui.PushFont(_defaultFont);

            CreateDockspace();

            OnGui?.Invoke();

            ImGui.PopFont();
        }

        public void EndFrame(ICommandList cmd, WindowSurface surface)
        {
            ImGui.Render();

            var size = surface.Window.ClientSize;

            RenderImDrawData(
                ImGui.GetDrawData(),
                cmd,
                surface,
                size.X,
                size.Y
            );

            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }

        public void RenderImDrawData(ImDrawDataPtr drawData, ICommandList cmd, WindowSurface surface, int fbWidth, int fbHeight)
        {
            if (drawData.CmdListsCount == 0) return;

            drawData.ScaleClipRects(Vector2.One);

            var mvp = Matrix4x4.CreateOrthographicOffCenterLeftHanded(
                0.0f, fbWidth,
                fbHeight, 0.0f,
                -1.0f, 1.0f
            );

            _imShaderData.ProjectionMatrix = mvp;
            _imShaderData.DpiScaling = 1.0f;

            cmd.SetFramebuffer(surface.Framebuffer);

            Engine.Renderer.PushViewport(cmd, new ViewportRect()
            {
                X = 0,
                Y = 0,
                Width = fbWidth,
                Height = fbHeight,
            });

            _shaderConstantBuffer.Update(_imShaderData);
            _guiDescriptor.SetUniformBuffer(0, _shaderConstantBuffer.Gpu);

            cmd.SetPipeline(_guiPipeline);

            cmd.SetVertexBuffer(_vertexBuffer.Gpu);
            cmd.SetIndexBuffer(_indexBuffer.Gpu);
            cmd.SetDescriptorSet(0, _guiDescriptor);

            var clipOffset = drawData.DisplayPos;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                int vtxSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vtxSize > _vertexBufferSize)
                {
                    _vertexBuffer.Dispose();
                    _vertexBufferSize = (int)Math.Max(_vertexBufferSize * 1.5f, vtxSize);
                    _vertexBuffer = new VertexBuffer<ImGuiVertex>(Engine.GraphicsDevice, _vertexBufferSize, ImGuiVertex.VertexInfo, ResourceUsage.Dynamic);
                    cmd.SetVertexBuffer(_vertexBuffer.Gpu);
                }

                int idxSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (idxSize > _indexBufferSize)
                {
                    _indexBuffer.Dispose();
                    _indexBufferSize = (int)Math.Max(_indexBufferSize * 1.5f, idxSize);
                    _indexBuffer = new IndexBuffer(Engine.GraphicsDevice, _indexBufferSize, ResourceUsage.Dynamic, IndexFormat.UInt16);
                    cmd.SetIndexBuffer(_indexBuffer.Gpu);
                }

                _vertexBuffer.Update(cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size);
                _indexBuffer.Update(cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size);

                for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdI];

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

                    //cmd.SetScissor(x1, y1, x2, y2);
                    Engine.Renderer.PushScissor(cmd, new ScissorRect()
                    {
                        X = x1,
                        Y = y1,
                        Width = x2,
                        Height = y2,
                    });

                    var texture = Engine.Instance.TextureManager.Get((ulong)pcmd.TextureId);
                    _guiDescriptor.SetTexture(0, texture.GPU);
                    _guiDescriptor.SetSampler(0, _defaultSampler.Gpu);

                    cmd.SetDescriptorSet(0, _guiDescriptor);
                    cmd.DrawIndexed((int)pcmd.ElemCount, (int)pcmd.IdxOffset, (int)pcmd.VtxOffset);

                    Engine.Renderer.PopScissor(cmd);
                }
            }

            Engine.Renderer.PopViewport(cmd);
        }

        public bool Handle(ref InputEvent e)
        {
            if ((OpenTK.Windowing.Common.CursorState)Engine.Cursor.GetCursorState() == OpenTK.Windowing.Common.CursorState.Grabbed)
                return false;

            if (e.DeviceType == InputDeviceType.Mouse)
            {
                if (ImGui.GetIO().WantCaptureMouse)
                    return true;
            }

            if (e.DeviceType == InputDeviceType.Keyboard)
            {
                if (ImGui.GetIO().WantCaptureKeyboard)
                    return true;
            }

            return false;
        }
    }
}