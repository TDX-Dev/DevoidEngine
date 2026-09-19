using DevoidEngine.Core;
using DevoidEngine.Nodes;
using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Panels
{
    public class OutlinerPanel : Panel
    {
        private EditorContext? context;
        private Node? selectedNode;

        public Node? SelectedNode => selectedNode;

        public OutlinerPanel()
            : base("Outliner")
        {
            WindowFlags =
                ImGuiWindowFlags.NoScrollbar;
        }

        public override void OnAttach(EditorContext context)
        {
            this.context = context;
        }

        public override void OnDetach()
        {
            context = null;
            selectedNode = null;
        }

        protected override void OnImGuiRender(EditorContext context)
        {
            if (context == null)
                return;


            Scene? scene = context.SceneService.SceneDocument?.Scene;

            if (scene == null)
                return;

            ImGui.BeginChild("#outliner", new Vector2(-1), ImGuiChildFlags.AlwaysUseWindowPadding);
            foreach (Node node in scene.Nodes)
            {
                if (node.Parent == null)
                {
                    DrawNode(node);
                }
            }
            ImGui.EndChild();
        }

        private void DrawToolbar()
        {
            if (ImGui.Button("Add"))
            {
                ImGui.OpenPopup("OutlinerAddPopup");
            }

            ImGui.SameLine();

            if (ImGui.Button("Search"))
            {
                ImGui.SetKeyboardFocusHere();
            }
        }

        private void DrawNode(Node node)
        {
            bool selected =
                selectedNode == node;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding;

            if (selected)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            if (node.Children.Count == 0)
            {
                flags |= ImGuiTreeNodeFlags.Leaf;
            }

            ImGui.PushStyleColor(
                ImGuiCol.HeaderHovered,
                new Vector4(
                    0.23f,
                    0.23f,
                    0.24f,
                    1f));

            ImGui.PushStyleColor(
                ImGuiCol.HeaderActive,
                new Vector4(
                    0.28f,
                    0.28f,
                    0.29f,
                    1f));

            string label = $"{GetNodeIcon(node)} {node.Name}###{node.Id}";

            bool opened =
                ImGui.TreeNodeEx(
                    label,
                    flags);

            ImGui.PopStyleColor(2);

            if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                selectedNode = node;

                if (context != null)
                    context.SelectedNode = node;
            }

            if (ImGui.IsItemHovered() &&
                ImGui.IsMouseDoubleClicked(
                    ImGuiMouseButton.Left))
            {
                FocusNode(node);
            }

            if (opened)
            {
                foreach (Node child in node.Children)
                {
                    DrawNode(child);
                }

                ImGui.TreePop();
            }
        }

        private void FocusNode(Node node)
        {
            if (context?.Camera == null)
                return;

            if (node is Node3D node3D)
            {
                context.Camera.Focus(node3D.Transform.Position);
            }
        }

        private static string GetNodeIcon(Node node)
        {
            string nodeIcon = LucideIconFont.IconLineDotRightHorizontal;
            NodeIcons.NodeIconMapping.TryGetValue(node.GetType(), out nodeIcon);

            return nodeIcon;
        }
    }
}