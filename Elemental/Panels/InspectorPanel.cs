using DevoidEngine.Attributes;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Serialization;
using Elemental;
using Elemental.Util;
using ImGuiNET;
using System.Numerics;
using System.Reflection;

namespace Elemental.Panels
{
    public class InspectorPanel : Panel
    {
        private readonly EditorContext _context;

        private string componentSearch = "";
        private readonly List<Component> deleteQueue = [];

        public InspectorPanel(EditorContext context) : base("Inspector")
        {
            _context = context;
        }

        protected override bool OnBeginWindow()
        {
            bool hasSelection = _context.SelectedObject != null;

            if (hasSelection)
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            bool visible = base.OnBeginWindow();

            if (hasSelection && !visible)
                ImGui.PopStyleVar();

            return visible;
        }

        protected override void OnEndWindow()
        {
            base.OnEndWindow();

            if (_context.SelectedObject != null)
                ImGui.PopStyleVar();
        }

        protected override void OnImGuiRender()
        {
            var obj = _context.SelectedObject;

            if (obj == null)
            {
                ImGui.Text("Nothing selected.");
                return;
            }

            /* HEADER */
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

            ImGui.BeginChild(
                "InspectorHeader",
                new Vector2(0, 0),
                ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.AutoResizeY
            );

            string name = obj.Name;

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);

            if (ImGui.InputText("##GameObjectName", ref name, 256))
            {
                obj.Name = name;
                _context.SceneDirty = true;
            }

            ImGui.PopItemWidth();
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

            DrawComponents(_context, obj);

            ImGui.EndChild();

            ImGui.PopStyleVar(2);
        }

        private void DrawComponents(EditorContext context, GameObject obj)
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
                    ImGuiChildFlags.Borders |
                    ImGuiChildFlags.AutoResizeY |
                    ImGuiChildFlags.AlwaysUseWindowPadding,
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse);

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
                }

                ImGui.EndChild();
                ImGui.PopStyleVar();
                ImGui.PopID();

                ImGui.Spacing();

                i++;
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

        private void DrawAddComponentPopup(EditorContext context, GameObject obj)
        {
            if (!ImGui.BeginPopup("AddComponentPopup"))
                return;

            ImGui.InputText("Search", ref componentSearch, 128);

            ImGui.Separator();

            var assemblies = new List<Assembly>
            {
                typeof(Component).Assembly // engine
            };

            var componentTypes = new List<Type>();

            // engine components
            componentTypes.AddRange(
                typeof(Component).Assembly
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract)
            );

            foreach (var type in componentTypes)
            {
                if (!string.IsNullOrWhiteSpace(componentSearch) &&
                    !type.Name.Contains(componentSearch, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.MenuItem(type.Name))
                {
                    var component = (Component)Activator.CreateInstance(type)!;

                    obj.AddComponent(component);

                    context.SceneDirty = true;

                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.EndPopup();
        }
    }
}