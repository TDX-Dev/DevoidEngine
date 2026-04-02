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
        public event Action ThemeChanged;

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


            ThemeChanged?.Invoke();
        }

        public void SetConstant<T>(string name, string themeType, T constant)
        {
            var data = GetOrCreateType(themeType);
            data.Constants[name] = constant;


            ThemeChanged?.Invoke();
        }

        public void SetFont(string name, string themeType, FontInternal font)
        {
            var data = GetOrCreateType(themeType);
            data.Fonts[name] = font;


            ThemeChanged?.Invoke();
        }

        public void SetFontSize(string name, string themeType, int fontSize)
        {
            var data = GetOrCreateType(themeType);
            data.FontSizes[name] = fontSize;

            ThemeChanged?.Invoke();
        }


        public void SetStyleBox(string name, string themeType, StyleBox stylebox)
        {
            var data = GetOrCreateType(themeType);
            data.StyleBoxes[name] = stylebox;


            ThemeChanged?.Invoke();
        }

        public int GetFontSize(string name, string themeType)
        {
            if (!TryGetTypeChain(themeType, out var chain))
                return 0;

            foreach (var type in chain)
            {
                if (types.TryGetValue(type, out var data) &&
                    data.FontSizes.TryGetValue(name, out var size))
                {
                    return size;
                }
            }

            return 0;
        }

        public FontInternal GetFont(string name, string themeType)
        {
            if (!TryGetTypeChain(themeType, out var chain))
                return null;

            foreach (var type in chain)
            {
                if (types.TryGetValue(type, out var data) &&
                    data.Fonts.TryGetValue(name, out var font))
                {
                    return font;
                }
            }

            return null;
        }

        public StyleBox GetStyleBox(string name, string themeType)
        {
            if (!TryGetTypeChain(themeType, out var chain))
                return null;

            foreach (var type in chain)
            {
                if (types.TryGetValue(type, out var data) &&
                    data.StyleBoxes.TryGetValue(name, out var style))
                {
                    return style;
                }
            }

            return null;
        }

        public Vector4 GetColor(string name, string themeType)
        {
            if (!TryGetTypeChain(themeType, out var chain))
                return Vector4.One;

            foreach (var type in chain)
            {
                if (types.TryGetValue(type, out var data) &&
                    data.Colors.TryGetValue(name, out var color))
                {
                    return color;
                }
            }

            return Vector4.One;
        }

        public T GetConstant<T>(string name, string themeType)
        {
            if (!TryGetTypeChain(themeType, out var chain))
                return default;

            foreach (var type in chain)
            {
                if (types.TryGetValue(type, out var data) &&
                    data.Constants.TryGetValue(name, out var value))
                {
                    return (T)value;
                }
            }

            return default;
        }

        public void ClearColor(string name, string themeType)
        {
            if (types.TryGetValue(themeType, out var data))
                data.Colors.Remove(name);


            ThemeChanged?.Invoke();
        }

        public void RenameColor(string oldName, string name, string themeType)
        {
            if (!types.TryGetValue(themeType, out var data))
                return;

            if (!data.Colors.TryGetValue(oldName, out var value))
                return;

            data.Colors.Remove(oldName);
            data.Colors[name] = value;


            ThemeChanged?.Invoke();
        }

        public bool HasColor(string name, string themeType)
        {
            return types.TryGetValue(themeType, out var data)
                && data.Colors.ContainsKey(name);
        }

        public bool HasFontSize(string name, string themeType)
        {
            return types.TryGetValue(themeType, out var data) &&
                   data.FontSizes.ContainsKey(name);
        }

        public bool HasConstant(string name, string themeType)
        {
            return types.TryGetValue(themeType, out var data)
                && data.Constants.ContainsKey(name);
        }


        public void ClearFontSize(string name, string themeType)
        {
            if (types.TryGetValue(themeType, out var data))
                data.FontSizes.Remove(name);

            ThemeChanged?.Invoke();
        }

        public void SetTypeVariation(string themeType, string baseType)
        {
            typeVariations[themeType] = baseType;


            ThemeChanged?.Invoke();
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

                foreach (var kv in src.FontSizes)
                    dst.FontSizes[kv.Key] = kv.Value;
            }


            ThemeChanged?.Invoke();
        }

        private bool TryGetTypeChain(string themeType, out IEnumerable<string> chain)
        {
            List<string> result = new();

            string current = themeType;

            while (!string.IsNullOrEmpty(current))
            {
                result.Add(current);

                if (!typeVariations.TryGetValue(current, out var baseType))
                    break;

                current = baseType;
            }

            chain = result;
            return result.Count > 0;
        }
    }
}
