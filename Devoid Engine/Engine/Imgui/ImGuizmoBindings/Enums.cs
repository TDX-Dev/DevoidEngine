namespace DevoidEngine.Engine.Imgui.ImGuizmoBindings
{
    public enum Operation
    {
        TranslateX = 1 << 0,
        TranslateY = 1 << 1,
        TranslateZ = 1 << 2,
        RotateX = 1 << 3,
        RotateY = 1 << 4,
        RotateZ = 1 << 5,
        RotateScreen = 1 << 6,
        ScaleX = 1 << 7,
        ScaleY = 1 << 8,
        ScaleZ = 1 << 9,
        Bounds = 1 << 10,
        ScaleXU = 1 << 11,
        ScaleYU = 1 << 12,
        ScaleZU = 1 << 13,
        Translate = TranslateX | TranslateY | TranslateZ,
        Rotate = RotateX | RotateY | RotateZ | RotateScreen,
        Scale = ScaleX | ScaleY | ScaleZ,
        ScaleU = ScaleXU | ScaleYU | ScaleZU,
        Universal = Translate | Rotate | ScaleU
    }

    public enum Mode
    {
        Local,
        World
    }

    public enum Color
    {
        DirectionX,
        DirectionY,
        DirectionZ,
        PlaneX,
        PlaneY,
        PlaneZ,
        Selection,
        Inactive,
        TranslationLine,
        ScaleLine,
        RotationUsingBorder,
        RotationUsingFill,
        HatchedAxisLines,
        Text,
        TextShadow,
        Count
    }
}