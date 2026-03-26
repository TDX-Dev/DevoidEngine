using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public static class FlexboxTools
    {
        //float ComputeJustifyOffset(float freeSpace)
        //{
        //    return Justify switch
        //    {
        //        JustifyContent.Start => 0,
        //        JustifyContent.Center => freeSpace * 0.5f,
        //        JustifyContent.End => freeSpace,
        //        JustifyContent.SpaceBetween => 0,
        //        //JustifyContent.SpaceAround => freeSpace / (count * 2),
        //        //JustifyContent.SpaceEvenly => freeSpace / (count + 1),
        //    };
        //}

        public static void ComputeJustify(
            JustifyContent justify,
            float freeSpace,
            int count,
            float gap,
            out float startOffset,
            out float interItemGap)
        {
            startOffset = 0f;
            interItemGap = gap;

            if (count == 0)
                return;


            switch (justify)
            {
                case JustifyContent.Start:
                    break;

                case JustifyContent.End:
                    startOffset = freeSpace;
                    break;

                case JustifyContent.Center:
                    startOffset = freeSpace * 0.5f;
                    break;

                case JustifyContent.SpaceBetween:
                    interItemGap = gap + freeSpace / (count - 1);
                    break;

                case JustifyContent.SpaceAround:
                    startOffset = freeSpace / (2 * count);
                    interItemGap = gap + freeSpace / count;
                    break;

                case JustifyContent.SpaceEvenly:
                    startOffset = freeSpace / (count + 1);
                    interItemGap = gap + freeSpace / (count + 1);
                    break;
            }
        }


        public static float ComputeCrossOffset(
            AlignItems align,
            float containerCross,
            float crossSize
        )
        {
            return align switch
            {
                AlignItems.Start => 0f,
                AlignItems.Center => (containerCross - crossSize) * 0.5f,
                AlignItems.End => containerCross - crossSize,
                AlignItems.Stretch => 0f,
                _ => 0f
            };
        }



        public static float Main(Vector2 v, FlexDirection dir)
            => dir == FlexDirection.Row ? v.X : v.Y;

        public static float Cross(Vector2 v, FlexDirection dir)
            => dir == FlexDirection.Row ? v.Y : v.X;

        public static Vector2 FromMainCross(float main, float cross, FlexDirection dir)
            => dir == FlexDirection.Row
                ? new Vector2(main, cross)
                : new Vector2(cross, main);


    }
}