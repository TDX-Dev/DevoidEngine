using DevoidEngine.Engine.Core;
using ElementalEditor.Editor.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Inspector.Tabs
{
    public class MaterialsInspectorTab : IInspectorTab
    {
        public string Id => "materials";
        public string Icon => BootstrapIconFont.Circle;
        public Vector4 IconColor => new Vector4(70, 132, 50, 255) / 255;

        public void Draw(EditorContext context, GameObject obj)
        {
            ImGui.Text("Materials");
        }
    }
}
