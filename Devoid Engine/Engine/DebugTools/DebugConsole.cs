using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    public class DebugConsole : Layer, IInputLayer
    {
        CanvasNode debugCanvas = null!;

        ContainerNode ConsoleContainer = null!;
        FlexboxNode InnerContainer = null!;
        ScrollNode logsContainer = null!;
        InputFieldNode inputField = null!;

        RenderState debugRenderState = new RenderState()
        {
            BlendMode = DevoidGPU.BlendMode.AlphaBlend,
            CullMode = DevoidGPU.CullMode.None,
            DepthTest = DevoidGPU.DepthTest.Disabled,
            DepthWrite = false,
        };

        CursorState prevCursorState;

        public bool Handle(InputEvent e)
        {
            if (e.Control == (ushort)InputSystem.InputDevices.Keys.F7 && e.Value == 1f)
            {
                debugCanvas.Visible = !debugCanvas.Visible;

                if (debugCanvas.Visible)
                {
                    prevCursorState = Cursor.GetCursorState();

                    Cursor.SetCursorState(CursorState.Normal);

                    Input.State.Clear();

                    inputField.BlockInput = true;
                }
                else
                {
                    Cursor.SetCursorState(prevCursorState);
                    inputField.BlockInput = false;
                }
            }
            return false;
            //return debugCanvas.Visible;
        }

        public override void OnAttach()
        {
            #region Command UI Setup
            debugCanvas = new CanvasNode();
            debugCanvas.Align = AlignItems.Start;
            debugCanvas.Justify = JustifyContent.Start;

            ConsoleContainer = new ContainerNode()
            {
                ParticipatesInLayout = false,
                Size = new Vector2(640, 480),
                Offset = new Vector2(100, 100),
                Padding = Padding.GetAll(10),
                Gap = 10
            };

            InnerContainer = new FlexboxNode()
            {
                Direction = FlexDirection.Column,
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 1
                }
            };

            logsContainer = new ScrollNode()
            {
                Align = AlignItems.Start,
                Justify = JustifyContent.Start,
                Direction = FlexDirection.Column,
                Padding = Padding.GetAll(5),
                Gap = 5,
                Layout = new LayoutOptions()
                {
                    FlexGrowMain = 1,
                    FlexGrowCross = 1
                },
            };

            inputField = new InputFieldNode()
            {
                HintText = "Type commands here..."
            };

            ConsoleContainer.Add(InnerContainer);
            InnerContainer.Add(logsContainer);
            InnerContainer.Add(inputField);

            ConsoleContainer.AddStyleBoxOverride(
                StyleKeys.Normal,
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.6f),
                    BorderColor = new Vector4(0.1f, 0.1f, 0.1f, 1f),
                    BorderRadius = new Vector4(10)
                }
            );


            debugCanvas.Add(ConsoleContainer);
            UISystem.AddRoot(debugCanvas);
            Input.Router.Push(this);
            debugCanvas.Visible = false;
            inputField.BlockInput = false;
            #endregion

            inputField.OnSubmit = OnInputFieldSubmit;



        }

        public override void OnPostRender()
        {
            var defaultCamera = SceneManager.CurrentScene?.GetDefaultCamera3D()?.Camera;
            if (defaultCamera == null) return;

            List<RenderItem> renderItems = new List<RenderItem>();
            debugCanvas.Render(renderItems, Matrix4x4.Identity, 0);

            Framebuffer renderTarget = defaultCamera.RenderTarget!;
            Renderer.SetupCamera(UISystem.ScreenData);
            renderTarget.Bind();
            Renderer.ExecuteDrawList(renderItems, debugRenderState);

        }

        public void OnInputFieldSubmit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length == 0)
                    return;

                string path = tokens[0];

                object?[] args = new object?[tokens.Length - 1];

                for (int i = 1; i < tokens.Length; i++)
                {
                    args[i - 1] = ParseToken(tokens[i]);
                }
                AddLog($"> {value}");
                var result = ConsoleExecutor.Execute(path, args);

                if (result != null)
                    AddLog(result.ToString()!);
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }

            inputField.Text = "";
        }

        void AddLog(string text)
        {
            logsContainer.Add(new LabelNode(text));
        }

        object ParseToken(string token)
        {
            if (int.TryParse(token, out int i))
                return i;

            if (float.TryParse(token, out float f))
                return f;

            if (bool.TryParse(token, out bool b))
                return b;

            return token;
        }

    }
}
