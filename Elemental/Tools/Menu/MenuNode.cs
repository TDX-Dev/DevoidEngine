namespace Elemental.Tools.Menu
{
    public sealed class MenuNode
    {
        public string Name { get; }
        public MenuItem? MenuItem { get; }

        public List<MenuNode> Children { get; } = [];

        public MenuNode(string name, MenuItem? menuItem = null)
        {
            Name = name;
            MenuItem = menuItem;
        }

        public bool IsLeaf => MenuItem != null;
    }
}
