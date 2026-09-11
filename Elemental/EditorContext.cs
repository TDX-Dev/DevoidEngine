using Elemental.Tools.EditorServices;
using Elemental.Tools.Menu;

namespace Elemental
{
    public class EditorContext
    {
        public EditorServiceRegistry Services = null!;
        public PanelManager PanelManager = null!;
        public MenuManager Menu = null!;
    }
}
