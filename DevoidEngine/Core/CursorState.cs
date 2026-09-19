namespace DevoidEngine.Core
{
    public enum CursorState
    {
        /// <summary>
        /// The cursor visible and cursor motion is not limited.
        /// </summary>
        Normal,

        /// <summary>
        /// Hides the cursor when over a window.
        /// </summary>
        Hidden,

        /// <summary>
        /// Hides the cursor and locks it to the specified window.
        /// </summary>
        Grabbed,

        /// <summary>
        /// Confines the cursor to the window content area.
        /// </summary>
        Confined,
    }
}
