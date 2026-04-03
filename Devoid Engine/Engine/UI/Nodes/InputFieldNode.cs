using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class InputFieldNode : ContainerNode
    {
        public override string ThemeType => "InputField";

        public string Text = "";
        public int CaretIndex = 0;
        public string HintText = "Type here..";

        public Action<string>? OnSubmit;

        internal LabelNode label;
        internal BoxNode caret;
        internal LabelNode hintLabel;

        private FontInternal? font;
        private int? fontSize;

        internal float caretTimer;
        internal bool caretVisible = true;
        internal float caretHeight = 5;

        const float BlinkTime = 0.8f;

        float backspaceHoldTimer = 0f;
        float backspaceRepeatTimer = 0f;
        bool backspaceHeld = false;

        const float BackspaceDelay = 0.45f;
        const float BackspaceRepeatRate = 0.035f;

        public InputFieldNode()
        {
            BlockInput = true;

            Direction = FlexDirection.Row;
            Align = AlignItems.Center;

            Padding = Padding.GetAll(6);

            font = GetFont(StyleKeys.Font);
            fontSize = GetFontSize(StyleKeys.FontSize);


            label = new LabelNode("", font!, (float)fontSize!);
            label.Layout.FlexGrowMain = 0;
            label.Layout.FlexGrowCross = 0;

            hintLabel = new LabelNode("", font!, (float)fontSize!);
            hintLabel.Layout.FlexGrowMain = 0;
            hintLabel.Layout.FlexGrowCross = 0;
            hintLabel.ParticipatesInLayout = false;   // <-- important
            hintLabel.AddColorOverride(StyleKeys.FontColor, new Vector4(1, 1, 1, 0.35f));

            caret = new BoxNode()
            {
                Size = new Vector2(2, caretHeight),
                Color = new Vector4(1, 1, 1, 1),
                ParticipatesInLayout = false
            };
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();


            Add(hintLabel);
            Add(label);
            Add(caret);

            UpdateText();
        }
        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            return base.MeasureCore(availableSize);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            float innerWidth = finalRect.size.X - Padding.Left - Padding.Right;

            label.LayoutOptions.MaxWidth = innerWidth;
            hintLabel.LayoutOptions.MaxWidth = innerWidth;

            base.ArrangeCore(finalRect);

            if (label.Rect != null)
            {
                hintLabel.Arrange(new UITransform(label.Rect.position, label.Rect.size));
            }
        }

        protected override void ApplyTheme()
        {
            font = GetFont(StyleKeys.Font);
            fontSize = GetFontSize(StyleKeys.FontSize);

            if (font == null || fontSize == null)
                throw new Exception("Input field font was not set");

            caretHeight = font!.LineHeight * font.GetScaleForFontSize(fontSize ?? 16);

            if (caret != null)
                caret.Size = new Vector2(2, caretHeight);

            base.ApplyTheme();
        }

        internal void UpdateText()
        {
            label.Text = Text;
            hintLabel.Text = HintText;

            hintLabel.Visible = string.IsNullOrEmpty(Text);
        }

        protected override void UpdateCore(float dt)
        {
            caretTimer += dt;

            if (caretTimer > BlinkTime)
            {
                caretVisible = !caretVisible;
                caretTimer = 0;
            }

            caret.Visible = caretVisible && UISystem.FocusedNode == this && !string.IsNullOrEmpty(Text);

            UpdateCaretPosition();

            HandleBackspaceRepeat(dt);
        }

        void UpdateCaretPosition()
        {
            if (label.Rect == null)
                return;

            CaretIndex = Math.Clamp(CaretIndex, 0, Text.Length);

            float maxWidth = label.Rect.size.X;

            float wrapWidth = Rect.size.X - Padding.Left - Padding.Right;
            Vector2 pos = label.GetCursorPosition(CaretIndex, wrapWidth);

            caret.Arrange(new UITransform(
                new Vector2(
                    label.Rect.position.X + pos.X,
                    label.Rect.position.Y + pos.Y
                ),
                caret.Size ?? new Vector2(2, caretHeight)
            ));
        }

        public override void OnMouseDown()
        {
            caretVisible = true;
            caretTimer = 0;
        }

        public override void OnTextInput(char c)
        {
            
            Text = Text.Insert(CaretIndex, c.ToString());
            CaretIndex++;

            caretVisible = true;
            caretTimer = 0;

            UpdateText();
        }
        public override void OnKeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Enter:
                    OnSubmit?.Invoke(Text);
                    break;

                case Keys.Backspace:

                    if (CaretIndex > 0)
                    {
                        Text = Text.Remove(CaretIndex - 1, 1);
                        CaretIndex--;
                        UpdateText();
                    }

                    backspaceHeld = true;
                    backspaceHoldTimer = 0;
                    backspaceRepeatTimer = 0;

                    break;

                case Keys.Delete:
                    if (CaretIndex < Text.Length)
                        Text = Text.Remove(CaretIndex, 1);
                    break;

                case Keys.Left:
                    CaretIndex--;
                    break;

                case Keys.Right:
                    CaretIndex++;
                    break;

                case Keys.Home:
                    CaretIndex = 0;
                    break;

                case Keys.End:
                    CaretIndex = Text.Length;
                    break;
            }

            CaretIndex = Math.Clamp(CaretIndex, 0, Text.Length);

            UpdateText();
        }

        public override void OnKeyUp(Keys key)
        {
            if (key == Keys.Backspace)
                backspaceHeld = false;
            base.OnKeyUp(key);
        }

        public override void OnClick()
        {
            if (label.Rect == null)
                return;

            Vector2 mouse = UISystem.mousePosition;

            float wrapWidth = Rect.size.X - Padding.Left - Padding.Right;

            int bestIndex = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i <= Text.Length; i++)
            {
                Vector2 cursor = label.GetCursorPosition(i, wrapWidth);

                Vector2 worldPos = new Vector2(
                    label.Rect.position.X + cursor.X,
                    label.Rect.position.Y + cursor.Y
                );

                float dist = Vector2.Distance(mouse, worldPos);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }

            CaretIndex = bestIndex;

            caretVisible = true;
            caretTimer = 0;
        }

        void HandleBackspaceRepeat(float dt)
        {
            if (!backspaceHeld)
                return;

            backspaceHoldTimer += dt;

            if (backspaceHoldTimer < BackspaceDelay)
                return;

            backspaceRepeatTimer += dt;

            if (backspaceRepeatTimer >= BackspaceRepeatRate)
            {
                backspaceRepeatTimer = 0;

                if (CaretIndex > 0)
                {
                    Text = Text.Remove(CaretIndex - 1, 1);
                    CaretIndex--;

                    UpdateText();
                }
            }
        }

        public override void OnFocus()
        {
            caretVisible = true;
            caretTimer = 0;
        }

        public override void OnBlur()
        {
            caretVisible = false;
        }
    }
}