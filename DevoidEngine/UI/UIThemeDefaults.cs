using DevoidEngine.UI.Theme;
using DevoidEngine.UI.Theme.Styleboxes;
using System.Numerics;

namespace DevoidEngine.UI
{
    public static class UIThemeDefaults
    {
        public static UITheme InitializeDefaultTheme()
        {
            //var font = FontLibrary.LoadFont(
            //    "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
            //    32
            //);

            var fontSize = 16;

            //var iconFont = FontLibrary.LoadFont(
            //    "Engine/Content/Fonts/bootstrap_icons.ttf",
            //    32
            //);

            var theme = new UITheme();

            // Fonts
            //theme.SetFont(StyleKeys.Font, "DropdownHeader", font);
            //theme.SetFont(StyleKeys.Font, "InputField", font);
            //theme.SetFont(StyleKeys.Font, "Button", font);
            //theme.SetFont(StyleKeys.Font, "Panel", font);

            theme.SetFontSize(StyleKeys.FontSize, "InputField", fontSize);
            //theme.SetFont(StyleKeys.Font, "Icon", iconFont);

            // Label
            theme.SetColor(
                StyleKeys.FontColor,
                "Label",
                new Vector4(1, 1, 1, 1)
            );

            theme.SetFontSize(
                StyleKeys.FontSize,
                "Label",
                16
            );

            // PANEL
            theme.SetStyleBox(
                StyleKeys.Normal,
                "Panel",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(1, 1, 1, 0.4f),
                    BorderWidth = 0,
                    BorderColor = new Vector4(1, 1, 1, 1),
                    BorderRadius = Vector4.Zero
                }
            );

            theme.SetStyleBox(
                StyleKeys.Normal,
                "InputField",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(1, 1, 1, 0.4f),
                    BorderWidth = 0,
                    BorderColor = new Vector4(1, 1, 1, 1),
                    BorderRadius = Vector4.Zero
                }
            );

            theme.SetColor(
                StyleKeys.FontColor,
                "Panel",
                new Vector4(0, 0, 1, 1)
            );

            // SLIDER TRACK
            theme.SetStyleBox(
                StyleKeys.Normal,
                "SliderTrack",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 1),
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = new Vector4(20)
                }
            );

            // SLIDER THUMB
            theme.SetStyleBox(
                StyleKeys.Normal,
                "SliderThumb",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(1, 1, 1, 0.5f),
                    BorderWidth = 3,
                    BorderColor = new Vector4(0, 0, 0, 0.4f),
                    BorderRadius = new Vector4(20)
                }
            );

            // CHECKBOX OUTER
            theme.SetStyleBox(
                StyleKeys.Normal,
                "CheckboxOuter",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 1),
                    BorderWidth = 1,
                    BorderColor = new Vector4(1, 1, 1, 0.15f),
                    BorderRadius = new Vector4(20)
                }
            );

            // CHECKBOX INNER
            theme.SetStyleBox(
                StyleKeys.Normal,
                "CheckboxInner",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(1, 1, 1, 1),
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = new Vector4(20)
                }
            );

            // DROPDOWN HEADER
            theme.SetStyleBox(
                StyleKeys.Normal,
                "DropdownHeader",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 1),
                    BorderWidth = 1,
                    BorderColor = new Vector4(1, 1, 1, 0.1f),
                    BorderRadius = new Vector4(4)
                }
            );

            // DROPDOWN ITEM
            theme.SetStyleBox(
                StyleKeys.Normal,
                "DropdownItem",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 0.5f),
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = Vector4.Zero
                }
            );

            // BUTTON NORMAL
            theme.SetStyleBox(
                StyleKeys.Normal,
                "Button",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1f),
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.4f),
                    BorderRadius = new Vector4(6)
                }
            );

            // BUTTON HOVER
            theme.SetStyleBox(
                StyleKeys.Hover,
                "Button",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.4f, 0.4f, 0.4f, 1f),
                    BorderWidth = 1,
                    BorderColor = new Vector4(1, 1, 1, 0.2f),
                    BorderRadius = new Vector4(6)
                }
            );

            // BUTTON PRESSED
            theme.SetStyleBox(
                StyleKeys.Pressed,
                "Button",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.35f, 0.35f, 0.35f, 1f),
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.6f),
                    BorderRadius = new Vector4(6)
                }
            );

            // SCROLLBAR TRACK
            theme.SetStyleBox(
                StyleKeys.Normal,
                "Scrollbar",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 1),
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = new Vector4(4)
                }
            );

            // SCROLLBAR THUMB NORMAL
            theme.SetStyleBox(
                StyleKeys.Normal,
                "ScrollbarThumb",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.6f, 0.6f, 0.6f, 0.6f),
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.4f),
                    BorderRadius = new Vector4(4)
                }
            );

            // SCROLLBAR THUMB HOVER
            theme.SetStyleBox(
                StyleKeys.Hover,
                "ScrollbarThumb",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.8f, 0.8f, 0.8f, 0.8f),
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.4f),
                    BorderRadius = new Vector4(4)
                }
            );

            // SCROLLBAR THUMB PRESSED
            theme.SetStyleBox(
                StyleKeys.Pressed,
                "ScrollbarThumb",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(1f, 1f, 1f, 0.9f),
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.6f),
                    BorderRadius = new Vector4(4)
                }
            );

            //theme.SetStyleBox(
            //    StyleKeys.Normal,
            //    "DragField",
            //    new StyleBoxFlat()
            //    {
            //        BackgroundColor = new Vector4(0.35f, 0.35f, 0.35f, 1),
            //        BorderWidth = 1,
            //        BorderColor = new Vector4(0, 0, 0, 0.6f),
            //        BorderRadius = new Vector4(4)
            //    }
            //);

            theme.SetStyleBox(
                StyleKeys.Editing,
                "DragField",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 1),
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.6f),
                    BorderRadius = new Vector4(4)
                }
            );

            theme.SetStyleBox(
                StyleKeys.Normal,
                "ScrollContainer",
                new StyleBoxFlat()
                {
                    BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 0.6f),
                    //BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.6f),
                    BorderRadius = new Vector4(4)
                }
            );

            // Type inheritance
            theme.SetTypeVariation("DropdownItem", "Button");
            theme.SetTypeVariation("DropdownHeader", "Panel");
            theme.SetTypeVariation("Dropdown", "Panel");
            theme.SetTypeVariation("Label", "Panel");
            theme.SetTypeVariation("DragField", "InputField");
            //theme.SetTypeVariation("InputField", "Button");
            theme.SetTypeVariation("InputField", "Panel");
            theme.SetTypeVariation("ScrollContentBox", "ScrollContainer");

            return theme;
        }

    }
}
