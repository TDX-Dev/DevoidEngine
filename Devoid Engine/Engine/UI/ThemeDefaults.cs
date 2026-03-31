using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
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

            var DefaultTheme = new UITheme();

            DefaultTheme.SetFont(
                StyleKeys.Font,
                "DropdownHeader",
                font
            );

            DefaultTheme.SetFont(
                StyleKeys.Font,
                "Button",
                font
            );

            DefaultTheme.SetFont(
                StyleKeys.Font,
                "Panel",
                font
            );

            // Label styling
            DefaultTheme.SetColor(
                StyleKeys.FontColor,
                "Label",
                new Vector4(1, 1, 1, 1)
            );

            DefaultTheme.SetFontSize(
                StyleKeys.FontSize,
                "Label",
                16
            );

            // Panel styling
            DefaultTheme.SetConstant(
                StyleKeys.BorderWidth,
                "Panel",
                0
            );


            DefaultTheme.SetColor(
                StyleKeys.BorderColor,
                "Panel",
                new Vector4(1, 1, 1, 1)
            );

            DefaultTheme.SetColor(
                StyleKeys.FontColor,
                "Panel",
                new Vector4(0, 0, 1, 1)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "Panel",
                new Vector4(1, 1, 1, 0.4f)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "SliderTrack",
                new Vector4(0.1f, 0.1f, 0.1f, 1)
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

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "SliderThumb",
                new Vector4(1, 1, 1, 0.5f)
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

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "CheckboxInner",
                new Vector4(1, 1, 1, 1)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "CheckboxOuter",
                new Vector4(0.1f, 0.1f, 0.1f, 1)
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

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownHeader",
                new Vector4(0.1f, 0.1f, 0.1f, 1)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownItem",
                new Vector4(0.1f, 0.1f, 0.1f, 1)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownItem",
                new Vector4(0.1f, 0.1f, 0.1f, 0.5f)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "Button",
                new Vector4(0.25f, 0.25f, 0.25f, 1f)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "ButtonHover",
                new Vector4(0.4f, 0.4f, 0.4f, 1f)
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "ButtonPressed",
                new Vector4(0.35f, 0.35f, 0.35f, 1f)
            );

            DefaultTheme.SetTypeVariation("DropdownItem", "Panel");
            DefaultTheme.SetTypeVariation("DropdownHeader", "Panel");
            DefaultTheme.SetTypeVariation("Dropdown", "Panel");
            DefaultTheme.SetTypeVariation("Label", "Panel");

            return DefaultTheme;
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

            var DefaultTheme = new UITheme();

            Vector4 bg = new Vector4(30f / 255f, 30f / 255f, 30f / 255f, 1f);
            Vector4 panel = new Vector4(45f / 255f, 45f / 255f, 48f / 255f, 1f);
            Vector4 accent = new Vector4(0f / 255f, 122f / 255f, 204f / 255f, 1f);
            Vector4 accentHover = new Vector4(28f / 255f, 151f / 255f, 234f / 255f, 1f);
            Vector4 text = new Vector4(220f / 255f, 220f / 255f, 220f / 255f, 1f);
            Vector4 button = new Vector4(60f / 255f, 60f / 255f, 60f / 255f, 1f);
            Vector4 buttonHover = new Vector4(0f / 255f, 122f / 255f, 204f / 255f, 1f);
            Vector4 buttonPressed = new Vector4(28f / 255f, 151f / 255f, 234f / 255f, 1f);


            DefaultTheme.SetFont(StyleKeys.Font, "DropdownHeader", font);
            DefaultTheme.SetFont(StyleKeys.Font, "Button", font);
            DefaultTheme.SetFont(StyleKeys.Font, "Panel", font);

            // Label
            DefaultTheme.SetColor(
                StyleKeys.FontColor,
                "Label",
                text
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
                panel
            );

            DefaultTheme.SetColor(
                StyleKeys.FontColor,
                "Panel",
                text
            );

            // Slider Track
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "SliderTrack",
                bg
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderWidth,
                "SliderTrack",
                0
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "SliderTrack",
                new Vector4(4)
            );

            // Slider Thumb
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "SliderThumb",
                accent
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderWidth,
                "SliderThumb",
                2
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "SliderThumb",
                new Vector4(4)
            );

            // Checkbox
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "CheckboxOuter",
                bg
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "CheckboxInner",
                accent
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "CheckboxOuter",
                new Vector4(3)
            );

            DefaultTheme.SetConstant(
                StyleKeys.BorderRadius,
                "CheckboxInner",
                new Vector4(3)
            );

            // Dropdown
            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownHeader",
                button
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "DropdownItem",
                bg
            );

            // Buttons

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "Button",
                button
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "ButtonHover",
                buttonHover
            );

            DefaultTheme.SetColor(
                StyleKeys.Background,
                "ButtonPressed",
                buttonPressed
            );

            DefaultTheme.SetTypeVariation("DropdownItem", "Panel");
            DefaultTheme.SetTypeVariation("DropdownHeader", "Panel");
            DefaultTheme.SetTypeVariation("Dropdown", "Panel");
            DefaultTheme.SetTypeVariation("Label", "Panel");

            return DefaultTheme;
        }

    }
}
