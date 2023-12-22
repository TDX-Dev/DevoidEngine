using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Utilities
{
    public enum MouseButtonEvent
    {
        Left,
        Right,
        Middle,
        None
    }

    public struct KeyboardEvent
    {
        public Keys Key;

        public int scanCode;
        public string modifiers;

        public bool Caps;
    }

    public struct TextInputEvent
    {
        public int Unicode;
    }

    public static class Input
    {

        public static MouseButtonEvent mouseButton;
        private static Vector2 MousePosition;

        public static Vector2 GetMousePos()
        {
            return MousePosition;
        }

        public static void SetMousePos(Vector2 val)
        {
            MousePosition = val;
        }

        public static MouseButtonEvent GetMouseButtonDown()
        {
            return mouseButton;
        }

    }
}
