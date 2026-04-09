using ElementalEditor.Utils;
using ImGuiNET;
using System;
using System.Linq;
using System.Reflection;
using System.Numerics;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Components;

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

            DrawComponents(obj);

            ImGui.Spacing();

            if (ImGui.Button("Add Component", new Vector2(-1, 30)))
                ImGui.OpenPopup("AddComponentPopup");

            DrawAddComponentPopup(obj);

            ImGui.End();
        }

        void DrawComponents(GameObject obj)
        {
            foreach (var component in obj.Components)
            {
                if (ImGui.CollapsingHeader(component.GetType().Name,
                    ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var type = component.GetType();

                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    EditorUI.BeginPropertyGrid(type.Name);

                    foreach (var field in fields)
                    {
                        EditorUI.BeginProperty(field.Name);
                        EditorUI.DrawGenericField(field, component);
                        EditorUI.EndProperty();
                    }

                    foreach (var prop in props)
                    {
                        EditorUI.BeginProperty(prop.Name);
                        EditorUI.DrawGenericProperty(prop, component);
                        EditorUI.EndProperty();
                    }

                    EditorUI.EndPropertyGrid();
                }
            }
        }

        void DrawAddComponentPopup(GameObject obj)
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

                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.EndPopup();
        }
    }
}