using DevoidEngine.Attributes;
using DevoidEngine.Nodes;
using DevoidEngine.Serialization;
using ImGuiNET;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;

namespace Elemental.Tools.Panels
{
    public class InspectorPanel : Panel
    {
        public InspectorPanel() : base("Inspector") 
        {
            
        }

        protected override void OnPushStyle()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0));
        }

        protected override void OnPopStyle()
        {
            ImGui.PopStyleVar();
        }

        protected override void OnImGuiRender(EditorContext context)
        {
            if (context.SelectedNode == null)
                return;

            const float tableMinHeight = 100.0f;
            Type nodeType = context.SelectedNode.GetType();
            string nodeIcon = LucideIconFont.IconLineDotRightHorizontal;
            NodeIcons.NodeIconMapping.TryGetValue(nodeType, out nodeIcon);


            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(8, 4));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 4));

            ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);

            ImGui.BeginChild("#inspector_table", new Vector2(-1, tableMinHeight), ImGuiChildFlags.AlwaysUseWindowPadding);
            ImGui.PushFont(context.BoldFont);
            if (ImGui.BeginTable("InspectorTable", 2))
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text("Type");
                ImGui.TableNextColumn();
                ImGui.TextColored(new Vector4(0.8f), nodeIcon + " " + nodeType.Name);

                ImGui.TableNextColumn();
                ImGui.Text("Name");
                ImGui.TableNextColumn();
                ImGui.Text(context.SelectedNode.Name);

                ImGui.EndTable();
            }

            float propertiesHeight = ImGui.GetTextLineHeight();
            float bottomY = ImGui.GetWindowHeight() - ImGui.GetStyle().WindowPadding.Y - propertiesHeight;

            ImGui.SetCursorPosY(Math.Max(ImGui.GetCursorPosY(), bottomY));
            ImGui.TextColored(new Vector4(0.8f), "Properties");
            ImGui.PopFont();

            ImGui.EndChild();

            ImGui.PopStyleColor();

            // Uses the normal ChildBg color.
            ImGui.BeginChild("#inspector_subchild", new Vector2(-1, -1), ImGuiChildFlags.AlwaysUseWindowPadding);

            DrawNodeProperties(context, context.SelectedNode);

            ImGui.EndChild();
            ImGui.PopStyleVar(2);
        }

        void DrawNodeProperties(EditorContext context, Node node)
        {
            var type = node.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            EditorUI.BeginPropertyGrid(type.Name);

            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(HideInInspector)))
                    continue;

                if (Attribute.IsDefined(field, typeof(DontSerialize)))
                    continue;

                EditorUI.BeginProperty(field.Name);

                bool changed = EditorUI.DrawGenericField(field, node);

                if (changed)
                    context.SceneService.SceneDocument?.MarkDirty();

                EditorUI.EndProperty();
            }

            foreach (var prop in props)
            {
                if (Attribute.IsDefined(prop, typeof(HideInInspector)))
                    continue;

                if (Attribute.IsDefined(prop, typeof(DontSerialize)))
                    continue;

                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                EditorUI.BeginProperty(prop.Name);

                bool changed = EditorUI.DrawGenericProperty(prop, node);

                if (changed)
                    context.SceneService.SceneDocument?.MarkDirty();

                EditorUI.EndProperty();
            }

            EditorUI.EndPropertyGrid();

        }
    }
}
