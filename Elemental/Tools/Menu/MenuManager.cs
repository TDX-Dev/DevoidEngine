using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Menu
{
    public class MenuManager
    {
        public int MainMenuSize = 25;

        private readonly Dictionary<string, MenuItem> _menus = [];

        private readonly List<MenuNode> _nodes = [];

        public void Register(string path,  MenuItem menuItem)
        {
            _menus.Add(path, menuItem);
            RebuildTree();
        }

        public void OnImguiRender(EditorContext context)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            if (!ImGui.BeginMainMenuBar())
                return;

            MainMenuSize = (int)ImGui.GetWindowSize().Y;

            foreach (var node in _nodes)
            {
                DrawNode(node, context);
            }

            ImGui.EndMainMenuBar();
            ImGui.PopStyleVar();
        }

        private void DrawNode(MenuNode node, EditorContext context)
        {
            // Leaf menu item
            if (node.MenuItem != null)
            {
                if (ImGui.MenuItem(node.MenuItem.Value.Name, node.MenuItem.Value.EditorAction?.Shortcut?.ToString()))
                {
                    node.MenuItem.Value.EditorAction?.Execute?.Invoke();
                }
                return;
            }

            // Menu containing children
            if (!ImGui.BeginMenu(node.Name))
                return;

            foreach (var child in node.Children)
            {
                DrawNode(child, context);
            }

            ImGui.EndMenu();
        }

        private void RebuildTree()
        {
            _nodes.Clear();

            foreach (var (path, menuItem) in _menus)
            {
                string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

                List<MenuNode> currentChildren = _nodes;

                for (int i = 0; i < segments.Length; i++)
                {
                    string segment = segments[i];

                    MenuNode? node = currentChildren.FirstOrDefault(x => x.Name == segment);

                    if (node == null)
                    {
                        node = new MenuNode(segment);

                        currentChildren.Add(node);
                    }

                    currentChildren = node.Children;
                }
                currentChildren.Add(new MenuNode(menuItem.Name, menuItem));
            }
        }


    }
}
