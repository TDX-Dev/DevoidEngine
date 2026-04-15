using ElementalEditor;
using ElementalEditor.Panels;
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

        foreach (var entry in EditorConsole.Entries)
        {
            Vector4 color = entry.Type switch
            {
                LogType.Error => new Vector4(1f, 0.3f, 0.3f, 1f),
                LogType.Warning => new Vector4(1f, 0.8f, 0.2f, 1f),
                _ => new Vector4(0.8f, 0.8f, 0.8f, 1f)
            };

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(entry.Message);
            ImGui.PopStyleColor();
        }

        ImGui.End();
    }
}