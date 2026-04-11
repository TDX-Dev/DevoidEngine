using DevoidEngine.Engine.ProjectSystem;
using ImGuiNET;
using NativeFileDialogSharp;
using System.IO;
using System.Text;

namespace ElementalEditor.ProjectSettings
{
    public class GameSettingsProvider : IProjectSettingsProvider
    {
        public string Category => "Game";
        public string Name => "General";

        public void Draw()
        {
            var settings = ProjectManager.Current.Settings;

            string display = settings.StartupScene ?? "";

            ImGui.Text("Startup Scene");

            ImGui.PushItemWidth(-80);

            ImGui.BeginDisabled();
            ImGui.InputText("##StartupScene", ref display, 512);
            ImGui.EndDisabled();

            ImGui.PopItemWidth();

            ImGui.SameLine();

            //--------------------------------------------------
            // Scene picker
            //--------------------------------------------------

            if (ImGui.Button("📁"))
            {
                var result = Dialog.FileOpen("scene");

                if (result.IsOk)
                {
                    string assetRoot = ProjectManager.Current.AssetPath;

                    string relative = Path.GetRelativePath(assetRoot, result.Path)
                        .Replace("\\", "/");

                    settings.StartupScene = relative;
                }
            }

            ImGui.SameLine();

            //--------------------------------------------------
            // Clear
            //--------------------------------------------------

            if (ImGui.Button("X"))
            {
                settings.StartupScene = "";
            }

            //--------------------------------------------------
            // Drag & Drop
            //--------------------------------------------------

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("ASSET_PATH");

                unsafe
                {
                    if (payload.NativePtr != null)
                    {
                        string relativePath = Encoding.UTF8.GetString(
                            (byte*)payload.Data,
                            payload.DataSize
                        );

                        if (Path.GetExtension(relativePath).ToLower() == ".scene")
                        {
                            settings.StartupScene = relativePath;
                        }
                    }
                }

                ImGui.EndDragDropTarget();
            }
        }
    }
}