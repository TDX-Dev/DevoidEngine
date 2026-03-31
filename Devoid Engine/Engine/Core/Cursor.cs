using OpenTK.Windowing.Common.Input;

namespace DevoidEngine.Engine.Core
{
    public static class Cursor
    {
        internal static CursorState cursorState;
        internal static CursorShape cursorShape;

        internal static bool stateDirty = false;
        internal static bool shapeDirty = false;

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