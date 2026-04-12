using DevoidEngine.Engine.ProjectSystem;
using ElementalEditor.ContextMenu;
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
            string root = ProjectManager.Current.AssetPath;

            bool atRoot = Path.GetFullPath(path) == Path.GetFullPath(root);

            if (atRoot)
                ImGui.BeginDisabled();

            if (ImGui.Button("Up") && !atRoot)
            {
                currentDirectory = Directory.GetParent(path).FullName;
                return;
            }

            if (atRoot)
                ImGui.EndDisabled();

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

            //---------------------------------
            // Context Menu
            //---------------------------------

            if (ImGui.BeginPopupContextItem())
            {
                AssetBrowserCommonMenu.Draw(file);

                ImGui.Separator();

                AssetContextMenuRegistry.Draw(file);

                ImGui.EndPopup();
            }

            //---------------------------------
            // Drag & Drop
            //---------------------------------

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