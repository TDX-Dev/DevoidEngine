using DevoidEngine.Engine.ProjectSystem;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.ProjectSettings
{
    public class RenderingSettingsProvider : IProjectSettingsProvider
    {
        public string Category => "Engine";
        public string Name => "Rendering";

        public void Draw()
        {
            var settings = ProjectManager.Current.Settings;

            int width = settings.RenderWidth;
            int height = settings.RenderHeight;

            if (ImGui.InputInt("Render Width", ref width))
                settings.RenderWidth = width;

            if (ImGui.InputInt("Render Height", ref height))
                settings.RenderHeight = height;
        }
    }
}
