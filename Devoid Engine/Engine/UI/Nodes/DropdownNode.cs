using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    class DropdownItem : ButtonNode
    {
        public override string ThemeType => "DropdownItem";
    }

    class DropdownHeader : ContainerNode
    {
        public override string ThemeType => "DropdownHeader";
    }

    public class DropdownNode : FlexboxNode
    {
        public override string ThemeType => "Dropdown";

        public List<string> Items = new();
        public int SelectedIndex = -1;

        public Action<int> OnSelectionChanged;

        ContainerNode header;
        LabelNode label;

        ContainerNode popup;
        FlexboxNode options = new FlexboxNode();

        bool open = false;

        public DropdownNode()
        {
            BlockInput = true;

            Direction = FlexDirection.Row;

            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 1;

            MinSize = new Vector2(120, 30);
            MaxSize = new Vector2(120, 30);

            label = new LabelNode("Dropdown", GetFont(StyleKeys.Font), 16);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // HEADER
            header = new DropdownHeader();
            header.Layout.FlexGrowMain = 1;
            header.Justify = JustifyContent.Center;
            header.Padding = Padding.GetAll(5);

            header.Add(label);

            // POPUP
            popup = new ContainerNode();
            popup.Visible = false;
            popup.ParticipatesInLayout = false;
            popup.Align = AlignItems.Stretch;

            options.Direction = FlexDirection.Column;
            options.Layout.FlexGrowMain = 1;

            popup.Add(options);

            Add(header);
            Add(popup);
        }

        public void SetItems(List<string> items)
        {
            Items = items;
            options.Clear();

            for (int i = 0; i < Items.Count; i++)
            {
                int index = i;

                DropdownItem optionContainer = new DropdownItem()
                {
                    Padding = Padding.GetAll(5),
                    Align = AlignItems.Start,
                    Justify = JustifyContent.Start,
                    Layout = new LayoutOptions()
                    {
                        FlexGrowMain = 1,
                        FlexGrowCross = 1,
                    }
                };

                
                optionContainer.Text = Items[i];

                optionContainer.BlockInput = true;

                optionContainer.OnPressed = () =>
                {
                    Select(index);
                    Close();
                };

                options.Add(optionContainer);
            }

            if (Items.Count > 0)
                Select(0);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            base.ArrangeCore(finalRect);

            if (!popup.Visible)
                return;

            if (header == null)
                return;

            float popupWidth = Rect.size.X;

            // let options measure itself
            options.Measure(new Vector2(popupWidth, float.PositiveInfinity));

            float popupHeight = options.DesiredSize.Y;

            Vector2 popupPos = new(
                header.Rect.position.X,
                header.Rect.position.Y + header.Rect.size.Y
            );

            popup.Arrange(new UITransform(
                popupPos,
                new Vector2(popupWidth, popupHeight)
            ));
        }

        void Select(int index)
        {
            SelectedIndex = index;
            label.Text = Items[index];

            OnSelectionChanged?.Invoke(index);
        }

        void Toggle()
        {
            open = !open;
            popup.Visible = open;
        }

        void Close()
        {
            open = false;
            popup.Visible = false;
        }

        public override void OnClick()
        {
            Toggle();
            Console.WriteLine("Toggle Dropdown");
        }
    }
}
