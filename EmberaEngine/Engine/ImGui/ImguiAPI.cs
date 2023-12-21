using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System.Runtime.InteropServices;

namespace EmberaEngine.Engine.Imgui
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImguiAPI : IDisposable
    {
        private bool _frameBegun;

        // Veldrid objects
        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        public int windowWidth, windowHeight;

        public ImFontPtr _CurrentFont;
        public static ImFontPtr ICONFONT;
        private Texture _fontTexture;
        private Shader _guiShader;
        private GameWindow _GameWindow;
        private int FontCount;
        Matrix4 mvp;
        uint DockspaceID;
        

        private System.Numerics.Vector2 Previous_Size = new System.Numerics.Vector2();

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImguiAPI(GameWindow gameWindow, int width, int height)
        {

            windowWidth = width;
            windowHeight = height;

            _GameWindow = gameWindow;
            var io = ImGui.GetIO();
            


            FontCount = io.Fonts.Fonts.Size;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;


            RecreateFontDeviceTexture();

            ImGui.StyleColorsDark();

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);
        }

        static unsafe ImFontPtr LoadIconFont(string path, int size, (ushort, ushort) range)
        {
            ImFontConfigPtr configuration = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());

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
                return ImGui.GetIO().Fonts.AddFontFromFileTTF(path, (float)size, configuration, rangeHandle.AddrOfPinnedObject());
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

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            ImguiUtils.CreateVertexArray("ImGui", out _vertexArray);

            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            ImguiUtils.CreateVertexBuffer("ImGui", out _vertexBuffer);
            ImguiUtils.CreateElementBuffer("ImGui", out _indexBuffer);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // If using opengl 4.5 this could be a better way of doing it so that we are not modifying the bound buffers
            // GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            // GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();
            _guiShader = new Shader("Engine/Imgui/shaders/GUI");

            GL.BindVertexArray(_vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);

            int stride = Unsafe.SizeOf<ImDrawVert>();

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // We don't need to unbind the element buffer as that is connected to the vertex array
            // And you should not touch the element buffer when there is no vertex array bound.

            //ImguiUtils.CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = new Texture("ImGui Text Atlas", width, height, pixels);
            _fontTexture.SetMagFilter(OpenTK.Graphics.OpenGL.TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(OpenTK.Graphics.OpenGL.TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture.GLTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            ImGui.PopFont();
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        float elapsedTime = 0f;

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();
            _frameBegun = true;
            
            ImGui.NewFrame();
            ImGui.PushFont(_CurrentFont);

            elapsedTime += deltaSeconds;

            if (elapsedTime > 10f)
            {
                //ImGui.SaveIniSettingsToDisk(AppContext.BaseDirectory + "Engine/Imgui/misc/imgui.ini");
                elapsedTime = 0;
            }

        }


        //int[] param = new int[];

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
            if (io.Fonts.Fonts.Size != FontCount)
            {
                //Console.WriteLine("Remaking Atlas, Font Count " + io.Fonts.Fonts.Size);
                RecreateFontDeviceTexture();
                FontCount = io.Fonts.Fonts.Size;
            }
        }

        public void OnResize(int width, int height)
        {

            windowWidth = width;
            windowHeight = height;

            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new System.Numerics.Vector2(
                    (float)windowWidth / _scaleFactor.X,
                    (float)windowHeight / _scaleFactor.Y
                );

            mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f
            );
        }

        readonly List<char> pressedChars = new List<char>();

        private void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = _GameWindow.MouseState.GetSnapshot();
            KeyboardState KeyboardState = _GameWindow.KeyboardState.GetSnapshot();

            io.MouseDown[0] = MouseState.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = MouseState.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = MouseState.IsButtonDown(MouseButton.Middle);

            var screenPoint = MouseState.Position;
            io.MousePos = new System.Numerics.Vector2(screenPoint.X, screenPoint.Y);

            io.MouseWheel = MouseState.Scroll.Y - MouseState.PreviousScroll.Y;
            io.MouseWheelH = MouseState.Scroll.X - MouseState.PreviousScroll.X;


            foreach (OpenTK.Windowing.GraphicsLibraryFramework.Keys key in Enum.GetValues(typeof(OpenTK.Windowing.GraphicsLibraryFramework.Keys)))
            {
                if (key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Unknown)
                    continue;

                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            for (int i = 0; i < pressedChars.Count; i++)
            {
                io.AddInputCharacter(pressedChars[i]);
            }

            pressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftSuper) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightSuper);
        }



        private char KeyToChar(OpenTK.Windowing.GraphicsLibraryFramework.Keys e, bool shift = false)
        {
            var str = e.ToString();

            if (str.Length == 0) { return ' '; }

            if (str.Length == 1)
            {
                return shift ? str[0] : str.ToLower()[0];
            }


            else if ((str.StartsWith("D") || str.StartsWith( "KeyPad")) && (str.Length == 7 || str.Length == 2))
            {
                return str[str.Length - 1];
            }


            switch (e)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backslash:
                    return '\\';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Slash:
                    return '/';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftBracket:
                    return '(';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightBracket:
                    return ')';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Comma:
                    return ',';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space:
                    return ' ';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.GraveAccent:
                    return '`';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Equal:
                    return '=';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEqual:
                    return '=';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadAdd:
                    return '+';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadDecimal:
                    return '.';
            }

            return '\0';
        }


        public void PressChar(char keyChar)
        {
            pressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _GameWindow.Size.X, _GameWindow.Size.Y);
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
            if (totalVBSize > _vertexBufferSize)
            {
                int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, totalVBSize);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                _vertexBufferSize = newSize;

                //Console.WriteLine($"Resized vertex buffer to new size {_vertexBufferSize}");
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBufferSize)
            {
                int newSize = (int)Math.Max(_indexBufferSize * 1.5f, totalIBSize);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _indexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                _indexBufferSize = newSize;

                //Console.WriteLine($"Resized index buffer to new size {_indexBufferSize}");
            }


            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(vertexOffsetInVertices * Unsafe.SizeOf<ImDrawVert>()), cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);

                //ImguiUtils.CheckGLError($"Data Vert {i}");

                GL.BindBuffer(BufferTarget.ArrayBuffer, _indexBuffer);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(indexOffsetInElements * sizeof(ushort)), cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

                //ImguiUtils.CheckGLError($"Data Idx {i}");

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            
            
            _guiShader.Use();
            _guiShader.SetMatrix4("projection_matrix", mvp, false);
            _guiShader.SetInt("in_fontTexture", 0);
            //ImguiUtils.CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            //ImguiUtils.CheckGLError("VAO");

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        //ImguiUtils.CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _GameWindow.Size.Y - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        //ImguiUtils.CheckGLError("Scissor");

                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                        //ImguiUtils.CheckGLError("Draw");
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        ImGuiWindowFlags windowFlags = /*ImGuiWindowFlags.MenuBar |*/ ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus /*| ImGuiWindowFlags.NoBackground*/ | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.DockNodeHost;
        bool firstFrame = true;

        public void SetUpDockspace()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0,0));
            ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(_GameWindow.Size.X, _GameWindow.Size.Y));
            ImGui.Begin("Dockspace", windowFlags);
            
            if (firstFrame) { DockspaceID = ImGui.GetID("Dockspace"); firstFrame = false; }

            ImGui.DockSpace(ImGui.GetID("Dockspace"));
            //ImGui.End();
            ImGui.PopStyleVar();
        }

        

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _fontTexture.Dispose();
            _guiShader.Dispose();
        }
    }
}

