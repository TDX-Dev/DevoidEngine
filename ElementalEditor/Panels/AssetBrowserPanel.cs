using DevoidEngine.Engine.ProjectSystem;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Panels
{
    public class AssetBrowserPanel : IEditorPanel
    {
        private string currentDirectory = ProjectManager.Current.AssetPath;

        public void Draw(EditorContext context)
        {
            if (!ImGui.Begin("Assets"))
            {
                ImGui.End();
                return;
            }

            DrawDirectory(currentDirectory);

            ImGui.End();
        }

        void DrawDirectory(string path)
        {
            if (ImGui.Button("Up") && path != "Assets")
            {
                currentDirectory = Directory.GetParent(path).FullName;
                return;
            }

            ImGui.Separator();

            foreach (var dir in Directory.GetDirectories(path))
            {
                if (ImGui.Selectable($"[DIR] {Path.GetFileName(dir)}"))
                {
                    currentDirectory = dir;
                }
            }

            foreach (var file in Directory.GetFiles(path))
            {
                DrawAsset(file);
            }
        }

        void DrawAsset(string file)
        {
            string name = Path.GetFileName(file);

            ImGui.Selectable(name);

            if (ImGui.BeginDragDropSource())
            {
                string relative = Path.GetRelativePath(ProjectManager.Current.AssetPath, file);
                var bytes = Encoding.UTF8.GetBytes(relative);

                unsafe
                {
                    fixed (byte* ptr = bytes)
                    {
                        ImGui.SetDragDropPayload("ASSET_PATH", (IntPtr)ptr, (uint)bytes.Length);
                    }
                }

                ImGui.Text(name);
                ImGui.EndDragDropSource();
            }
        }
    }
}