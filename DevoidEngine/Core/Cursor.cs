using System.Numerics;

namespace DevoidEngine.Core
{
    public class Cursor
    {
        internal static CursorState CursorState;
        internal static CursorShape CursorShape;
        internal static Vector2 MousePosition;

        internal static bool StateDirty = false;
        internal static bool ShapeDirty = false;
        internal static bool PosDirty = false;

        public void SetCursorPosition(Vector2 position)
        {
            MousePosition = position;
            PosDirty = true;
        }

        public void SetCursorState(CursorState state)
        {
            CursorState = state;
            StateDirty = true;
        }

        public CursorState GetCursorState()
        {
            return CursorState;
        }

        public void SetCursorShape(CursorShape shape)
        {
            CursorShape = shape;
            ShapeDirty = true;
        }

        public CursorShape GetCursorShape()
        {
            return CursorShape;
        }
    }
}
