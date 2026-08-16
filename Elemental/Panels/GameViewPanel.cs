using DevoidEngine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Panels
{
    public class GameViewPanel : ViewportPanel
    {
        public GameViewPanel(Scene activeScene) : base("Game View", activeScene)
        {
            // Leaves Viewport.OverrideCamera = null, automatically renders Scene.MainCamera
        }

        protected override void OnViewportOverlayRender()
        {
        }
    }
}
