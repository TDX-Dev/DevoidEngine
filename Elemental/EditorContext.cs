using Elemental.Tools.EditorActions;
using Elemental.Tools.EditorServices;
using Elemental.Tools.Menu;
using Elemental.Tools.Shortcuts;

namespace Elemental
{
    public class EditorContext
    {
        public EditorServiceRegistry Services = null!;
        public PanelManager PanelManager = null!;
        public MenuManager Menu = null!;
        public ShortcutManager Shortcuts = null!;
        public EditorActionRegistry EditorActions = null!;
    }
}
