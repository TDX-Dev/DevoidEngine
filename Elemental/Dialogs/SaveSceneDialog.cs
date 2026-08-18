using ImGuiNET;
using System;
using System.IO;
using System.Numerics;

namespace Elemental.Dialogs
{
    public sealed class SaveSceneDialog : EditorDialog
    {
        protected override string PopupId => "Save Scene";

        private readonly Func<string, bool> save;
        private readonly Action<string>? onSaved;

        private string sceneName;
        private string sceneDirectory;

        private string error = "";

        public SaveSceneDialog(
            string sceneName,
            string sceneDirectory,
            Func<string, bool> save,
            Action<string>? onSaved = null)
        {
            this.sceneName = sceneName;
            this.sceneDirectory = sceneDirectory;
            this.save = save;
            this.onSaved = onSaved;
        }

        protected override void OnOpen()
        {
            error = "";
        }

        protected override bool DrawContents()
        {
            ImGui.TextUnformatted("Name");

            ImGui.SetNextItemWidth(450);

            ImGui.InputText(
                "##SceneName",
                ref sceneName,
                256);

            ImGui.Spacing();

            ImGui.TextUnformatted("Location");

            ImGui.SetNextItemWidth(450);

            ImGui.InputText(
                "##SceneDirectory",
                ref sceneDirectory,
                1024);

            ImGui.Spacing();

            string filename = sceneName.Trim();

            if (!filename.EndsWith(
                    ".scene",
                    StringComparison.OrdinalIgnoreCase))
            {
                filename += ".scene";
            }

            string path = Path.Combine(
                sceneDirectory,
                filename);

            ImGui.TextDisabled(path);

            if (!string.IsNullOrEmpty(error))
            {
                ImGui.Spacing();

                ImGui.TextColored(
                    new Vector4(1f, 0.3f, 0.3f, 1f),
                    error);
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Save", new Vector2(-1, 0)))
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    error = "Scene name cannot be empty.";
                }
                else if (string.IsNullOrWhiteSpace(sceneDirectory))
                {
                    error = "Scene location cannot be empty.";
                }
                else if (!Directory.Exists(sceneDirectory))
                {
                    error = "The specified directory does not exist.";
                }
                else
                {
                    string finalPath =
                        Path.Combine(
                            sceneDirectory,
                            filename);

                    if (save(finalPath))
                    {
                        onSaved?.Invoke(finalPath);
                        return true;
                    }

                    error = "Failed to save scene.";
                }
            }

            if (ImGui.Button("Cancel", new Vector2(-1, 0)))
            {
                return true;
            }

            return false;
        }
    }
}