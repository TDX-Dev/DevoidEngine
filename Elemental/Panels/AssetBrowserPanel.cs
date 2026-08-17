using DevoidEngine.Core;
using Elemental.ContextMenu;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace Elemental.Panels
{

    public class AssetBrowserPanel : Panel
    {
        private sealed class FolderNode
        {
            public string Path = string.Empty;
            public string Name = string.Empty;

            public List<FolderNode>? Children;
        }


        private static readonly Dictionary<string, string> FileIcons =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".cs"] = LucideIconFont.IconFileCode,
                [".json"] = LucideIconFont.IconFileJson,
                [".xml"] = LucideIconFont.IconFileCode,
                [".shader"] = LucideIconFont.IconFileCode,
                [".hlsl"] = LucideIconFont.IconFileCode,

                [".png"] = LucideIconFont.IconImage,
                [".jpg"] = LucideIconFont.IconImage,
                [".jpeg"] = LucideIconFont.IconImage,
                [".bmp"] = LucideIconFont.IconImage,

                [".wav"] = LucideIconFont.IconAudioLines,
                [".mp3"] = LucideIconFont.IconAudioLines,
                [".ogg"] = LucideIconFont.IconAudioLines,

                [".obj"] = LucideIconFont.IconBox,
                [".fbx"] = LucideIconFont.IconBox,

                [".scene"] = LucideIconFont.IconPanelsTopLeft,
                [".prefab"] = LucideIconFont.IconPackage,

                [".ttf"] = LucideIconFont.IconTypeOutline,
            };



        private string currentDirectory = Engine.Instance.ProjectSystem.AssetPath;

        private readonly string _rootPath = string.Empty;

        private string[] _currentDirectories = [];
        private string[] _currentFiles = [];

        private bool _directoryDirty = true;
        private FolderNode? _folderRoot;
        private bool _folderTreeDirty = true;

        private readonly Dictionary<string, string> _normalizedPaths =
            new(StringComparer.OrdinalIgnoreCase);

        private const float FolderPanelWidth = 250.0f;
        private const float AssetItemHeight = 48.0f;
        private const float AssetIconWidth = 32.0f;



        public AssetBrowserPanel() : base("Asset Browser")
        {
            _rootPath = Path.GetFullPath(
                Engine.Instance.ProjectSystem.AssetPath);

            currentDirectory = _rootPath;
        }

        private void NavigateTo(string directory)
        {
            directory = Path.GetFullPath(directory);

            if (string.Equals(
                currentDirectory,
                directory,
                StringComparison.OrdinalIgnoreCase))
                return;

            currentDirectory = directory;
            _directoryDirty = true;
        }

        private void RefreshCurrentDirectory()
        {
            try
            {
                _currentDirectories =
                    Directory.GetDirectories(currentDirectory);

                _currentFiles =
                    Directory.GetFiles(currentDirectory);
            }
            catch (DirectoryNotFoundException)
            {
                currentDirectory = _rootPath;

                _currentDirectories =
                    Directory.GetDirectories(currentDirectory);

                _currentFiles =
                    Directory.GetFiles(currentDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                _currentDirectories = [];
                _currentFiles = [];
            }

            _directoryDirty = false;
        }

        protected override void OnImGuiRender()
        {
            if (!Directory.Exists(currentDirectory))
                NavigateTo(_rootPath);

            if (_directoryDirty)
                RefreshCurrentDirectory();

            Vector2 available = ImGui.GetContentRegionAvail();

            ImGui.BeginChild(
                "AssetBrowserFolders",
                new Vector2(FolderPanelWidth, available.Y),
                ImGuiChildFlags.Borders
            );

            DrawFolderTree(_rootPath);

            ImGui.EndChild();

            ImGui.SameLine();

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
            FolderNode node;

            if (string.Equals(
                path,
                _rootPath,
                StringComparison.OrdinalIgnoreCase))
            {
                node = GetFolderRoot();
            }
            else
            {
                // This overload is no longer what we want for recursion.
                return;
            }

            DrawFolderNode(node);
        }

        private void DrawFolderNode(FolderNode node)
        {
            bool isRoot = string.Equals(
                node.Path,
                _rootPath,
                StringComparison.OrdinalIgnoreCase);

            string name = isRoot
                ? $"{LucideIconFont.IconFolderRoot} Assets"
                : $"{LucideIconFont.IconFolder} {node.Name}";

            bool isCurrent = string.Equals(
                node.Path,
                currentDirectory,
                StringComparison.OrdinalIgnoreCase);

            ImGuiTreeNodeFlags flags =
                ImGuiTreeNodeFlags.OpenOnArrow |
                ImGuiTreeNodeFlags.SpanAvailWidth;

            if (isCurrent)
                flags |= ImGuiTreeNodeFlags.Selected;

            LoadFolderChildren(node);

            if (node.Children!.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;

            ImGui.PushID(node.Path);

            bool open = ImGui.TreeNodeEx(name, flags);

            if (ImGui.IsItemClicked())
                NavigateTo(node.Path);

            if (open)
            {
                foreach (FolderNode child in node.Children)
                    DrawFolderNode(child);

                ImGui.TreePop();
            }

            ImGui.PopID();
        }
        private void DrawCurrentDirectory()
        {
            string relativePath = Path.GetRelativePath(
                _rootPath,
                currentDirectory);

            ImGui.TextUnformatted(
                relativePath == "."
                    ? "Assets"
                    : relativePath);

            ImGui.Separator();

            DrawParentDirectory();

            foreach (string directory in _currentDirectories)
                DrawDirectoryItem(directory);

            foreach (string file in _currentFiles)
            {
                if (Path.GetExtension(file).Equals(
                    ".meta",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                DrawAsset(file);
            }
        }

        private void DrawParentDirectory()
        {
            if (string.Equals(
                currentDirectory,
                _rootPath,
                StringComparison.OrdinalIgnoreCase))
                return;

            if (DrawAssetItem(
                "..",
                LucideIconFont.IconFolderUp,
                "__parent_directory__"))
            {
                string? parent =
                    Directory.GetParent(currentDirectory)?.FullName;

                if (parent != null &&
                    parent.StartsWith(
                        _rootPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    NavigateTo(parent);
                }
            }
        }

        private void DrawDirectoryItem(string directory)
        {
            string name = Path.GetFileName(directory);

            if (DrawAssetItem(
                name,
                LucideIconFont.IconFolder,
                directory))
            {
                NavigateTo(directory);
            }
        }


        private void DrawAsset(string file)
        {
            string name = Path.GetFileName(file);

            DrawAssetItem(
                name,
                GetFileIcon(file),
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
        private static bool DrawAssetItem(
            string name,
            string icon,
            string contextPath
        )
        {
            Vector2 size = new(
                ImGui.GetContentRegionAvail().X,
                AssetItemHeight);

            ImGui.PushID(contextPath);

            ImGui.Selectable(
                "##AssetItem",
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

            ImGui.PopID();

            // Double click
            bool doubleClicked =
                    ImGui.IsItemHovered() &&
                    ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);

            // Context menu
            if (ImGui.BeginPopupContextItem())
            {
                AssetBrowserCommonMenu.Draw(contextPath);

                ImGui.Separator();

                AssetContextMenuRegistry.Draw(contextPath);

                ImGui.EndPopup();
            }
            return doubleClicked;
        }

        private static string GetFileIcon(string file)
        {
            string extension = Path.GetExtension(file);

            if (FileIcons.TryGetValue(extension, out string? icon))
                return icon;

            return LucideIconFont.IconFile;
        }

        public static void RegisterFileIcon(string extension, string icon)
        {
            if (!extension.StartsWith('.'))
                extension = "." + extension;

            FileIcons[extension] = icon;
        }

        private FolderNode GetFolderRoot()
        {
            if (_folderRoot == null || _folderTreeDirty)
            {
                _folderRoot = new FolderNode
                {
                    Path = _rootPath,
                    Name = "Assets"
                };

                _folderTreeDirty = false;
            }

            return _folderRoot;
        }

        private static void LoadFolderChildren(FolderNode node)
        {
            if (node.Children != null)
                return;

            node.Children = [];

            try
            {
                foreach (string directory in Directory.GetDirectories(node.Path))
                {
                    node.Children.Add(new FolderNode
                    {
                        Path = directory,
                        Name = Path.GetFileName(directory)
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
    }
}