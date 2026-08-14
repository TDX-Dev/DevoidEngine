using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.Rendering;
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
        private static readonly Keys[] imGuiKeys =
        [
            Keys.Tab, Keys.Left, Keys.Right, Keys.Up, Keys.Down,
            Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End, Keys.Insert,
            Keys.Delete, Keys.Backspace, Keys.Space, Keys.Enter, Keys.Escape,
            Keys.A, Keys.C, Keys.S, Keys.V, Keys.X, Keys.Y, Keys.Z
        ];

        private readonly Shader guiShader;
        private ImShaderData imShaderData = new();
        private readonly UniformBuffer shaderConstantBuffer;

        private VertexBuffer<ImGuiVertex> vertexBuffer;
        private IndexBuffer indexBuffer;

        private readonly IDescriptorSet guiDescriptor;

        private int vertexBufferSize = 2000;
        private int indexBufferSize = 4000;

        private Texture? fontTexture;
        private readonly Sampler defaultSampler;

        private readonly RenderTarget imguiRenderTarget;
        private Texture? imguiRenderTexture;

        private int prevWidth;
        private int prevHeight;

        private int fontsCount = -1;
        private ImFontPtr defaultFont;

        public Action? OnGUI { get; set; }
        public float FooterHeight { get; set; } = 24f;
        public float ToolbarHeight { get; set; } = 28f;

        public ImGuiRenderer()
        {
            guiShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/imgui_shader.dsd");
            guiDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(guiShader.DefaultPass.DescriptorLayout);
            shaderConstantBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<ImShaderData>());

            vertexBuffer = new VertexBuffer<ImGuiVertex>(Engine.GraphicsDevice, vertexBufferSize, ImGuiVertex.VertexInfo, ResourceUsage.Dynamic);
            indexBuffer = new IndexBuffer(Engine.GraphicsDevice, indexBufferSize, ResourceUsage.Dynamic, IndexFormat.UInt16);

            defaultSampler = Sampler.Create(new SamplerDescription()
            {
                MinFilter = FilterMode.Linear,
                MagFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                AddressU = WrapMode.ClampToEdge,
                AddressV = WrapMode.ClampToEdge,
                AddressW = WrapMode.ClampToEdge,
                MaxAnisotropy = 1,
                MipLODBias = 0.0f,
                MinLOD = 0.0f,
                MaxLOD = 0.0f
            });

            imguiRenderTarget = RenderTarget.Create(1);
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
            defaultFont = font;
        }

        public unsafe ImFontPtr LoadIconFont(string path, int size, (ushort, ushort) range)
        {
            ImFontConfigPtr config = new(ImGuiNative.ImFontConfig_ImFontConfig())
            {
                GlyphOffset = new Vector2(0, 4),
                GlyphMinAdvanceX = size,
                MergeMode = true,
                PixelSnapH = true
            };

            ushort[] ranges = [range.Item1, range.Item2, 0];

            fixed (ushort* rangePtr = ranges)
            {
                try
                {
                    return ImGui.GetIO().Fonts.AddFontFromFileTTF(path, size, config, (IntPtr)rangePtr);
                }
                finally
                {
                    config.Destroy();
                }
            }
        }

        private unsafe void UpdateMonitors()
        {
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();

            if (platformIO.NativePtr->Monitors.Data != IntPtr.Zero)
            {
                Marshal.FreeHGlobal((nint)platformIO.NativePtr->Monitors.Data);
            }

            int count = Monitors.GetMonitors().Count;
            if (count == 0)
            {
                platformIO.NativePtr->Monitors = new ImVector();
                return;
            }

            IntPtr data = Marshal.AllocHGlobal(Unsafe.SizeOf<ImGuiPlatformMonitor>() * count);
            platformIO.NativePtr->Monitors = new ImVector(count, count, data);

            var monitors = Monitors.GetMonitors();
            for (int i = 0; i < monitors.Count; i++)
            {
                MonitorInfo monitor = monitors[i];
                ImGuiPlatformMonitorPtr imguiMonitor = platformIO.Monitors[i];

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

            fontTexture?.Dispose(); // Optimize: Prevents memory leak when fonts change/rebuild

            fontTexture = Texture.Create2D(width, height, TextureFormat.RGBA8_UNorm, TextureUsage.ShaderResource);
            fontTexture.GPU.Update(managedPixels);

            io.Fonts.SetTexID((nint)fontTexture.ID);
        }

        public void UpdateDisplay(Window window)
        {
            if (window.ClientSize.X == 0 || window.ClientSize.Y == 0)
                return;

            ImGui.GetIO().DisplaySize = new(window.ClientSize.X, window.ClientSize.Y);

            if (window.ClientSize.X != prevWidth || window.ClientSize.Y != prevHeight)
            {
                imguiRenderTexture?.Dispose(); // Optimize: Resolves memory leak upon window resize

                imguiRenderTexture = Texture.Create2D(
                    window.ClientSize.X,
                    window.ClientSize.Y,
                    TextureFormat.RGBA8_UNorm,
                    TextureUsage.RenderTarget | TextureUsage.ShaderResource
                );

                prevWidth = window.ClientSize.X;
                prevHeight = window.ClientSize.Y;
            }

            if (imguiRenderTexture != null)
            {
                imguiRenderTarget.SetColorAttachment(0, imguiRenderTexture);
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
                Keys.A => ImGuiKey.A,
                Keys.C => ImGuiKey.C,
                Keys.S => ImGuiKey.S,
                Keys.V => ImGuiKey.V,
                Keys.X => ImGuiKey.X,
                Keys.Y => ImGuiKey.Y,
                Keys.Z => ImGuiKey.Z,
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

            foreach (Keys key in imGuiKeys)
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

            if (io.Fonts.Fonts.Size != fontsCount)
            {
                ConfigureFontAtlas();
                fontsCount = io.Fonts.Fonts.Size;
            }
        }

        public void BeginFrame(WindowSurface surface, float delta)
        {
            UpdatePerFrameParameters(delta);
            UpdateDisplay(surface.Window);
            UpdateInput(surface.Window);

            ImGui.NewFrame();
            ImGui.PushFont(defaultFont);

            CreateDockspace();

            OnGUI?.Invoke();

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

            imShaderData.ProjectionMatrix = mvp;
            imShaderData.DpiScaling = 1.0f;

            cmd.SetFramebuffer(surface.Framebuffer);

            Engine.Renderer.PushViewport(cmd, new ViewportRect()
            {
                X = 0,
                Y = 0,
                Width = fbWidth,
                Height = fbHeight,
            });

            shaderConstantBuffer.Update(imShaderData);
            guiDescriptor.SetUniformBuffer(0, shaderConstantBuffer.GPU);

            IPipeline guiPipeline = guiShader.DefaultPass.GetPipeline(Engine.GraphicsDevice, ImGuiVertex.VertexInfo);
            cmd.SetPipeline(guiPipeline);

            cmd.SetVertexBuffer(vertexBuffer.GPU);
            cmd.SetIndexBuffer(indexBuffer.GPU);
            cmd.SetDescriptorSet(0, guiDescriptor);

            var clipOffset = drawData.DisplayPos;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                int vtxSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vtxSize > vertexBufferSize)
                {
                    vertexBuffer.Dispose();
                    vertexBufferSize = (int)Math.Max(vertexBufferSize * 1.5f, vtxSize);
                    vertexBuffer = new VertexBuffer<ImGuiVertex>(Engine.GraphicsDevice, vertexBufferSize, ImGuiVertex.VertexInfo, ResourceUsage.Dynamic);
                    cmd.SetVertexBuffer(vertexBuffer.GPU);
                }

                int idxSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (idxSize > indexBufferSize)
                {
                    indexBuffer.Dispose();
                    indexBufferSize = (int)Math.Max(indexBufferSize * 1.5f, idxSize);
                    indexBuffer = new IndexBuffer(Engine.GraphicsDevice, indexBufferSize, ResourceUsage.Dynamic, IndexFormat.UInt16);
                    cmd.SetIndexBuffer(indexBuffer.GPU);
                }

                vertexBuffer.Update(cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size);
                indexBuffer.Update(cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size);

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

                    //cmd.SetScissor(x1, y1, x2, y2);
                    Engine.Renderer.PushScissor(cmd, new ScissorRect()
                    {
                        X = x1,
                        Y = y1,
                        Width = x2,
                        Height = y2,
                    });

                    var texture = Engine.Instance.TextureManager.Get((ulong)pcmd.TextureId);
                    guiDescriptor.SetTexture(0, texture.GPU);
                    guiDescriptor.SetSampler(0, defaultSampler.GPU);

                    cmd.DrawIndexed((int)pcmd.ElemCount, (int)pcmd.IdxOffset, (int)pcmd.VtxOffset);

                    Engine.Renderer.PopScissor(cmd);
                }
            }

            Engine.Renderer.PopViewport(cmd);
        }

        public bool Handle(ref InputEvent e)
        {
            if (Engine.Cursor.GetCursorState() == OpenTK.Windowing.Common.CursorState.Grabbed)
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