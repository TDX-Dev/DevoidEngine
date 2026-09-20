using Elemental.Tools.EditorActions;
using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Toolbar
{
    public sealed class ToolbarGroup
    {
        public string Title { get; }

        private readonly List<EditorActionId> actions = [];

        public ToolbarGroup(string title)
        {
            Title = title;
        }

        public ToolbarGroup AddAction(EditorActionId action)
        {
            actions.Add(action);
            return this;
        }

        public void Draw(EditorContext context)
        {
            EditorActionRegistry registry = context.EditorActions;

            ImGui.BeginGroup();

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4);
            ImGui.BeginChild($"##ToolbarGroup_{Title}", new Vector2(0, -1), ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeX);

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(new Vector4(0.8f),Title);
            ImGui.SameLine(0, 6);

            for (int i = 0; i < actions.Count; i++)
            {
                if (!registry.TryGet(actions[i], out EditorAction? action))
                    continue;

                DrawAction(action!);

                if (i < actions.Count - 1)
                    ImGui.SameLine(0, 2);
            }

            ImGui.EndChild();
            ImGui.PopStyleVar();

            ImGui.EndGroup();
        }

        private static void DrawAction(EditorAction action)
        {
            ImGui.BeginDisabled(!action.Enabled);

            string id = $"##ToolbarAction_{action.Id}";

            string buttonText;

            if (!string.IsNullOrEmpty(action.Icon))
            {
                buttonText = action.Icon;

                if (!string.IsNullOrEmpty(action.Label))
                    buttonText += $" {action.Label}";
            }
            else
            {
                buttonText = action.Label;
            }

            buttonText += id;

            bool clicked = ImGui.Button(buttonText);

            if (clicked)
                action.Execute();

            if (ImGui.IsItemHovered())
            {
                string tooltip = action.Name;

                if (action.Shortcut.HasValue)
                    tooltip += $"\n{action.Shortcut.Value}";

                ImGui.SetTooltip(tooltip);
            }

            ImGui.EndDisabled();
        }
    }
}