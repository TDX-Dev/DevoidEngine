using DevoidEngine.AssetPipeline;
using DevoidEngine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.ContextMenu
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
                Engine.Instance.AssetDatabase.RefreshDatabase();
            }
        }
    }
}
