using DevoidEngine.Assets;
using DevoidEngine.Components;
using DevoidEngine.Logging;
using DevoidEngine.Metadata;
using DevoidEngine.Serialization;
using System.Text;

namespace DevoidEngine.Core
{
    public static class ResourceFormatText
    {
        public static void Save(string path, Scene scene)
        {
            StringBuilder sb = new();

            sb.AppendLine($"[dv_scene id={scene.Guid.ToString("N")}]");

            sb.AppendLine();

            for (int i = 0; i < scene.GameObjects.Count; i++)
            {
                GameObject go = scene.GameObjects[i];
                sb.AppendLine($"[gameobject name=\"{go.Name}\" id=\"{go.Id.ToString("N")}\" parent=\"{go.Parent?.Id.ToString("N") ?? Guid.Empty.ToString("N")}\"]");
                sb.AppendLine();

                for (int j = 0; j < go.Components.Count; j++)
                {
                    Component component = go.Components[j];
                    ClassInfo? info = ClassDB.GetClass(component.GetType());
                    if (info == null)
                    {
                        DevoidLog.Warning(LogCategory.Serialization, $"Component ({component.Type}) was unable to be serialized as class metadata was not generated in ClassDB.");
                        continue;
                    }
                    sb.AppendLine($"[component type=\"{ClassDB.GetClass(component.GetType())?.AssemblyQualifiedName}\"]");

                    PropertyInfo[] properties = info.Properties;

                    for (int k = 0; k < properties.Length; k++)
                    {
                        PropertyInfo property = properties[k];
                        if (property.Name == "Type" || property.Name == "TickMode")
                            continue;

                        string serializedValue = string.Empty;

                        if (ValueWriter.IsValueSerializable(property.PropertyType))
                        {

                            object? propertyvalue = property.Getter(component);
                            if (propertyvalue == null)
                                continue;
                            serializedValue = ValueWriter.SerializeType(property.PropertyType, propertyvalue);
                            
                        } else if (typeof(AssetType).IsAssignableFrom(property.PropertyType))
                        {
                            serializedValue = "ASSETTYPE";
                        }



                        sb.AppendLine($"{property.Name}={serializedValue}");


                    }

                    sb.AppendLine();
                }
            }

            ClassDB.PrintAllTypes();
            
            File.WriteAllText(Path.Join(Engine.Instance.ProjectSystem.AssetPath, path), sb.ToString());
        }


    }
}
