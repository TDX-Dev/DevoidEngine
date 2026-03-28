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
        public static List<UINode> Roots = new();

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
        }

        public static void AddRoot(UINode node)
        {
            Roots.Add(node);
            node.Initialize();
        }

        public static void RemoveRoot(UINode node)
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
            HandleInput();
        }

        private static void HandleInput()
        {
            Vector2 mouse = Input.State.Get(InputDeviceType.Mouse, 0) != 0
                ? Vector2.Zero
                : Vector2.Zero;

            mouse = InputBackendMousePosition();

            HoveredNode = null;

            for (int i = Roots.Count - 1; i >= 0; i--)
            {
                HoveredNode = HitTest(Roots[i], mouse);
                if (HoveredNode != null)
                    break;
            }

            bool mouseDown = Input.GetActionDown("MouseLeft");
            bool mouseHeld = Input.GetAction("MouseLeft") > 0;

            // Mouse press
            if (mouseDown)
            {
                PressedNode = HoveredNode;

                if (PressedNode != null)
                {
                    SetFocus(PressedNode);
                    PressedNode.OnMouseDown();
                }
            }

            // Mouse release
            if (!mouseHeld && PressedNode != null)
            {
                PressedNode.OnMouseUp();

                if (HoveredNode == PressedNode)
                {
                    PressedNode.OnClick();
                }

                PressedNode = null;
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