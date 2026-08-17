using DevoidEngine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.ProjectSettings
{
    public class ProjectSettingsWindow
    {
        bool open = false;

        string search = "";
        string selectedCategory = "";

        public void Open()
        {
            open = true;
            ImGui.OpenPopup("Project Settings");
        }

        public void Draw()
        {
            if (!open)
                return;

            if (!ImGui.BeginPopupModal("Project Settings", ref open
                /*ImGuiWindowFlags.AlwaysAutoResize*/))
                return;

            var providers = ProjectSettingsRegistry.Providers;

            if (string.IsNullOrEmpty(selectedCategory) && providers.Count > 0)
                selectedCategory = providers[0].Category;

            // Search bar
            ImGui.InputText("Search", ref search, 256);

            ImGui.Separator();

            float sidebarWidth = 200;

            ImGui.BeginChild("sidebar", new Vector2(sidebarWidth, -40), ImGuiChildFlags.Borders);

            var categories = providers
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c);

            foreach (var category in categories)
            {
                bool selected = category == selectedCategory;

                if (ImGui.Selectable(category, selected))
                {
                    selectedCategory = category;
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("content", new Vector2(0, -40), ImGuiChildFlags.Borders);

            foreach (var provider in providers)
            {
                if (provider.Category != selectedCategory)
                    continue;

                DrawProvider(provider);
            }

            ImGui.EndChild();

            ImGui.Separator();

            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 180);

            if (ImGui.Button("Apply & Save Project Settings", new Vector2(160, 0)))
            {
                Engine.Instance.ProjectSystem.SaveSettings();
            }

            ImGui.EndPopup();
        }

        void DrawProvider(IProjectSettingsProvider provider)
        {
            ImGui.Text(provider.Name);
            ImGui.Separator();

            provider.Draw();
        }
    }
}
