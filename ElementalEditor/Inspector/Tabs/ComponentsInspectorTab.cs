using DevoidEngine.Engine.Core;
using ElementalEditor.Editor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Inspector.Tabs
{
    public class ComponentsInspectorTab : IInspectorTab
    {
        public string Id => "components";
        public string Icon => BootstrapIconFont.GearWide;
        public Vector4 IconColor => new Vector4(216, 235, 246, 255) / 255;

        public void Draw(EditorContext context, GameObject obj)
        {
            InspectorComponentDrawer.Draw(context, obj);
        }
    }
}
