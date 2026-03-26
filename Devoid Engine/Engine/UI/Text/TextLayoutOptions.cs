namespace DevoidEngine.Engine.UI.Text
{

    public enum TextAlign
    {
        Left,
        Center,
        Right
    }

    public enum TextOverflow
    {
        Overflow,
        Wrap,
        Clip,
        Ellipsis
    }

    public struct TextLayoutOptions
    {
        public float MaxWidth;
        public TextAlign Align;
        public TextOverflow Overflow;

        public static readonly TextLayoutOptions Default = new TextLayoutOptions
        {
            MaxWidth = float.PositiveInfinity,
            Align = TextAlign.Left,
            Overflow = TextOverflow.Wrap
        };
    }
}