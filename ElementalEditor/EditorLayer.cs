using DevoidEngine.Engine.Core;
using ElementalEditor.Panels;
using ImGuiNET;

namespace ElementalEditor
{
    class EditorLayer : Layer
    {
        EditorContext context;

        List<IEditorPanel> panels;

        public override void OnAttach()
        {
            panels = new List<IEditorPanel>();
            context = new EditorContext();
            panels.Add(new SceneViewportPanel());
        }

        public override void OnGUIRender()
        {
            foreach (var panel in panels)
                panel.Draw(context);
        }
    }
}