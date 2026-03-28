namespace DevoidEngine.Engine.Core
{
    public static class Cursor
    {
        internal static CursorState cursorState;
        internal static bool isDirty = false;

        public static void SetCursorState(CursorState state)
        {
            cursorState = state;
            isDirty = true;
        }

        public static CursorState GetCursorState()
        {
            return cursorState;
        }

    }
}