namespace Elemental
{
    public sealed class EditorAction
    {
        public string Name = string.Empty;
        public string Label = string.Empty;

        public string? Icon;

        public Action Execute { get; init; } = null!;

    }
}
