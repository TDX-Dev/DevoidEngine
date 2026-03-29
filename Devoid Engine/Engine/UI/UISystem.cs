using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.UI
{
    public static class UISystem
    {
        public static List<CanvasNode> Roots = new();

        public static UINode FocusedNode { get; private set; }
        public static UINode HoveredNode { get; private set; }
        public static UINode PressedNode { get; private set; }

        private static Material uiMaterial;
        private static Material textMaterial;

        public static Mesh QuadMesh;

        public static MaterialInstance UIMaterial => new MaterialInstance(uiMaterial);
        public static MaterialInstance TextMaterial => new MaterialInstance(textMaterial);

        public static CameraData ScreenData;
        private static IUniformBuffer screenDataBuffer;

        private static Vector2 previousMouse;
        private static bool isDragging;
        private static Vector2 dragStartMouse;

        public static RenderState RenderState = new RenderState()
        {
            BlendMode = BlendMode.AlphaBlend,
            CullMode = CullMode.Back,
            DepthTest = DepthTest.LessEqual,
            DepthWrite = false,
        };

        public static void Resize(int width, int height)
        {
            CreateCameraData(width, height);
            screenDataBuffer.SetData(ScreenData);
        }

        public static void Initialize()
        {
            QuadMesh = new Mesh();
            QuadMesh.SetVertices(Primitives.GetQuadVertex());

            uiMaterial = new Material(
                new Shader("Engine/Content/Shaders/UI/basic")
            );

            textMaterial = new Material(
                new Shader(
                    "Engine/Content/Shaders/UI/basic.vert.hlsl",
                    "Engine/Content/Shaders/UI/sdf_text.frag.hlsl"
                )
            );

            CreateCameraData((int)Screen.Size.X, (int)Screen.Size.Y);

            screenDataBuffer =
                Renderer.GraphicsDevice.BufferFactory.CreateUniformBuffer(
                    Marshal.SizeOf<CameraData>(), BufferUsage.Dynamic);

            screenDataBuffer.SetData(ScreenData);

            foreach (var root in Roots)
                root.Initialize();

            Input.Router.Push(new UIInputLayer());
        }

        public static void AddRoot(CanvasNode node)
        {
            Roots.Add(node);
            node.Initialize();
        }

        public static void RemoveRoot(CanvasNode node)
        {
            Roots.Remove(node);
        }

        public static void SetFocus(UINode node)
        {
            if (FocusedNode == node)
                return;

            FocusedNode?.OnBlur();
            FocusedNode = node;
            FocusedNode?.OnFocus();
        }

        public static void Update(float deltaTime)
        {
            Vector2 screen = Screen.Size;

            // -------------------------
            // 1. Update logic
            // -------------------------
            foreach (var root in Roots)
            {
                root.Update(deltaTime);
            }

            // -------------------------
            // 2. Layout pass
            // -------------------------
            foreach (var root in Roots)
            {
                root.Measure(screen);
                root.Arrange(new UITransform(Vector2.Zero, screen));
            }

            // -------------------------
            // 3. Interaction pass
            // -------------------------
            //HandleInput();
        }

        public static void MouseMove(Vector2 mouse)
        {
            Vector2 mouseDelta = mouse - previousMouse;
            previousMouse = mouse;

            UINode previousHovered = HoveredNode;
            HoveredNode = null;

            for (int i = Roots.Count - 1; i >= 0; i--)
            {
                HoveredNode = HitTest(Roots[i], mouse);
                if (HoveredNode != null)
                    break;
            }

            if (previousHovered != HoveredNode)
            {
                previousHovered?.OnMouseLeave();
                HoveredNode?.OnMouseEnter();
            }

            // Drag logic
            if (PressedNode != null)
            {
                if (!isDragging)
                {
                    if (Vector2.Distance(mouse, dragStartMouse) > 3f)
                    {
                        isDragging = true;
                        PressedNode.OnDragStart(mouse);
                    }
                    else
                    {
                        PressedNode.OnMouseHeld();   // ← add this
                    }
                }

                if (isDragging)
                {
                    PressedNode.OnDrag(mouse, mouseDelta);
                }
            }
        }
        public static void MouseDown(Vector2 mouse)
        {
            PressedNode = HoveredNode;
            dragStartMouse = mouse;
            isDragging = false;

            if (PressedNode != null)
            {
                SetFocus(PressedNode);
                PressedNode.OnMouseDown();
            }
        }
        public static void MouseUp(Vector2 mouse)
        {
            if (PressedNode == null)
                return;

            if (isDragging)
            {
                PressedNode.OnDragEnd(mouse);
            }

            PressedNode.OnMouseUp();

            if (!isDragging && HoveredNode == PressedNode)
            {
                PressedNode.OnClick();
            }

            PressedNode = null;
            isDragging = false;
        }


        private static void HandleInput()
        {
            Vector2 mouse = InputBackendMousePosition();
            Vector2 mouseDelta = mouse - previousMouse;
            previousMouse = mouse;

            UINode previousHovered = HoveredNode;
            HoveredNode = null;

            for (int i = Roots.Count - 1; i >= 0; i--)
            {
                HoveredNode = HitTest(Roots[i], mouse);
                if (HoveredNode != null)
                    break;
            }

            if (previousHovered != HoveredNode)
            {
                previousHovered?.OnMouseLeave();
                HoveredNode?.OnMouseEnter();
            }

            bool mouseDown = Input.State.GetDown(InputDeviceType.Mouse, (ushort)MouseButton.Left);
            bool mouseHeld = Input.State.Get(InputDeviceType.Mouse, (ushort)MouseButton.Left) > 0;

            // Press
            if (mouseDown)
            {
                PressedNode = HoveredNode;
                dragStartMouse = mouse;
                isDragging = false;

                if (PressedNode != null)
                {
                    SetFocus(PressedNode);
                    PressedNode.OnMouseDown();
                }
            }

            // Drag detection
            if (mouseHeld && PressedNode != null)
            {
                if (!isDragging)
                {
                    if (Vector2.Distance(mouse, dragStartMouse) > 3f)
                    {
                        isDragging = true;
                        PressedNode.OnDragStart(mouse);
                    }
                }

                if (isDragging)
                {
                    PressedNode.OnDrag(mouse, mouseDelta);
                }
            }

            // Release
            if (!mouseHeld && PressedNode != null)
            {
                if (isDragging)
                {
                    PressedNode.OnDragEnd(mouse);
                }

                PressedNode.OnMouseUp();

                if (!isDragging && HoveredNode == PressedNode)
                {
                    PressedNode.OnClick();
                }

                PressedNode = null;
                isDragging = false;
            }
        }

        private static Vector2 InputBackendMousePosition()
        {
            float x = Input.State.Get(InputDeviceType.Mouse, (ushort)MouseAxis.X);
            float y = Input.State.Get(InputDeviceType.Mouse, (ushort)MouseAxis.Y);

            return new Vector2(x, y);
        }

        private static UINode HitTest(UINode node, Vector2 position)
        {
            if (!node.Visible)
                return null;

            for (int i = node._children.Count - 1; i >= 0; i--)
            {
                var hit = HitTest(node._children[i], position);
                if (hit != null)
                    return hit;
            }

            if (!node.BlockInput)
                return null;

            var rect = node.Rect;
            if (node.Rect == null)
                return null;

            if (position.X >= rect.position.X &&
                position.X <= rect.position.X + rect.size.X &&
                position.Y >= rect.position.Y &&
                position.Y <= rect.position.Y + rect.size.Y)
            {
                return node;
            }

            return null;
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

        public static Matrix4x4 BuildModel(UITransform t)
        {
            return
                Matrix4x4.CreateScale(t.size.X, t.size.Y, 1f) *
                Matrix4x4.CreateTranslation(t.position.X, t.position.Y, 0f);
        }

        public static Matrix4x4 BuildTranslationModel(UITransform t)
        {
            return Matrix4x4.CreateTranslation(
                t.position.X,
                t.position.Y,
                0f
            );
        }

        //public bool Handle(InputEvent e)
        //{
        //    if (HoveredNode != null)
        //    {

        //        return true; // consume
        //    }

        //    return false;
        //}
    }
}