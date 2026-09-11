using Elemental.Tools.Shortcuts;

namespace Elemental
{
    public sealed class EditorAction
    {
        public EditorActionId Id;
        public string Name = string.Empty;
        public string Label = string.Empty;

        public string? Icon;

        public Shortcut? Shortcut;

        public Action Execute { get; init; } = null!;

    }
}
