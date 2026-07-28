using DevoidEngine.AssetPipeline;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.UI.Text;
using DevoidEngine.UI.Theme;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.UI.UINodes
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

        private Font? font;

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

            font = Asset.Load<Font>("valveoracle-semibold.ttf")!;

            label = new LabelNode()
            {
                Font = font,
                Text = " ",
                Overflow = TextOverflow.Wrap
            };
            label.Layout.FlexGrowMain = 0;
            label.Layout.FlexGrowCross = 0;

            hintLabel = new LabelNode()
            {
                Font = font,
                Text = " ",
                Overflow = TextOverflow.Wrap
            };
            hintLabel.Layout.FlexGrowMain = 0;
            hintLabel.Layout.FlexGrowCross = 0;
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

        protected override void ArrangeCore(Rect finalRect)
        {
            Rect = finalRect;

            base.ArrangeCore(finalRect);

            //hintLabel.Arrange(new Rect(label.Rect.Position, label.Rect.Size));
        }

        protected override void ApplyTheme()
        {
            font = label.Font;


            caretHeight = font!.LineHeight;

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

            //caret.Visible = caretVisible && UISystem.FocusedNode == this && !string.IsNullOrEmpty(Text);

            UpdateCaretPosition();

            HandleBackspaceRepeat(dt);
        }

        void UpdateCaretPosition()
        {

            //CaretIndex = Math.Clamp(CaretIndex, 0, Text.Length);

            //float maxWidth = label.Rect.Size.X;

            //float wrapWidth = Rect.Size.X - Padding.Left - Padding.Right;
            //Vector2 pos = label.GetCursorPosition(CaretIndex, wrapWidth);

            //caret.Arrange(new Rect(
            //    new Vector2(
            //        label.Rect.Position.X + pos.X,
            //        label.Rect.Position.Y + pos.Y
            //    ),
            //    caret.Size ?? new Vector2(2, caretHeight)
            //));
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

        //public override void OnClick()
        //{

        //    Vector2 mouse = UISystem.mousePosition;

        //    float wrapWidth = Rect.Size.X - Padding.Left - Padding.Right;

        //    int bestIndex = 0;
        //    float bestDist = float.MaxValue;

        //    for (int i = 0; i <= Text.Length; i++)
        //    {
        //        Vector2 cursor = label.GetCursorPosition(i, wrapWidth);

        //        Vector2 worldPos = new Vector2(
        //            label.Rect.Position.X + cursor.X,
        //            label.Rect.Position.Y + cursor.Y
        //        );

        //        float dist = Vector2.Distance(mouse, worldPos);

        //        if (dist < bestDist)
        //        {
        //            bestDist = dist;
        //            bestIndex = i;
        //        }
        //    }

        //    CaretIndex = bestIndex;

        //    caretVisible = true;
        //    caretTimer = 0;
        //}

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
