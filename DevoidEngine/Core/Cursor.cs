using OpenTK.Windowing.Common;
using System.Numerics;

namespace DevoidEngine.Core
{
    public class Cursor
    {
        internal static CursorState cursorState;
        internal static CursorShape cursorShape;
        internal static Vector2 mousePosition;

        internal static bool stateDirty = false;
        internal static bool shapeDirty = false;
        internal static bool posDirty = false;

        public void SetCursorPosition(Vector2 position)
        {
            mousePosition = position;
        }

        public void SetCursorState(CursorState state)
        {
            cursorState = state;
            stateDirty = true;
        }

        public CursorState GetCursorState()
        {
            return cursorState;
        }

        public void SetCursorShape(CursorShape shape)
        {
            cursorShape = shape;
            shapeDirty = true;
        }

        public CursorShape GetCursorShape()
        {
            return cursorShape;
        }
    }
}
