using System;

namespace Elemental
{
    public sealed class EditorDialogManager
    {
        private IEditorDialog? activeDialog;

        public void Open(IEditorDialog dialog)
        {
            activeDialog = dialog;
            dialog.Open();
        }

        public void Close()
        {
            activeDialog = null;
        }

        public void Draw()
        {
            if (activeDialog == null)
                return;

            IEditorDialog dialog = activeDialog;

            if (dialog.Draw() &&
                ReferenceEquals(activeDialog, dialog))
            {
                activeDialog = null;
            }
        }
    }
}