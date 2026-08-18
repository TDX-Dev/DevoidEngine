using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Serialization;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Elemental.Panels
{
    public class HierarchyPanel : Panel
    {
        private readonly EditorContext _context;

        private readonly List<GameObject> deleteQueue = [];
        private readonly List<GameObject> duplicateQueue = [];
        private GCHandle? dragHandle;

        public HierarchyPanel(EditorContext context) : base("Hierarchy")
        {
            _context = context;
        }

        protected unsafe override void OnImGuiRender()
        {
            if (_context.ActiveScene == null)
                return;

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);

            ImGui.BeginChild("HierarchyContent", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DrawContextMenu();

            ImGuiTreeNodeFlags sceneFlags = ImGuiTreeNodeFlags.DefaultOpen |
                                            ImGuiTreeNodeFlags.OpenOnArrow |
                                            ImGuiTreeNodeFlags.SpanFullWidth |
                                            ImGuiTreeNodeFlags.FramePadding;

            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.23f, 0.23f, 0.24f, 1f));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.28f, 0.28f, 0.29f, 1f));

            string sceneName = _context.ActiveDocument?.DisplayName ?? "Empty Scene";
            if (_context.ActiveDocument!.IsDirty)
                sceneName += " *";
            bool sceneOpened = ImGui.TreeNodeEx($"{LucideIconFont.IconTriangle} {sceneName}###SceneRoot", sceneFlags);

            ImGui.PopStyleColor(2);

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("OBJECT_REF");
                if (payload.NativePtr != null)
                {
                    unsafe
                    {
                        IntPtr ptr = *(IntPtr*)payload.Data;
                        var handle = GCHandle.FromIntPtr(ptr);

                        if (handle.Target is GameObject dragged)
                        {
                            dragged.SetParent(null);
                        }

                        handle.Free();
                    }
                }
                ImGui.EndDragDropTarget();
            }

            if (sceneOpened)
            {
                foreach (var obj in _context.ActiveScene.GameObjects)
                {
                    if (obj.Parent == null)
                    {
                        DrawGameObjectNode(obj);
                    }
                }

                ImGui.TreePop();
            }

            ImGui.EndChild();
            ImGui.PopStyleVar(2);

            foreach (var obj in deleteQueue)
            {
                _context.ActiveScene.RemoveGameObject(obj);

                if (_context.SelectedObject == obj)
                    _context.SelectedObject = null;
            }

            foreach (var obj in duplicateQueue)
            {
                var clone = DuplicateRecursive(_context.ActiveScene, obj);
                clone.SetParent(obj.Parent);
            }

            if (duplicateQueue.Count > 0)
            {
                GameObjectSerializer.ResolveComponentReferences(_context.ActiveScene);
                GameObjectSerializer.ResolveGameObjectReferences(_context.ActiveScene);
            }

            duplicateQueue.Clear();
            deleteQueue.Clear();
        }

        private void DrawGameObjectNode(GameObject obj)
        {
            bool selected = _context.SelectedObject == obj;

            ImGuiTreeNodeFlags flags =
                ImGuiTreeNodeFlags.OpenOnArrow |
                ImGuiTreeNodeFlags.SpanFullWidth |
                ImGuiTreeNodeFlags.FramePadding;

            if (selected)
                flags |= ImGuiTreeNodeFlags.Selected;

            if (obj.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;

            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.23f, 0.23f, 0.24f, 1f));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.28f, 0.28f, 0.29f, 1f));

            // Uniform GameObject label formatting without individual component icons
            string label = $"{LucideIconFont.IconBox} {obj.Name}###{obj.GetHashCode()}";

            bool opened = ImGui.TreeNodeEx(label, flags);

            ImGui.PopStyleColor(2);

            if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                _context.SelectedObject = obj;
            }

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                _context.EditorCamera?.FocusObject(obj);
            }

            HandleDragDrop(obj);

            HandleObjectContextMenu(obj);

            if (opened)
            {
                for (int i = 0; i < obj.Children.Count; i++)
                {
                    var child = obj.Children[i];
                    DrawGameObjectNode(child);
                }

                ImGui.TreePop();
            }
        }

        private unsafe void HandleDragDrop(GameObject obj)
        {
            if (ImGui.BeginDragDropSource())
            {
                dragHandle = GCHandle.Alloc(obj);
                IntPtr ptr = GCHandle.ToIntPtr(dragHandle.Value);

                ImGui.SetDragDropPayload(
                    "OBJECT_REF",
                    (IntPtr)(&ptr),
                    (uint)IntPtr.Size
                );

                ImGui.Text(obj.Name);
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("OBJECT_REF");

                if (payload.NativePtr != null)
                {
                    IntPtr ptr = *(IntPtr*)payload.Data;
                    var handle = GCHandle.FromIntPtr(ptr);

                    if (handle.Target is GameObject dragged &&
                        dragged != obj &&
                        !IsDescendant(dragged, obj))
                    {
                        dragged.SetParent(obj);
                    }

                    handle.Free();
                }

                ImGui.EndDragDropTarget();
            }
        }

        private void HandleObjectContextMenu(GameObject obj)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Create Child"))
                {
                    var child = _context.ActiveScene!.AddGameObject("GameObject");
                    child.SetParent(obj);
                }

                if (ImGui.MenuItem("Duplicate"))
                {
                    duplicateQueue.Add(obj);
                }

                if (ImGui.MenuItem("Delete"))
                {
                    deleteQueue.Add(obj);
                }

                ImGui.EndPopup();
            }
        }

        private void DrawContextMenu()
        {
            if (ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
            {
                if (ImGui.MenuItem("Create Empty"))
                {
                    _context.ActiveScene?.AddGameObject("GameObject");
                }

                ImGui.EndPopup();
            }
        }

        public static GameObject DuplicateGameObject(Scene scene, GameObject original)
        {
            var data = GameObjectSerializer.Serialize(original);
            data.Id = Guid.NewGuid();
            data.Parent = Guid.Empty;

            Dictionary<Guid, GameObject> map = [];
            var clone = GameObjectSerializer.Deserialize(data, scene, map);

            scene.AddGameObject(clone);
            return clone;
        }

        public static GameObject DuplicateRecursive(Scene scene, GameObject original)
        {
            var clone = DuplicateGameObject(scene, original);

            foreach (var child in original.Children)
            {
                var childClone = DuplicateRecursive(scene, child);
                childClone.SetParent(clone, false);
            }

            return clone;
        }

        private bool IsDescendant(GameObject parent, GameObject candidate)
        {
            var current = candidate;

            while (current != null)
            {
                if (current == parent)
                    return true;

                current = current.Parent;
            }

            return false;
        }

    }
}