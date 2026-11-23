namespace DevoidGPU
{
    public struct BlendState
    {
        public bool Enable;

        public BlendFactor SrcColor;
        public BlendFactor DstColor;
        public BlendOp ColorOp;

        public BlendFactor SrcAlpha;
        public BlendFactor DstAlpha;
        public BlendOp AlphaOp;

        public ColorMask WriteMask;

        public static BlendState FromBlendMode(BlendMode mode)
        {
            return mode switch
            {
                BlendMode.AlphaBlend => new()
                {
                    Enable = true,

                    SrcColor = BlendFactor.SrcColor,
                    DstColor = BlendFactor.InvSrcAlpha,
                    ColorOp = BlendOp.Add,


                    SrcAlpha = BlendFactor.One,
                    DstAlpha = BlendFactor.InvSrcAlpha,
                    AlphaOp = BlendOp.Add,

                    WriteMask = ColorMask.All
                },
                BlendMode.Additive => new()
                {
                    Enable = true,

                    SrcColor = BlendFactor.SrcAlpha,
                    DstColor = BlendFactor.One,
                    ColorOp = BlendOp.Add,

                    SrcAlpha = BlendFactor.One,
                    DstAlpha = BlendFactor.One,
                    AlphaOp = BlendOp.Add,

                    WriteMask = ColorMask.All
                },
                _ => throw new ArgumentException(null, nameof(mode))
            };
        }


    }
}
