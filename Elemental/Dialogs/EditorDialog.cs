using ImGuiNET;

namespace Elemental.Dialogs
{
    public abstract class EditorDialog : IEditorDialog
    {
        private bool openRequested;

        protected abstract string PopupId { get; }

        public void Open()
        {
            openRequested = true;
            OnOpen();
        }

        protected virtual void OnOpen()
        {
        }

        public bool Draw()
        {
            if (openRequested)
            {
                ImGui.OpenPopup(PopupId);
                openRequested = false;
            }

            if (!ImGui.BeginPopupModal(
                    PopupId,
                    ImGuiWindowFlags.AlwaysAutoResize))
            {
                return false;
            }

            bool close = DrawContents();

            if (close)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();

            return close;
        }

        protected abstract bool DrawContents();
    }
}