using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serialization;
using ElementalEditor.Utils;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace ElementalEditor.Panels
{
    public class InspectorPanel : IEditorPanel
    {
        string componentSearch = "";

        public void Draw(EditorContext context)
        {
            if (!ImGui.Begin("Inspector"))
            {
                ImGui.End();
                return;
            }

            var obj = context.SelectedObject;

            if (obj == null)
            {
                ImGui.Text("Nothing selected.");
                ImGui.End();
                return;
            }

            ImGui.Text(obj.Name);
            ImGui.Separator();

            DrawComponents(context, obj);

            ImGui.Spacing();

            if (ImGui.Button("Add Component", new Vector2(-1, 30)))
                ImGui.OpenPopup("AddComponentPopup");

            DrawAddComponentPopup(context, obj);

            ImGui.End();
        }

        void DrawComponents(EditorContext context, GameObject obj)
        {
            foreach (var component in obj.Components)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);

                ImGui.BeginChild(component.GetType().Name,
                    new Vector2(0, 0),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

                if (ImGui.CollapsingHeader(component.GetType().Name,
                    ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var type = component.GetType();

                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    EditorUI.BeginPropertyGrid(type.Name);

                    foreach (var field in fields)
                    {
                        if (Attribute.IsDefined(field, typeof(DontSerialize)))
                            continue;

                        EditorUI.BeginProperty(field.Name);

                        bool changed = EditorUI.DrawGenericField(field, component);

                        if (changed)
                            context.SceneDirty = true;

                        EditorUI.EndProperty();
                    }

                    foreach (var prop in props)
                    {
                        if (Attribute.IsDefined(prop, typeof(DontSerialize)))
                            continue;

                        if (!prop.CanRead || !prop.CanWrite)
                            continue;

                        EditorUI.BeginProperty(prop.Name);

                        bool changed = EditorUI.DrawGenericProperty(prop, component);

                        if (changed)
                            context.SceneDirty = true;

                        EditorUI.EndProperty();
                    }

                    EditorUI.EndPropertyGrid();
                }

                ImGui.EndChild();

                ImGui.PopStyleVar();
            }
        }

        void DrawAddComponentPopup(EditorContext context, GameObject obj)
        {
            if (!ImGui.BeginPopup("AddComponentPopup"))
                return;

            ImGui.InputText("Search", ref componentSearch, 128);

            ImGui.Separator();

            var componentTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsSubclassOf(typeof(Component)) &&
                    !t.IsAbstract)
                .OrderBy(t => t.Name);

            foreach (var type in componentTypes)
            {
                if (!string.IsNullOrWhiteSpace(componentSearch) &&
                    !type.Name.Contains(componentSearch, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.MenuItem(type.Name))
                {
                    var component = (Component)Activator.CreateInstance(type);

                    obj.AddComponent(component);

                    context.SceneDirty = true;

                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.EndPopup();
        }
    }
}