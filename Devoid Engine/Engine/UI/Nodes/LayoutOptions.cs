namespace DevoidEngine.Engine.UI.Nodes
{
    public struct Padding
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public float Horizontal => Left + Right;
        public float Vertical => Top + Bottom;

        public static Padding GetAll(float val) => new Padding { Left = val, Right = val, Top = val, Bottom = val };
        public static Padding GetVertical(float val) => new Padding { Top = val, Bottom = val };
        public static Padding GetHorizontal(float val) => new Padding { Left = val, Right = val };
    }

    public enum FlexWrap
    {
        NoWrap,
        Wrap
    }

    public class LayoutOptions
    {
        public float FlexGrowMain;
        public float FlexGrowCross;
        public float FlexBasis;
        public FlexWrap Wrap;

        public static readonly LayoutOptions Default = new LayoutOptions
        {
            FlexGrowMain = 1,
            FlexGrowCross = 1,
            FlexBasis = 0,
            Wrap = FlexWrap.NoWrap
        };
    }
}