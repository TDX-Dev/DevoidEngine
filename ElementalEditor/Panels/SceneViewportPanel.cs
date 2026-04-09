using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Panels
{
    class SceneViewportPanel : IEditorPanel
    {
        public void Draw(EditorContext context)
        {
            if (ImGui.Begin("Scene"))
            {
                ImGui.End();
            }
        }
    }
}
