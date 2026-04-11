using DevoidEngine.Engine.ProjectSystem;
using ElementalEditor.Utils;
using ImGuiNET;
using NativeFileDialogSharp;
using System.Numerics;

namespace ElementalEditor.Windows
{
    public class SaveAssetDialog
    {
        bool open;

        string name = "";
        string folder = "";
        string extension = "";

        Action<string>? onSave;

        public void Open(string defaultName, string defaultFolder, string ext, Action<string> saveCallback)
        {
            name = defaultName;
            folder = ToRelative(defaultFolder);
            extension = ext;
            onSave = saveCallback;

            open = true;
            ImGui.OpenPopup("Save Asset");
        }

        public void Draw()
        {
            if (!open)
                return;

            if (!ImGui.BeginPopupModal("Save Asset", ref open, ImGuiWindowFlags.AlwaysAutoResize))
                return;

            ImGui.Text("Save Asset");
            ImGui.Separator();

            //-------------------------------------
            // Name
            //-------------------------------------

            ImGui.Text("Name");
            ImGui.InputText("##name", ref name, 256);

            //-------------------------------------
            // Path
            //-------------------------------------

            ImGui.Text("Folder");

            float buttonWidth = ImGui.CalcTextSize(BootstrapIconFont.FolderFill).X + ImGui.GetStyle().FramePadding.X * 2;

            ImGui.PushItemWidth(-buttonWidth - ImGui.GetStyle().ItemSpacing.X);
            ImGui.InputText("##folder", ref folder, 512);
            ImGui.PopItemWidth();

            ImGui.SameLine();

            if (ImGui.Button(BootstrapIconFont.FolderFill))
            {
                var result = Dialog.FolderPicker();

                if (result.IsOk)
                    folder = ToRelative(result.Path);
            }

            ImGui.Spacing();

            string fullPath = Path.Combine(ToAbsolute(folder), name + extension);

            ImGui.TextColored(
                new Vector4(0.7f, 0.7f, 0.7f, 1f),
                Path.Combine(folder, name + extension)
            );

            ImGui.Separator();

            //-------------------------------------
            // Buttons
            //-------------------------------------

            if (ImGui.Button("Save", new Vector2(120, 0)))
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    onSave?.Invoke(fullPath);
                    open = false;
                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                open = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        static string ToRelative(string absolute)
        {
            string root = ProjectManager.Current.AssetPath;

            if (absolute.StartsWith(root))
                return Path.GetRelativePath(root, absolute);

            return absolute;
        }

        static string ToAbsolute(string relative)
        {
            if (Path.IsPathRooted(relative))
                return relative;

            return Path.Combine(ProjectManager.Current.AssetPath, relative);
        }
    }
}