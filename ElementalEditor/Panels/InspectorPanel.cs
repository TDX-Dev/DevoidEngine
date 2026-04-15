using DevoidEngine.Engine.Attributes;
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
        enum InspectorTab
        {
            Components,
            Materials
        }

        InspectorTab activeTab = InspectorTab.Components;

        string componentSearch = "";
        List<Component> deleteQueue = new();

        public void Draw(EditorContext context)
        {

            var obj = context.SelectedObject;
            if (obj != null)
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            if (!ImGui.Begin("Inspector"))
            {
                ImGui.End();
                ImGui.PopStyleVar();
                return;
            }

            if (obj == null)
            {
                ImGui.Text("Nothing selected.");
                ImGui.End();
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

            float tabWidth = 40;

            /* LEFT TAB BAR */

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.10f, 0.10f, 0.11f, 1f)); // darker

            ImGui.BeginChild(
                "Tabs",
                new Vector2(tabWidth, 0),
                ImGuiChildFlags.None
            );

            DrawTabs();

            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            ImGui.SameLine();

            /* RIGHT SIDE */

            ImGui.BeginChild(
                "InspectorRight",
                new Vector2(0, 0),
                ImGuiChildFlags.None
            );

            /* HEADER */

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

            ImGui.BeginChild(
                "InspectorHeader",
                new Vector2(0, 45),
                ImGuiChildFlags.AlwaysUseWindowPadding
            );

            ImGui.Text(obj.Name);
            ImGui.Separator();

            ImGui.EndChild();

            ImGui.PopStyleVar();

            /* CONTENT */

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

            ImGui.BeginChild(
                "Content",
                new Vector2(0, 0),
                ImGuiChildFlags.AlwaysUseWindowPadding
            );

            switch (activeTab)
            {
                case InspectorTab.Components:
                    DrawComponents(context, obj);
                    break;

                case InspectorTab.Materials:
                    DrawMaterials(context, obj);
                    break;
            }

            ImGui.EndChild();

            ImGui.PopStyleVar(2);

            ImGui.EndChild();

            ImGui.PopStyleVar();

            ImGui.End();
            ImGui.PopStyleVar();
        }

        void DrawComponents(EditorContext context, GameObject obj)
        {
            deleteQueue.Clear();
            int i = 0;
            foreach (var component in obj.Components)
            {
                ImGui.PushID(component.Type + i);

                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);

                ImGui.BeginChild("component",
                    new Vector2(0, 0),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

                bool open = ImGui.CollapsingHeader(component.GetType().Name,
                    ImGuiTreeNodeFlags.DefaultOpen);

                // right-click menu on header
                if (ImGui.BeginPopupContextItem("ComponentContext"))
                {
                    if (ImGui.MenuItem("Remove Component"))
                        deleteQueue.Add(component);

                    ImGui.EndPopup();
                }

                if (open)
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

            // remove after iteration
            foreach (var comp in deleteQueue)
            {
                obj.RemoveComponent(comp);
                context.SceneDirty = true;
            }
        }
        void DrawAddComponentPopup(EditorContext context, GameObject obj)
        {
            if (!ImGui.BeginPopup("AddComponentPopup"))
                return;

            ImGui.InputText("Search", ref componentSearch, 128);

            ImGui.Separator();

            var assemblies = new List<Assembly>
            {
                typeof(Component).Assembly // engine
            };

            if (ScriptAssemblyLoader.Assembly != null)
                assemblies.Add(ScriptAssemblyLoader.Assembly);

            var componentTypes = new List<Type>();

            // engine components
            componentTypes.AddRange(
                typeof(Component).Assembly
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract)
            );

            // script components
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

        void DrawMaterials(EditorContext context, GameObject obj)
        {
            ImGui.Text("Materials");

            //foreach (var mesh in obj.GetComponents<MeshRenderer>())
            //{
            //    var mat = mesh.Material;

            //    if (mat == null)
            //        continue;

            //    ImGui.Text(mat.Name);
            //}
        }

        void DrawTabs()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10));
            DrawTabButton(BootstrapIconFont.GearWide, InspectorTab.Components);
            DrawTabButton(BootstrapIconFont.Circle, InspectorTab.Materials);
            ImGui.PopStyleVar(2);
        }

        void DrawTabButton(string icon, InspectorTab tab)
        {
            bool active = activeTab == tab;

            Vector4 baseColor = new Vector4(0.14f, 0.14f, 0.15f, 1f);
            Vector4 hoverColor = new Vector4(0.20f, 0.20f, 0.21f, 1f);
            Vector4 activeColor = new Vector4(0.28f, 0.28f, 0.30f, 1f);

            ImGui.PushStyleColor(ImGuiCol.Button, active ? activeColor : baseColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoverColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, activeColor);

            if (active)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.85f, 0.85f, 0.9f, 1f));
            else
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.65f, 0.65f, 0.68f, 1f));

            if (ImGui.Button(icon, new Vector2(-1, 0)))
                activeTab = tab;

            ImGui.PopStyleColor(4);
        }
    }
}