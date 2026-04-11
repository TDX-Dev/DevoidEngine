using DevoidEngine.Engine.Core;
using ImGuiNET;
using System.Numerics;

namespace ElementalEditor.Panels
{
    public class HierarchyPanel : IEditorPanel
    {
        private List<GameObject> deleteQueue = new();

        public void Draw(EditorContext context)
        {
            if (!ImGui.Begin("Hierarchy"))
            {
                ImGui.End();
                return;
            }

            if (context.Scene == null)
            {
                ImGui.End();
                return;
            }

            // Card container like project settings window
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);

            ImGui.BeginChild("HierarchyContent",
                new System.Numerics.Vector2(0, 0),
                ImGuiChildFlags.Borders);

            DrawContextMenu(context);

            foreach (var obj in context.Scene.GameObjects)
            {
                if (obj.parentObject == null)
                    DrawGameObjectNode(obj, context);
            }

            ImGui.EndChild();

            ImGui.PopStyleVar(2);

            foreach (var obj in deleteQueue)
            {
                context.Scene.DestroyGameObject(obj);

                if (context.SelectedObject == obj)
                    context.SelectedObject = null;
            }

            deleteQueue.Clear();

            ImGui.End();
        }

        void DrawGameObjectNode(GameObject obj, EditorContext context)
        {
            bool selected = context.SelectedObject == obj;

            ImGuiTreeNodeFlags flags =
                ImGuiTreeNodeFlags.OpenOnArrow |
                ImGuiTreeNodeFlags.SpanFullWidth |
                ImGuiTreeNodeFlags.FramePadding;

            if (selected)
                flags |= ImGuiTreeNodeFlags.Selected;

            if (obj.children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;

            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.23f, 0.23f, 0.24f, 1f));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.28f, 0.28f, 0.29f, 1f));

            bool opened = ImGui.TreeNodeEx(obj.GetHashCode().ToString(), flags, obj.Name);

            ImGui.PopStyleColor(2);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                context.SelectedObject = obj;

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                FocusObject(context, obj);
            }

            HandleDragDrop(obj, context);

            HandleObjectContextMenu(obj, context);

            if (opened)
            {
                foreach (var child in obj.children)
                    DrawGameObjectNode(child, context);

                ImGui.TreePop();
            }
        }

        unsafe void HandleDragDrop(GameObject obj, EditorContext context)
        {
            if (ImGui.BeginDragDropSource())
            {
                ImGui.SetDragDropPayload("DND_GAMEOBJECT", IntPtr.Zero, 0);
                context.SelectedObject = obj;
                ImGui.Text(obj.Name);
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("DND_GAMEOBJECT");

                if (payload.NativePtr != null && context.SelectedObject != null)
                {
                    context.SelectedObject.SetParent(obj);
                }

                ImGui.EndDragDropTarget();
            }
        }

        void HandleObjectContextMenu(GameObject obj, EditorContext context)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Create Child"))
                {
                    var child = context.Scene.AddGameObject("GameObject");
                    child.SetParent(obj);
                }

                if (ImGui.MenuItem("Delete"))
                {
                    deleteQueue.Add(obj);
                }

                ImGui.EndPopup();
            }
        }

        void DrawContextMenu(EditorContext context)
        {
            if (ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
            {
                if (ImGui.MenuItem("Create Empty"))
                {
                    context.Scene.AddGameObject("GameObject");
                }

                ImGui.EndPopup();
            }
        }

        void FocusObject(EditorContext context, GameObject obj)
        {
            var cam = context.EditorCamera;

            Vector3 target = obj.Transform.Position;
            Vector3 scale = obj.Transform.Scale;

            float radius = MathF.Max(scale.X, MathF.Max(scale.Y, scale.Z));

            float distance = radius * 3f + 1f;

            Vector3 offset =
                -cam.GetForward() * distance +
                Vector3.UnitY * radius;

            cam.Position = target + offset;

            Vector3 dir = Vector3.Normalize(target - cam.Position);

            cam.Yaw = MathF.Atan2(dir.Z, dir.X) * (180f / MathF.PI);
            cam.Pitch = MathF.Asin(dir.Y) * (180f / MathF.PI);

            cam.UpdateView();
        }
    }
}