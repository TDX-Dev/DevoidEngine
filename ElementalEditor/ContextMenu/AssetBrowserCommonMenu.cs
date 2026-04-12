using DevoidEngine.Engine.AssetPipeline;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.ContextMenu
{
    static class AssetBrowserCommonMenu
    {
        public static void Draw(string path)
        {
            if (ImGui.MenuItem("Rename"))
            {
                // implement rename popup later
            }

            if (ImGui.MenuItem("Delete"))
            {
                File.Delete(path);
                AssetDatabase.RefreshDatabase();
            }
        }
    }
}
