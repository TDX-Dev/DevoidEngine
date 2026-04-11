using DevoidEngine.Engine.ProjectSystem;
using ImGuiNET;

namespace ElementalEditor.Panels
{
    public class ProjectSettingsPanel : IEditorPanel
    {
        public void Draw(EditorContext context)
        {
            if (!ImGui.Begin("Project Settings"))
            {
                ImGui.End();
                return;
            }

            var project = ProjectManager.Current;
            if (project == null)
            {
                ImGui.Text("No project loaded.");
                ImGui.End();
                return;
            }

            var settings = project.Settings;

            int width = settings.RenderWidth;
            int height = settings.RenderHeight;
            string startupScene = settings.StartupScene;

            if (ImGui.InputInt("Render Width", ref width))
                settings.RenderWidth = width;

            if (ImGui.InputInt("Render Height", ref height))
                settings.RenderHeight = height;

            if (ImGui.InputText("Startup Scene", ref startupScene, 256))
                settings.StartupScene = startupScene;

            ImGui.Spacing();

            if (ImGui.Button("Save Settings"))
            {
                Save(project);
            }

            ImGui.End();
        }

        void Save(Project project)
        {
            string path = Path.Combine(project.SettingsPath, "ProjectSettings.json");

            var json = System.Text.Json.JsonSerializer.Serialize(project.Settings);

            File.WriteAllText(path, json);
        }
    }
}