using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;
using Plane = DevoidEngine.Engine.Utilities.Plane;

namespace DevoidEngine.Engine.UI
{
    public static class UISystem
    {
        public static UITheme DefaultTheme;

        public static List<CanvasNode> Roots = new();

        public static UINode FocusedNode { get; private set; }
        public static UINode HoveredNode { get; private set; }
        public static UINode PressedNode { get; private set; }

        public static float OrderEpsilon = 0.01f;
        public static bool DebugDraw = true;
        public static float UIScale = 1f;

        private static Material uiMaterial;
        private static Material textMaterial;
        private static Material debugUIRectMaterial;

        public static Mesh QuadMesh;

        public static MaterialInstance UIMaterial => new MaterialInstance(uiMaterial);
        public static MaterialInstance TextMaterial => new MaterialInstance(textMaterial);
        public static MaterialInstance DebugMaterial => new MaterialInstance(debugUIRectMaterial);

        public static CameraData ScreenData;
        private static IUniformBuffer screenDataBuffer;

        public static Vector2 mousePosition;
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
            DefaultTheme = ThemeDefaults.InitializeDefaultTheme();

            QuadMesh = new Mesh();
            QuadMesh.SetVertices(Primitives.GetQuadVertex());

            uiMaterial = new Material(
                new Shader("Engine/Content/Shaders/UI/basic")
            );

            debugUIRectMaterial = new Material(
                new Shader("Engine/Content/Shaders/UI/debugUI")
            );

            debugUIRectMaterial.SetVector4("COLOR", new Vector4(1,1,1, 0.01f));
            debugUIRectMaterial.SetVector4("BORDER_COLOR", new Vector4(1,1,1, 0.2f));
            debugUIRectMaterial.SetFloat("BORDER_THICKNESS", 1.5f);
            debugUIRectMaterial.SetFloat("DASH_SIZE", 6);
            debugUIRectMaterial.SetFloat("GAP_SIZE", 4);
            debugUIRectMaterial.SetInt("DEBUG_DASHED", 1);

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

            foreach (var root in Roots)
            {

                if (root.RenderMode == CanvasRenderMode.ScreenSpace)
                {
                    Vector2 scaledScreen = Screen.Size / UISystem.UIScale;

                    root.Measure(scaledScreen);
                    root.Arrange(new UITransform(Vector2.Zero, scaledScreen));
                }
                else
                {
                    //root.Measure(new Vector2(float.PositiveInfinity));

                    Vector2 canvasSize = root.DesiredSize;
                    root.Measure(canvasSize);
                    root.Arrange(new UITransform(Vector2.Zero, canvasSize));
                }

                root.Update(deltaTime);
            }
        }

        static bool TryProjectMouseToCanvas(
            CanvasNode canvas,
            Vector2 mouse,
            out Vector2 canvasPos)
        {
            canvasPos = default;

            Vector2 size = canvas.DesiredSize;

            Matrix4x4 canvasMatrix = canvas.GetWorldCanvasMatrix();

            if (DebugDraw)
            {
                Vector3 bl = Vector3.Transform(new Vector3(0, 0, 0), canvasMatrix);
                Vector3 br = Vector3.Transform(new Vector3(size.X, 0, 0), canvasMatrix);
                Vector3 tl = Vector3.Transform(new Vector3(0, size.Y, 0), canvasMatrix);
                Vector3 tr = Vector3.Transform(new Vector3(size.X, size.Y, 0), canvasMatrix);

                DrawMarker(bl);
                DrawMarker(br);
                DrawMarker(tl);
                DrawMarker(tr);
            }

            Ray ray = BuildMouseRay(mouse);

            Vector3 normal = Vector3.Normalize(
                Vector3.TransformNormal(Vector3.UnitZ, canvasMatrix)
            );

            Vector3 point = Vector3.Transform(Vector3.Zero, canvasMatrix);

            Plane plane = new Plane(
                normal,
                -Vector3.Dot(normal, point)
            );

            if (!RayPlane(ray, plane, out Vector3 hit))
                return false;

            if (DebugDraw)
            {
                DrawMarker(hit);
            }

            if (!Matrix4x4.Invert(canvasMatrix, out Matrix4x4 inv))
                return false;

            Vector3 local = Vector3.Transform(hit, inv);

            if (local.X < 0 || local.X > size.X ||
                local.Y < 0 || local.Y > size.Y)
                return false;

            canvasPos = new Vector2(local.X, local.Y);

            return true;
        }

        public static void DrawMarker(Vector3 pos)
        {
            Matrix4x4 m =
                Matrix4x4.CreateScale(0.05f) *
                Matrix4x4.CreateTranslation(pos);

            DebugRenderSystem.DrawCube(m);
        }

        public static void DrawRay(Vector3 origin, Vector3 dir, float length)
        {
            Vector3 center = origin + dir * length * 0.5f;

            Matrix4x4 model =
                Matrix4x4.CreateScale(0.01f, 0.01f, length) *
                Matrix4x4.CreateWorld(center, dir, Vector3.UnitY);

            DebugRenderSystem.DrawCube(model);
        }

        static bool RayPlane(Ray ray, Plane plane, out Vector3 hit)
        {
            hit = default;

            float denom = Vector3.Dot(plane.Normal, ray.Direction);

            if (MathF.Abs(denom) < 0.0001f)
                return false;

            float t = -(Vector3.Dot(plane.Normal, ray.Origin) + plane.D) / denom;

            if (t < 0)
                return false;

            hit = ray.Origin + ray.Direction * t;

            return true;
        }

        public static void MouseMove(Vector2 mouse)
        {
            Vector2 mouseDelta = mouse - previousMouse;
            previousMouse = mouse;

            UINode previousHovered = HoveredNode;
            HoveredNode = null;

            for (int i = Roots.Count - 1; i >= 0; i--)
            {
                CanvasNode canvas = Roots[i];

                Vector2 pos;

                if (canvas.RenderMode == CanvasRenderMode.ScreenSpace)
                {
                    pos = mouse;
                }
                else
                {
                    if (!TryProjectMouseToCanvas(canvas, mouse, out pos))
                        continue;
                }

                HoveredNode = HitTest(canvas, pos);

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

            mousePosition = mouse;
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

        public static void MouseScroll(Vector2 scroll)
        {
            if (HoveredNode != null)
            {
                HoveredNode.OnMouseScroll(scroll);
            }
        }
        static Ray BuildMouseRay(Vector2 mouse)
        {
            var cam = SceneManager.CurrentScene.GetDefaultCamera3D().Camera;

            Vector2 screen = Screen.Size;

            float x = (2f * mouse.X) / screen.X - 1f;
            float y = 1f - (2f * mouse.Y) / screen.Y;

            Vector4 nearClip = new Vector4(x, y, 0f, 1f);
            Vector4 farClip = new Vector4(x, y, 1f, 1f);

            Matrix4x4 invProj = cam.GetInverseProjectionMatrix();
            Matrix4x4 invView = cam.GetInverseViewMatrix();

            Vector4 nearView = Vector4.Transform(nearClip, invProj);
            Vector4 farView = Vector4.Transform(farClip, invProj);

            nearView /= nearView.W;
            farView /= farView.W;

            Vector4 nearWorld = Vector4.Transform(nearView, invView);
            Vector4 farWorld = Vector4.Transform(farView, invView);

            Vector3 origin = new Vector3(nearWorld.X, nearWorld.Y, nearWorld.Z);
            Vector3 dir = Vector3.Normalize(
                new Vector3(farWorld.X, farWorld.Y, farWorld.Z) - origin
            );

            return new Ray(origin, dir);
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