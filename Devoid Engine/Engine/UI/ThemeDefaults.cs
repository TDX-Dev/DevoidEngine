using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI
{
    static class ThemeDefaults
    {
        public static UITheme InitializeDefaultTheme()
        {
            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            //var iconFont = FontLibrary.LoadFont(
            //    "Engine/Content/Fonts/bootstrap_icons.ttf",
            //    32
            //);

            var theme = new UITheme();

            // Fonts
            theme.SetFont(StyleKeys.Font, "DropdownHeader", font);
            theme.SetFont(StyleKeys.Font, "Button", font);
            theme.SetFont(StyleKeys.Font, "Panel", font);
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

            // Type inheritance
            theme.SetTypeVariation("DropdownItem", "Button");
            theme.SetTypeVariation("DropdownHeader", "Panel");
            theme.SetTypeVariation("Dropdown", "Panel");
            theme.SetTypeVariation("Label", "Panel");

            return theme;
        }
        public static UITheme InitializeRetroTheme()
        {
            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            var DefaultTheme = new UITheme();

            Vector4 bg = new Vector4(242f / 255f, 96f / 255f, 118f / 255f, 1f);
            Vector4 accent1 = new Vector4(255f / 255f, 151f / 255f, 96f / 255f, 1f);
            Vector4 accent2 = new Vector4(255f / 255f, 209f / 255f, 80f / 255f, 1f);
            Vector4 fg = new Vector4(69f / 255f, 139f / 255f, 115f / 255f, 1f);

            DefaultTheme.SetFont(StyleKeys.Font, "DropdownHeader", font);
            DefaultTheme.SetFont(StyleKeys.Font, "Button", font);
            DefaultTheme.SetFont(StyleKeys.Font, "Panel", font);

            // Label
            DefaultTheme.SetColor(
                StyleKeys.FontColor,
                "Label",
                fg
            );

            DefaultTheme.SetFontSize(
                StyleKeys.FontSize,
                "Label",
                16
            );

            // Panel
            DefaultTheme.SetConstant(
                StyleKeys.BorderWidth,
                "Panel",
                0
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "Panel",
                bg
            );

            DefaultTheme.SetColor(
                StyleKeys.FontColor,
                "Panel",
                fg
            );

            // Slider Track
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "SliderTrack",
                accent1
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderWidth,
                "SliderTrack",
                0
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "SliderTrack",
                new Vector4(20)
            );

            // Slider Thumb
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "SliderThumb",
                accent2
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderWidth,
                "SliderThumb",
                3
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "SliderThumb",
                new Vector4(20)
            );

            // Checkbox
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "CheckboxInner",
                accent2
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "CheckboxOuter",
                accent1
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "CheckboxInner",
                new Vector4(20)
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "CheckboxOuter",
                new Vector4(20)
            );

            // Dropdown
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownHeader",
                accent1
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownItem",
                accent2
            );

            // Button
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "Button",
                accent1
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "ButtonHover",
                accent2
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "ButtonPressed",
                fg
            );

            DefaultTheme.SetTypeVariation("DropdownItem", "Panel");
            DefaultTheme.SetTypeVariation("DropdownHeader", "Panel");
            DefaultTheme.SetTypeVariation("Dropdown", "Panel");
            DefaultTheme.SetTypeVariation("Label", "Panel");

            return DefaultTheme;
        }

        public static UITheme InitializeVisualStudioTheme()
        {
            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            var theme = new UITheme();

            Vector4 bg = new Vector4(30f / 255f, 30f / 255f, 30f / 255f, 1f);
            Vector4 panel = new Vector4(45f / 255f, 45f / 255f, 48f / 255f, 1f);
            Vector4 accent = new Vector4(0f / 255f, 122f / 255f, 204f / 255f, 1f);
            Vector4 accentHover = new Vector4(28f / 255f, 151f / 255f, 234f / 255f, 1f);
            Vector4 text = new Vector4(220f / 255f, 220f / 255f, 220f / 255f, 1f);
            Vector4 button = new Vector4(60f / 255f, 60f / 255f, 60f / 255f, 1f);
            Vector4 buttonHover = new Vector4(0f / 255f, 122f / 255f, 204f / 255f, 1f);
            Vector4 buttonPressed = new Vector4(28f / 255f, 151f / 255f, 234f / 255f, 1f);

            // Fonts
            theme.SetFont(StyleKeys.Font, "DropdownHeader", font);
            theme.SetFont(StyleKeys.Font, "Button", font);
            theme.SetFont(StyleKeys.Font, "Panel", font);

            // Label
            theme.SetColor(StyleKeys.FontColor, "Label", text);
            theme.SetFontSize(StyleKeys.FontSize, "Label", 16);

            // Panel
            theme.SetStyleBox(
                StyleKeys.Normal,
                "Panel",
                new StyleBoxFlat()
                {
                    BackgroundColor = panel,
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = Vector4.Zero
                }
            );

            // Slider Track
            theme.SetStyleBox(
                StyleKeys.Normal,
                "SliderTrack",
                new StyleBoxFlat()
                {
                    BackgroundColor = bg,
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = new Vector4(4)
                }
            );

            // Slider Thumb
            theme.SetStyleBox(
                StyleKeys.Normal,
                "SliderThumb",
                new StyleBoxFlat()
                {
                    BackgroundColor = accent,
                    BorderWidth = 2,
                    BorderColor = accentHover,
                    BorderRadius = new Vector4(4)
                }
            );

            // Checkbox Outer
            theme.SetStyleBox(
                StyleKeys.Normal,
                "CheckboxOuter",
                new StyleBoxFlat()
                {
                    BackgroundColor = bg,
                    BorderWidth = 1,
                    BorderColor = accent,
                    BorderRadius = new Vector4(3)
                }
            );

            // Checkbox Inner
            theme.SetStyleBox(
                StyleKeys.Normal,
                "CheckboxInner",
                new StyleBoxFlat()
                {
                    BackgroundColor = accent,
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = new Vector4(3)
                }
            );

            // Dropdown Header
            theme.SetStyleBox(
                StyleKeys.Normal,
                "DropdownHeader",
                new StyleBoxFlat()
                {
                    BackgroundColor = button,
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = new Vector4(3)
                }
            );

            // Dropdown Item
            theme.SetStyleBox(
                StyleKeys.Normal,
                "DropdownItem",
                new StyleBoxFlat()
                {
                    BackgroundColor = bg,
                    BorderWidth = 0,
                    BorderColor = Vector4.Zero,
                    BorderRadius = Vector4.Zero
                }
            );

            // Buttons
            theme.SetStyleBox(
                StyleKeys.Normal,
                "Button",
                new StyleBoxFlat()
                {
                    BackgroundColor = button,
                    BorderWidth = 1,
                    BorderColor = new Vector4(0, 0, 0, 0.4f),
                    BorderRadius = new Vector4(4)
                }
            );

            theme.SetStyleBox(
                StyleKeys.Hover,
                "Button",
                new StyleBoxFlat()
                {
                    BackgroundColor = buttonHover,
                    BorderWidth = 1,
                    BorderColor = accentHover,
                    BorderRadius = new Vector4(4)
                }
            );

            theme.SetStyleBox(
                StyleKeys.Pressed,
                "Button",
                new StyleBoxFlat()
                {
                    BackgroundColor = buttonPressed,
                    BorderWidth = 1,
                    BorderColor = accentHover,
                    BorderRadius = new Vector4(4)
                }
            );

            // SCROLLBAR TRACK
            theme.SetStyleBox(
                StyleKeys.Normal,
                "Scrollbar",
                new StyleBoxFlat()
                {
                    BackgroundColor = button,
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

            // Type inheritance
            theme.SetTypeVariation("DropdownItem", "Panel");
            theme.SetTypeVariation("DropdownHeader", "Panel");
            theme.SetTypeVariation("Dropdown", "Panel");
            theme.SetTypeVariation("Label", "Panel");

            return theme;
        }

    }
}
