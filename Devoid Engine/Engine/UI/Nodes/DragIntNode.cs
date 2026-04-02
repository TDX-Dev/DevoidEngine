using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.UI.Theme;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class DragIntNode : InputFieldNode
    {
        public override string ThemeType => "DragField";

        public int Value
        {
            get => value;
            set
            {
                Text = value.ToString();
                CaretIndex = Text.Length;
                UpdateText();
                this.value = value;
            }
        }

        public float Sensitivity = 0.1f;

        int value;
        bool editing = false;
        bool dragging = false;
        bool mouseOver = false;

        public DragIntNode()
        {
            Console.WriteLine(GetFont(StyleKeys.Font));
        }

        protected override void InitializeCore()
        {
            BlockInput = true;

            base.OnSubmit = (string submitVal) =>
            {
                if (int.TryParse(submitVal, out int result))
                {
                    value = result;
                } else
                {
                    Text = value.ToString();
                }
            };

            HintText = "";

            base.InitializeCore();
        }

        public override void OnClick()
        {
            editing = true;
            UpdateEditingState();
            CaretIndex = Text.Length;
            caretVisible = true;
            caretTimer = 0;
            Cursor.SetCursorShape(CursorShape.Arrow);
        }

        protected override void UpdateCore(float dt)
        {
            timer += dt;

            if (!editing)
            {
                caret.Visible = false;
                return;
            }

            base.UpdateCore(dt);
        }

        float timer = 0;

        float accumulated;

        public override void OnDragStart(Vector2 mouse)
        {
            accumulated = 0;
            dragging = true;
        }

        public override void OnDrag(Vector2 mouse, Vector2 delta)
        {
            if (editing)
                return;

            accumulated += delta.X * Sensitivity;

            int step = (int)accumulated;

            if (step != 0)
            {
                Value += step;
                accumulated -= step;
            }
        }

        public override void OnDragEnd(Vector2 mouse)
        {
            dragging = false;
            if (!mouseOver)
                Cursor.SetCursorShape(CursorShape.Arrow);
        }

        public override void OnMouseEnter()
        {
            mouseOver = true;
            if (!editing)
                Cursor.SetCursorShape(CursorShape.ResizeEW);
        }

        public override void OnMouseLeave()
        {
            mouseOver = false;
            if (!editing && !dragging)
                Cursor.SetCursorShape(CursorShape.Arrow);
        }

        public override void OnMouseDown()
        {
            if (!editing)
                return;

            base.OnMouseDown();
        }

        public override void OnTextInput(char c)
        {
            if (!editing)
                return;

            base.OnTextInput(c);
        }

        public override void OnKeyDown(Keys key)
        {
            if (!editing)
                return;

            if (key == Keys.Enter || key == Keys.Escape)
            {
                editing = false;
                UpdateEditingState();
                base.OnKeyDown(key);
                if (mouseOver)
                    Cursor.SetCursorShape(CursorShape.ResizeEW);
                return;
            }

            base.OnKeyDown(key);
        }

        void UpdateEditingState()
        {
            if (editing)
                State |= UIState.Editing;
            else
                State &= ~UIState.Editing;

            ApplyTheme();
        }
    }
}