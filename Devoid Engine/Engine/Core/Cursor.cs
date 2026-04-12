using OpenTK.Windowing.Common.Input;
using System.Numerics;

namespace DevoidEngine.Engine.Core
{
    public static class Cursor
    {
        internal static CursorState cursorState;
        internal static CursorShape cursorShape;
        internal static Vector2 mousePosition;

        internal static bool stateDirty = false;
        internal static bool shapeDirty = false;
        internal static bool posDirty = false;

        public static void SetCursorPosition(Vector2 position)
        {
            mousePosition = position;
        }

        public static void SetCursorState(CursorState state)
        {
            cursorState = state;
            stateDirty = true;
        }

        public static CursorState GetCursorState()
        {
            return cursorState;
        }

        public static void SetCursorShape(CursorShape shape)
        {
            cursorShape = shape;
            shapeDirty = true;
        }

        public static CursorShape GetCursorShape()
        {
            return cursorShape;
        }
    }
}