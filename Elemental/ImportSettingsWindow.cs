using DevoidEngine.AssetPipeline;
using DevoidEngine.Core;
using ImGuiNET;
using MessagePack;

namespace Elemental
{
    public class ImportSettingsWindow
    {
        bool open;
        Guid guid;
        string assetPath = string.Empty;

        object settings = null!;
        IAssetImporter importer = null!;
        AssetMeta meta = null!;

        public static string ToRelative(string absolute)
        {
            string root = Engine.Instance.ProjectSystem.AssetPath;

            if (absolute.StartsWith(root))
                return Path.GetRelativePath(root, absolute);

            return absolute;
        }

        public void Open(string path)
        {
            assetPath = ToRelative(path);

            Console.WriteLine(assetPath);

            if (!Engine.Instance.AssetDatabase.TryGetGuid(assetPath, out guid))
                return;

            meta = Engine.Instance.AssetDatabase.GetMeta(guid);

            importer = ImporterRegistry.GetImporter(
                Path.GetExtension(path).ToLower()
            );

            settings = MessagePackSerializer.Deserialize(
                importer.SettingsType,
                meta.Settings
            )!;

            open = true;

            ImGui.OpenPopup("Import Settings");
        }

        public void Draw()
        {
            if (!open)
                return;

            if (!ImGui.BeginPopupModal("Import Settings", ref open))
                return;

            ImGui.Text(assetPath);
            ImGui.Separator();

            bool changed = DrawSettings(settings);

            if (changed)
            {
                meta.Settings = MessagePackSerializer.Serialize(
                    settings.GetType(),
                    settings
                );

                Engine.Instance.AssetDatabase.UpdateMeta(guid, meta);
            }

            ImGui.Spacing();

            if (ImGui.Button("Reimport"))
            {
                Engine.Instance.AssetDatabase.Reimport(guid);
                Engine.Instance.AssetManager.Invalidate(guid);
            }

            ImGui.EndPopup();
        }

        bool DrawSettings(object settings)
        {
            bool changed = false;

            foreach (var field in settings.GetType().GetFields())
            {
                object value = field.GetValue(settings)!;

                if (value is bool b)
                {
                    bool v = b;
                    if (ImGui.Checkbox(field.Name, ref v))
                    {
                        field.SetValue(settings, v);
                        changed = true;
                    }
                }

                else if (value is float f)
                {
                    float v = f;
                    if (ImGui.DragFloat(field.Name, ref v))
                    {
                        field.SetValue(settings, v);
                        changed = true;
                    }
                }

                else if (value is int i)
                {
                    int v = i;
                    if (ImGui.DragInt(field.Name, ref v))
                    {
                        field.SetValue(settings, v);
                        changed = true;
                    }
                }

                else if (value.GetType().IsEnum)
                {
                    int index = (int)value;
                    string[] names = Enum.GetNames(value.GetType());

                    if (ImGui.Combo(field.Name, ref index, names, names.Length))
                    {
                        field.SetValue(settings, Enum.ToObject(value.GetType(), index));
                        changed = true;
                    }
                }
            }

            return changed;
        }
    }
}