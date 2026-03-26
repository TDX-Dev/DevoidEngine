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
    }

    public class LayoutOptions
    {
        public float FlexGrowMain;
        public float FlexGrowCross;

        public float FlexBasis;

        public static readonly LayoutOptions Default = new LayoutOptions
        {
            FlexGrowMain = 1,
            FlexGrowCross = 1,
            FlexBasis = 0
        };
    }
}