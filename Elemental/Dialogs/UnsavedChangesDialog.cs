using ImGuiNET;

namespace Elemental.Dialogs
{
    public sealed class UnsavedChangesDialog : EditorDialog
    {
        protected override string PopupId => "Unsaved Changes";

        private readonly string message;
        private readonly Action onSave;
        private readonly Action onDiscard;

        public UnsavedChangesDialog(
            string title,
            string message,
            Action onSave,
            Action onDiscard)
        {
            this.message = message;
            this.onSave = onSave;
            this.onDiscard = onDiscard;
        }

        protected override bool DrawContents()
        {
            ImGui.TextWrapped(message);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Save", new System.Numerics.Vector2(120, 0)))
            {
                onSave();
                return true;
            }

            ImGui.SameLine();

            if (ImGui.Button("Discard", new System.Numerics.Vector2(120, 0)))
            {
                onDiscard();
                return true;
            }

            ImGui.SameLine();

            if (ImGui.Button("Cancel", new System.Numerics.Vector2(120, 0)))
            {
                return true;
            }

            return false;
        }
    }
}