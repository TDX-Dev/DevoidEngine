using DevoidEngine.Engine.ProjectSystem;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.ProjectSettings
{
    public class PhysicsSettingsProvider : IProjectSettingsProvider
    {
        public string Category => "Engine";
        public string Name => "Physics";

        public void Draw()
        {
            var settings = ProjectManager.Current.Settings;

            int physicsFrequency = settings.PhysicsUpdateFrequency;
            bool physicsInterpolation = settings.UsePhysicsInterpolation;

            if (ImGui.InputInt("Physics Update Frequency", ref physicsFrequency))
                settings.PhysicsUpdateFrequency = physicsFrequency;

            if (ImGui.Checkbox("Use Physics Interpolation", ref physicsInterpolation))
                settings.UsePhysicsInterpolation = physicsInterpolation;
        }
    }
}
