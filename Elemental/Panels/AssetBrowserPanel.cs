using DevoidEngine.Core;
using Elemental.ContextMenu;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace Elemental.Panels
{
    public class AssetBrowserPanel : Panel
    {
        private string currentDirectory = Engine.Instance.ProjectSystem.AssetPath;

        private const float FolderPanelWidth = 250.0f;
        private const float AssetItemHeight = 48.0f;
        private const float AssetIconWidth = 32.0f;



        public AssetBrowserPanel() : base("Asset Browser")
        {
        }

        protected override void OnImGuiRender()
        {
            string root = Engine.Instance.ProjectSystem.AssetPath;

            // Make sure the current directory is still valid.
            if (!Directory.Exists(currentDirectory))
                currentDirectory = root;

            Vector2 available = ImGui.GetContentRegionAvail();

            // -------------------------
            // Folder tree
            // -------------------------

            ImGui.BeginChild(
                "AssetBrowserFolders",
                new Vector2(FolderPanelWidth, available.Y),
                ImGuiChildFlags.Borders
            );

            DrawFolderTree(root);

            ImGui.EndChild();

            ImGui.SameLine();

            // -------------------------
            // Asset list
            // -------------------------

            ImGui.BeginChild(
                "AssetBrowserContents",
                new Vector2(0, available.Y),
                ImGuiChildFlags.Borders
            );

            DrawCurrentDirectory();

            ImGui.EndChild();
        }

        private void DrawFolderTree(string path)
        {
            string root = Engine.Instance.ProjectSystem.AssetPath;

            string name = Path.GetFileName(path);

            // The asset root has no useful filename when it's something
            // like "C:\" so give it a proper label.
            if (Path.GetFullPath(path) == Path.GetFullPath(root))
                name = $"{LucideIconFont.IconFolderRoot} Assets";
            else
                name = $"{LucideIconFont.IconFolder} " + name;

                bool isCurrent = Path.GetFullPath(path) ==
                                 Path.GetFullPath(currentDirectory);

            ImGuiTreeNodeFlags flags =
                ImGuiTreeNodeFlags.OpenOnArrow |
                ImGuiTreeNodeFlags.SpanAvailWidth;

            if (isCurrent)
                flags |= ImGuiTreeNodeFlags.Selected;

            string[] directories = Directory.GetDirectories(path);

            // Leaf directories don't need a tree arrow.
            if (directories.Length == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;

            bool open = ImGui.TreeNodeEx(name, flags);

            // Clicking the folder name selects it.
            if (ImGui.IsItemClicked())
            {
                currentDirectory = path;
            }

            if (open)
            {
                foreach (string directory in directories)
                {
                    DrawFolderTree(directory);
                }

                ImGui.TreePop();
            }
        }

        private void DrawCurrentDirectory()
        {
            string[] directories = Directory.GetDirectories(currentDirectory);
            string[] files = Directory.GetFiles(currentDirectory);

            // Optional: show the current path at the top.
            ImGui.TextUnformatted(
                Path.GetRelativePath(
                    Engine.Instance.ProjectSystem.AssetPath,
                    currentDirectory
                )
            );

            ImGui.Separator();

            // Directories appear first.
            foreach (string directory in directories)
            {
                DrawDirectoryItem(directory);
            }

            foreach (string file in files)
            {
                if (Path.GetExtension(file).Equals(
                    ".meta",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                DrawAsset(file);
            }
        }

        private void DrawDirectoryItem(string directory)
        {
            string name = Path.GetFileName(directory);

            DrawAssetItem(
                name,
                LucideIconFont.IconFolder,
                () => currentDirectory = directory,
                directory);
        }


        private void DrawAsset(string file)
        {
            string name = Path.GetFileName(file);

            DrawAssetItem(
                name,
                LucideIconFont.IconFile,
                null,
                file);

            if (ImGui.BeginDragDropSource())
            {
                string relative = Path.GetRelativePath(
                    Engine.Instance.ProjectSystem.AssetPath,
                    file);

                byte[] bytes = Encoding.UTF8.GetBytes(relative);

                unsafe
                {
                    fixed (byte* ptr = bytes)
                    {
                        ImGui.SetDragDropPayload(
                            "ASSET_PATH",
                            (IntPtr)ptr,
                            (uint)bytes.Length);
                    }
                }

                ImGui.TextUnformatted(name);

                ImGui.EndDragDropSource();
            }
        }

        private static void DrawAssetItem(
            string name,
            string icon,
            Action? doubleClickAction,
            string contextPath
        )
        {
            Vector2 size = new(
                ImGui.GetContentRegionAvail().X,
                AssetItemHeight);

            ImGui.Selectable(
                $"##AssetItem_{contextPath}",
                false,
                ImGuiSelectableFlags.AllowDoubleClick,
                size);

            Vector2 min = ImGui.GetItemRectMin();
            //Vector2 max = ImGui.GetItemRectMax();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // Icon
            Vector2 iconSize = ImGui.CalcTextSize(icon);

            Vector2 iconPos = new(
                min.X + 12.0f,
                min.Y + (AssetItemHeight - iconSize.Y) * 0.5f);

            drawList.AddText(
                iconPos,
                ImGui.GetColorU32(ImGuiCol.Text),
                icon);

            // Name
            Vector2 textPos = new(
                min.X + AssetIconWidth + 12.0f,
                min.Y + (AssetItemHeight - ImGui.GetTextLineHeight()) * 0.5f);

            drawList.AddText(
                textPos,
                ImGui.GetColorU32(ImGuiCol.Text),
                name);

            // Double click
            if (doubleClickAction != null &&
                ImGui.IsItemHovered() &&
                ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                doubleClickAction();
            }

            // Context menu
            if (ImGui.BeginPopupContextItem())
            {
                AssetBrowserCommonMenu.Draw(contextPath);

                ImGui.Separator();

                AssetContextMenuRegistry.Draw(contextPath);

                ImGui.EndPopup();
            }
        }
    }
}