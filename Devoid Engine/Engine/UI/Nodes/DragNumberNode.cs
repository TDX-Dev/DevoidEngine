using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.UI.Theme;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.UI.Nodes
{
    public abstract class DragNumberNode<T> : InputFieldNode where T : struct
    {
        public override string ThemeType => "DragField";

        protected T value;

        public T Value
        {
            get => value;
            set
            {
                Text = Format(value);
                CaretIndex = Text.Length;
                UpdateText();

                this.value = value;

                OnValueChanged?.Invoke(value);
            }
        }

        public Action<T>? OnValueChanged;

        public float Sensitivity = 0.1f;

        bool editing = false;
        bool dragging = false;
        bool mouseOver = false;

        float accumulated;

        protected abstract bool TryParse(string text, out T result);
        protected abstract string Format(T value);
        protected abstract T Add(T value, float delta);
        protected abstract void ApplyDelta(float delta);

        public override void OnBlur()
        {
            editing = false;
            UpdateEditingState();
        }

        protected override void InitializeCore()
        {
            BlockInput = true;

            base.OnSubmit = (string submitVal) =>
            {
                if (TryParse(submitVal, out T result))
                    value = result;
                else
                    Text = Format(value);

                OnValueChanged?.Invoke(value);
            };

            HintText = "";

            Layout.FlexGrowMain = 1;

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

            float step = accumulated;

            if (MathF.Abs(step) >= 1)
            {
                Value = Add(Value, step);
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

        protected override void UpdateCore(float dt)
        {
            if (!editing)
            {
                caret.Visible = false;
                return;
            }

            base.UpdateCore(dt);
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