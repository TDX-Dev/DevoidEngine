using DevoidEngine.Engine.Attributes;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serialization;
using ElementalEditor.Utils;
using ImGuiNET;
using System.Numerics;
using System.Reflection;

namespace ElementalEditor.Inspector
{
    public static class InspectorComponentDrawer
    {
        static string componentSearch = "";
        static List<Component> deleteQueue = new();

        public static void Draw(EditorContext context, GameObject obj)
        {
            deleteQueue.Clear();

            int i = 0;

            foreach (var component in obj.Components)
            {
                ImGui.PushID(component.Type + i);

                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);

                ImGui.BeginChild(
                    "component",
                    new Vector2(0, 0),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY
                );

                bool open = ImGui.CollapsingHeader(
                    component.GetType().Name,
                    ImGuiTreeNodeFlags.DefaultOpen
                );

                if (ImGui.BeginPopupContextItem("ComponentContext"))
                {
                    if (ImGui.MenuItem("Remove Component"))
                        deleteQueue.Add(component);

                    ImGui.EndPopup();
                }

                if (open)
                {
                    DrawComponentFields(context, component);
                    i++;
                }

                ImGui.EndChild();
                ImGui.PopStyleVar();
                ImGui.PopID();

                ImGui.Spacing();
            }

            if (ImGui.Button("Add Component", new Vector2(-1, 30)))
                ImGui.OpenPopup("AddComponentPopup");

            DrawAddComponentPopup(context, obj);

            foreach (var comp in deleteQueue)
            {
                obj.RemoveComponent(comp);
                context.SceneDirty = true;
            }
        }

        static void DrawComponentFields(EditorContext context, Component component)
        {
            var type = component.GetType();

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

                bool changed = EditorUI.DrawGenericField(field, component);

                if (changed)
                    context.SceneDirty = true;

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

                bool changed = EditorUI.DrawGenericProperty(prop, component);

                if (changed)
                    context.SceneDirty = true;

                EditorUI.EndProperty();
            }

            EditorUI.EndPropertyGrid();
        }

        static void DrawAddComponentPopup(EditorContext context, GameObject obj)
        {
            if (!ImGui.BeginPopup("AddComponentPopup"))
                return;

            ImGui.InputText("Search", ref componentSearch, 128);

            ImGui.Separator();

            var componentTypes = new List<Type>();

            componentTypes.AddRange(
                typeof(Component).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract)
            );

            foreach (var name in ScriptAssemblyLoader.ScriptComponentTypeNames)
            {
                var t = ScriptAssemblyLoader.Assembly?.GetType(name);

                if (t != null)
                    componentTypes.Add(t);
            }

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
