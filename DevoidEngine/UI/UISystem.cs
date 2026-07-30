using DevoidEngine.Core;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidEngine.UI.Theme;
using DevoidEngine.UI.UINodes;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.UI
{
    public class UISystem
    {
        public Vector2 mousePosition;
        public UITheme DefaultTheme = null!;
        public MaterialInstance DefaultBoxMaterial => new(boxMaterial);
        public MaterialInstance TextSDFMaterial => new(textSdfMaterial);

        public FontLibrary FontLibrary = null!;

        private UIContext? HoveredContext;
        private UIContext? FocusedContext;
        private UIContext? CapturedContext;

        private Material boxMaterial = null!;
        private Material textSdfMaterial = null!;

        public void Initialize()
        {
            Engine.InputSystem.Router.Push(new UIInputLayer());

            Shader boxShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/ui_mat.dsd");
            boxMaterial = new Material(boxShader);

            Shader textSdfShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/ui_textsdf_mat.dsd");
            textSdfMaterial = new Material(textSdfShader);

            DefaultTheme = UIThemeDefaults.InitializeDefaultTheme();

            FontLibrary = new FontLibrary();
        }

        public void Update(float deltaTime, List<Viewport> viewports)
        {
            foreach (var viewport in viewports)
            {
                UIContext context = viewport.UIContext;

                foreach (var canvas in context.Canvases)
                {
                    if (canvas.RenderMode == CanvasRenderMode.ScreenSpace)
                    {
                        Vector2 scaledScreen = new Vector2(viewport.Width, viewport.Height) / context.UIScale;

                        canvas.Measure(scaledScreen);
                        canvas.Arrange(new Rect(viewport.Bounds.Position, scaledScreen));
                    }

                    canvas.Update(deltaTime);
                }
            }
        }


        public void MouseMove(Vector2 mouse)
        {
            List<Viewport> viewports = Engine.Instance.SceneTree.GetViewports();

            if (CapturedContext != null)
            {
                ProcessMouseMove(CapturedContext, CapturedContext.Viewport, mouse);
                return;
            }

            for (int i = viewports.Count - 1; i >= 0; i--)
            {
                Viewport viewport = viewports[i];

                if (!viewport.Bounds.Contains(mouse))
                    continue;

                ProcessMouseMove(viewport.UIContext, viewport, mouse);
                return;
            }

            foreach (Viewport viewport in viewports)
            {
                UIContext context = viewport.UIContext;

                if (context.Hovered != null)
                {
                    context.Hovered.OnMouseLeave();
                    context.Hovered = null;
                }
            }
        }

        private void ProcessMouseMove(UIContext context, Viewport viewport, Vector2 globalMouse)
        {
            // Convert into viewport-local coordinates.
            Vector2 mouse = globalMouse - viewport.Bounds.Position;

            Vector2 mouseDelta = mouse - context.PrevMousePosition;
            context.PrevMousePosition = mouse;

            UINode? previousHovered = context.Hovered;
            context.Hovered = null;

            for (int i = context.Canvases.Count - 1; i >= 0; i--)
            {
                CanvasNode canvas = context.Canvases[i];

                if (!canvas.Visible)
                    continue;

                Vector2 position = mouse;

                switch (canvas.RenderMode)
                {
                    case CanvasRenderMode.ScreenSpace:
                        position = mouse;
                        break;
                }

                context.Hovered = HitTest(canvas, position);

                if (context.Hovered != null)
                    break;
            }

            if (previousHovered != context.Hovered)
            {
                previousHovered?.OnMouseLeave();
                context.Hovered?.OnMouseEnter();
                HoveredContext = context;
            }

            // Dragging
            if (context.Pressed != null)
            {
                if (!context.IsDragging)
                {
                    if (Vector2.Distance(mouse, context.DragStartMouse) > 3f)
                    {
                        context.IsDragging = true;
                        context.Pressed.OnDragStart(mouse);
                    }
                    else
                    {
                        context.Pressed.OnMouseHeld();
                    }
                }

                if (context.IsDragging)
                {
                    context.Pressed.OnDrag(mouse, mouseDelta);
                }
            }

            context.MousePosition = mouse;
        }

        public void MouseDown(Vector2 globalMouse)
        {
            UIContext? context = GetTargetContext(globalMouse);

            if (context == null)
                return;

            context.Pressed = context.Hovered;
            context.DragStartMouse = context.MousePosition;
            context.IsDragging = false;

            if (context.Pressed != null)
            {
                CapturedContext = context;

                SetFocus(context, context.Pressed);

                context.Pressed.OnMouseDown();
            }
            else
            {
                ClearFocus();
            }
        }

        public void MouseUp(Vector2 globalMouse)
        {
            UIContext? context = CapturedContext ?? GetTargetContext(globalMouse);

            if (context == null || context.Pressed == null)
                return;

            if (context.IsDragging)
            {
                context.Pressed.OnDragEnd(context.MousePosition);
            }

            context.Pressed.OnMouseUp();

            if (!context.IsDragging &&
                context.Hovered == context.Pressed)
            {
                context.Pressed.OnClick();
            }

            context.Pressed = null;
            context.IsDragging = false;

            CapturedContext = null;
        }

        public void MouseScroll(Vector2 scroll)
        {
            UIContext? context = CapturedContext ?? HoveredContext;

            context?.Hovered?.OnMouseScroll(scroll);
        }

        public void KeyDown(Keys key)
        {
            FocusedContext?.Focused?.OnKeyDown(key);
        }

        public void KeyUp(Keys key)
        {
            FocusedContext?.Focused?.OnKeyUp(key);
        }

        public void KeyPressed(Keys key)
        {
            FocusedContext?.Focused?.OnKeyPressed(key);
        }

        public void TextInput(char c)
        {
            FocusedContext?.Focused?.OnTextInput(c);
        }

        private void SetFocus(UIContext context, UINode node)
        {
            if (FocusedContext == context &&
                context.Focused == node)
                return;

            // Blur the previous focused node (even if it's in another viewport)
            if (FocusedContext != null)
            {
                FocusedContext.Focused?.OnBlur();
                FocusedContext.Focused = null;
            }

            FocusedContext = context;
            context.Focused = node;

            node.OnFocus();
        }

        private void ClearFocus()
        {
            if (FocusedContext == null)
                return;

            FocusedContext.Focused?.OnBlur();
            FocusedContext.Focused = null;
            FocusedContext = null;
        }

        private UINode? HitTest(UINode node, Vector2 position)
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

            if (position.X >= rect.Position.X &&
                position.X <= rect.Position.X + rect.Size.X &&
                position.Y >= rect.Position.Y &&
                position.Y <= rect.Position.Y + rect.Size.Y)
            {
                return node;
            }

            return null;
        }

        private UIContext? GetTargetContext(Vector2 globalMouse)
        {
            if (CapturedContext != null)
                return CapturedContext;

            List<Viewport> viewports = Engine.Instance.SceneTree.GetViewports();

            for (int i = viewports.Count - 1; i >= 0; i--)
            {
                Viewport vp = viewports[i];

                if (vp.Bounds.Contains(globalMouse))
                    return vp.UIContext;
            }

            return null;
        }
    }
}
