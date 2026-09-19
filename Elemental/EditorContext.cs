using DevoidEngine.Nodes;
using Elemental.Tools;
using Elemental.Tools.EditorActions;
using Elemental.Tools.EditorServices;
using Elemental.Tools.Menu;
using Elemental.Tools.Shortcuts;
using Elemental.Tools.Toolbar;
using ImGuiNET;
using System.Numerics;

namespace Elemental
{
    public class EditorContext
    {
        public ImFontPtr DefaultFont;
        public ImFontPtr BoldFont;

        public EditorServiceRegistry Services = null!;
        public PanelManager PanelManager = null!;
        public MenuManager Menu = null!;
        public Toolbar Toolbar = null!;
        public ShortcutManager Shortcuts = null!;
        public EditorActionRegistry EditorActions = null!;
        public SceneService SceneService = null!;

        public EditorCamera? Camera;
        
        public Vector2 MousePosition = new();
        public Node? SelectedNode;

    }
}
