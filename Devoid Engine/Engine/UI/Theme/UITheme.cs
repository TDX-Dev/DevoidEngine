using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Theme
{
    public class UITheme
    {
        private Dictionary<string, ThemeTypeData> types = new();
        private Dictionary<string, string> typeVariations = new();

        private ThemeTypeData GetOrCreateType(string type)
        {
            if (!types.TryGetValue(type, out var data))
            {
                data = new ThemeTypeData();
                types[type] = data;
            }

            return data;
        }

        public void AddType(string themeType)
        {
            GetOrCreateType(themeType);
        }

        public void SetColor(string name, string themeType, Vector4 color)
        {
            var data = GetOrCreateType(themeType);
            data.Colors[name] = color;
        }

        public void SetConstant(string name, string themeType, int constant)
        {
            var data = GetOrCreateType(themeType);
            data.Constants[name] = constant;
        }

        public void SetFont(string name, string themeType, FontInternal font)
        {
            var data = GetOrCreateType(themeType);
            data.Fonts[name] = font;
        }

        public void SetStyleBox(string name, string themeType, StyleBox stylebox)
        {
            var data = GetOrCreateType(themeType);
            data.StyleBoxes[name] = stylebox;
        }

        public StyleBox GetStyleBox(string name, string themeType)
        {
            if (types.TryGetValue(themeType, out var data) &&
                data.StyleBoxes.TryGetValue(name, out var style))
            {
                return style;
            }

            return null;
        }

        public Vector4 GetColor(string name, string themeType)
        {
            if (types.TryGetValue(themeType, out var data) &&
                data.Colors.TryGetValue(name, out var color))
            {
                return color;
            }

            return Vector4.One;
        }

        public int GetConstant(string name, string themeType)
        {
            if (types.TryGetValue(themeType, out var data) &&
                data.Constants.TryGetValue(name, out var value))
            {
                return value;
            }

            return 0;
        }

        public void ClearColor(string name, string themeType)
        {
            if (types.TryGetValue(themeType, out var data))
                data.Colors.Remove(name);
        }

        public void RenameColor(string oldName, string name, string themeType)
        {
            if (!types.TryGetValue(themeType, out var data))
                return;

            if (!data.Colors.TryGetValue(oldName, out var value))
                return;

            data.Colors.Remove(oldName);
            data.Colors[name] = value;
        }

        public bool HasColor(string name, string themeType)
        {
            return types.TryGetValue(themeType, out var data)
                && data.Colors.ContainsKey(name);
        }

        public void SetTypeVariation(string themeType, string baseType)
        {
            typeVariations[themeType] = baseType;
        }

        public bool IsTypeVariation(string themeType, string baseType)
        {
            return typeVariations.TryGetValue(themeType, out var baseT)
                && baseT == baseType;
        }

        public void MergeWith(UITheme other)
        {
            foreach (var type in other.types)
            {
                var dst = GetOrCreateType(type.Key);
                var src = type.Value;

                foreach (var kv in src.Colors)
                    dst.Colors[kv.Key] = kv.Value;

                foreach (var kv in src.Constants)
                    dst.Constants[kv.Key] = kv.Value;

                foreach (var kv in src.Fonts)
                    dst.Fonts[kv.Key] = kv.Value;
            }
        }
    }
}
