using ElementalEditor;
using ElementalEditor.Panels;
using ElementalEditor.Utils;
using ImGuiNET;
using SharpDX.Win32;
using System.Numerics;

public class ConsolePanel : IEditorPanel
{
    public void Draw(EditorContext context)
    {
        if (!ImGui.Begin("Console"))
        {
            ImGui.End();
            return;
        }

        if (ImGui.Button("Clear"))
            EditorConsole.Clear();

        ImGui.Separator();

        bool scrollToBottom = ImGui.GetScrollY() >= ImGui.GetScrollMaxY();

        for (int i = 0; i <  EditorConsole.Entries.Count; i++)
        {
            Vector4 color = EditorConsole.Entries[i].Type switch
            {
                LogType.Error => new Vector4(1f, 0.3f, 0.3f, 1f),
                LogType.Warning => new Vector4(1f, 0.8f, 0.2f, 1f),
                _ => new Vector4(0.8f, 0.8f, 0.8f, 1f)
            };

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(EditorConsole.Entries[i].Message);
            ImGui.PopStyleColor();
        }

        if (scrollToBottom)
            ImGui.SetScrollHereY(1.0f); // jump to bottom

        ImGui.End();
    }
}