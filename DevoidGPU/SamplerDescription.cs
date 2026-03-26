namespace DevoidGPU
{
    public class SamplerDescription
    {
        public TextureFilter MinFilter { get; set; }
        public TextureFilter MagFilter { get; set; }
        public MipFilter MipFilter { get; set; }

        public TextureWrapMode WrapU { get; set; }
        public TextureWrapMode WrapV { get; set; }
        public TextureWrapMode WrapW { get; set; }

        public int MaxAnisotropy { get; set; } = 1;
        public float MipLODBias { get; set; } = 0.0f;
        public float MinLOD { get; set; } = 0.0f;
        public float MaxLOD { get; set; } = float.MaxValue;

        public static SamplerDescription Default => new SamplerDescription()
        {
            MinFilter = TextureFilter.Linear,
            MagFilter = TextureFilter.Linear,
            WrapU = TextureWrapMode.ClampToEdge,
            WrapV = TextureWrapMode.ClampToEdge,
            WrapW = TextureWrapMode.ClampToEdge
        };

    }


    public enum TextureWrapMode
    {
        Repeat,
        MirroredRepeat,
        ClampToEdge,
        ClampToBorder,
        MirrorClamp
    }

    public enum TextureFilter
    {
        Nearest,
        Linear
    }

    public enum MipFilter
    {
        None,
        Nearest,
        Linear
    }
}