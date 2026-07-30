namespace DevoidEngine.UI.Text
{
    public sealed class TextLayoutResult
    {
        public readonly List<PositionedGlyph> Glyphs = [];
        public readonly List<int> LineStarts = [];
        public readonly List<float> LineWidths = [];

        public float Width;
        public float Height;
        public int LineCount;

        public void Clear()
        {
            Glyphs.Clear();
            LineStarts.Clear();
            LineWidths.Clear();

            Width = 0;
            Height = 0;
            LineCount = 0;
        }
    }
}
